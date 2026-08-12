<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Question\QuestionService;
use App\Application\Quiz\QuizService;
use App\Application\QuizFolder\QuizFolderService;
use App\Application\WorkspaceFolder\WorkspaceFolderService;
use App\Domain\Quiz\Quiz;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Quiz səhifələri — server-rendered Blade.
 * Sual əməliyyatları JS vasitəsilə /api/v1-ə gedir; buradakı fragment-lər
 * server-rendered olub yalnız lazım olan bölməni yeniləyir.
 */
class QuizController
{
    public function __construct(
        private readonly QuizService $quizzes,
        private readonly QuizFolderService $quizFolders,
        private readonly WorkspaceFolderService $workspaceFolders,
        private readonly QuestionService $questions,
    ) {
    }

    public function index(Request $request): View
    {
        $search = (string) $request->string('search');
        $folderId = (int) $request->integer('folder_id');

        return view('teacher.quizzes.index', [
            'quizzes' => $this->quizzes->paginate((int) auth()->id(), $search ?: null, $folderId, 15),
            'search' => $search,
            'folderId' => $folderId,
            'folders' => $this->quizFolders->directory((int) auth()->id(), $folderId ?: null),
            'folderTree' => $this->quizFolders->folderTree((int) auth()->id()),
        ]);
    }

    public function create(Request $request): View
    {
        $selectedFolder = $this->quizFolders->resolveFolderFor((int) auth()->id(), (int) $request->integer('folder_id') ?: null);

        return view('teacher.quizzes.form', [
            'heading' => 'Yeni Quiz',
            'subtitle' => 'Yeni quiz əlavə et',
            'creating' => true,
            'quiz' => null,
            'folderTree' => $this->quizFolders->folderTree((int) auth()->id()),
            'selectedFolderId' => $selectedFolder,
            'workspaceContext' => $this->workspaceFolders->resolveContextFor(
                (int) $request->integer('workspace_id') ?: null,
                (int) $request->integer('ws_folder_id') ?: null,
                (int) auth()->id(),
            ),
        ]);
    }

    public function store(Request $request): RedirectResponse
    {
        $data = $this->validated($request);
        $data['folder_id'] = $this->quizFolders->resolveFolderFor((int) auth()->id(), $data['folder_id'] ?? null);

        $context = $this->workspaceFolders->resolveContextFor(
            (int) $request->integer('workspace_id') ?: null,
            (int) $request->integer('ws_folder_id') ?: null,
            (int) auth()->id(),
        );
        if ($context !== null) {
            $data['workspace_id'] = $context['workspace_id'];
            $data['ws_folder_id'] = $context['folder_id'];
        }

        $quiz = $this->quizzes->create((int) auth()->id(), $data);

        return redirect()
            ->route('teacher.quizzes.edit', $quiz->getKey())
            ->with('success', 'Quiz yaradıldı — sual əlavə etmək üçün redaktoru açın.');
    }

    public function edit(int $quiz): View
    {
        $this->assertAccess($this->quizzes->find($quiz));
        $formData = $this->quizzes->formData($quiz);

        return view('teacher.quizzes.edit', [
            'contentId' => $quiz,
            'quiz' => $formData,
            'questions' => $this->quizzes->questionList($quiz),
            'bankOptions' => $this->bankOptions($quiz),
            'folderTree' => $this->quizFolders->folderTree((int) auth()->id()),
            'workspaceContext' => $this->workspaceFolders->resolveContextFor(
                (int) ($formData['workspace_id'] ?? 0) ?: null,
                (int) ($formData['ws_folder_id'] ?? 0) ?: null,
                (int) auth()->id(),
            ),
        ]);
    }

    /** JS-in sual siyahısını yeniləməsi üçün server-rendered fragment. */
    public function questionsFragment(int $quiz): View
    {
        $this->assertAccess($this->quizzes->find($quiz));

        return view('teacher.quizzes._questions', [
            'contentId' => $quiz,
            'questions' => $this->quizzes->questionList($quiz),
        ]);
    }

    public function update(Request $request, int $quiz): RedirectResponse
    {
        $this->assertAccess($this->quizzes->find($quiz));

        $data = $this->validated($request);
        $data['folder_id'] = $this->quizFolders->resolveFolderFor((int) auth()->id(), $data['folder_id'] ?? null);

        $context = $this->workspaceFolders->resolveContextFor(
            (int) $request->integer('workspace_id') ?: null,
            (int) $request->integer('ws_folder_id') ?: null,
            (int) auth()->id(),
        );
        if ($context !== null) {
            $data['workspace_id'] = $context['workspace_id'];
            $data['ws_folder_id'] = $context['folder_id'];
        }

        $this->quizzes->update($quiz, $data);

        return redirect()->route('teacher.quizzes.edit', $quiz)->with('success', 'Quiz yeniləndi.');
    }

    public function destroy(int $quiz): RedirectResponse
    {
        $this->assertAccess($this->quizzes->find($quiz));

        $this->quizzes->delete($quiz);

        return redirect()->route('teacher.quizzes.index')->with('success', 'Quiz silindi.');
    }

    protected function validated(Request $request): array
    {
        $data = $request->validate([
            'title' => ['required', 'string', 'max:200'],
            'description' => ['nullable', 'string'],
            'is_published' => ['nullable', 'boolean'],
            'folder_id' => ['nullable', 'integer'],
            'workspace_id' => ['nullable', 'integer'],
            'ws_folder_id' => ['nullable', 'integer'],
        ]);

        $data['is_published'] = $request->boolean('is_published');

        return $data;
    }

    /** Sual bankındakı mövcud suallar (quizə əlavə olunmayanlar): [id => text]. */
    protected function bankOptions(int $contentId): array
    {
        $options = [];

        foreach ($this->quizzes->availableQuestionIds($contentId, (int) auth()->id()) as $id) {
            $data = $this->questions->formData($id);
            if ($data !== []) {
                $options[$id] = $data['text'];
            }
        }

        return $options;
    }

    protected function assertAccess(?Quiz $quiz): void
    {
        if ($quiz === null) {
            abort(404);
        }
        $user = auth()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $quiz->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
