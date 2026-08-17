<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Question\QuestionService;
use App\Application\Quiz\QuizService;
use App\Application\QuizFolder\QuizFolderService;
use App\Application\WorkspaceFolder\WorkspaceFolderService;
use App\Domain\Quiz\Quiz;
use App\Domain\QuizFolder\QuizFolder;
use App\Http\Requests\AddQuizQuestionRequest;
use App\Http\Requests\MoveFolderRequest;
use App\Http\Requests\MoveQuizQuestionRequest;
use App\Http\Requests\MoveQuizRequest;
use App\Http\Requests\QuizRequest;
use App\Http\Requests\RenameFolderRequest;
use App\Http\Requests\StoreFolderRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Quiz səhifələri — server-rendered Blade.
 * Sual əməliyyatları JS vasitəsilə web controller üzərindən gedir; buradakı
 * fragment-lər server-rendered olub yalnız lazım olan bölməni yeniləyir.
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

    public function store(QuizRequest $request): RedirectResponse
    {
        $data = $request->validated();
        $data['is_published'] = $request->boolean('is_published');
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

    /** Quiz-in ətraflı görünüşü: məlumat + suallar siyahısı. */
    public function show(int $quiz): View
    {
        $model = $this->quizzes->find($quiz);
        $this->assertAccess($model);
        $formData = $this->quizzes->formData($quiz);

        return view('teacher.quizzes.show', [
            'contentId' => $quiz,
            'quizData' => [
                'title' => $formData['title'] ?? '',
                'description' => $formData['description'] ?? null,
                'is_published' => (bool) ($formData['is_published'] ?? false),
                'workspace' => $model?->content?->workspace?->name,
                'folder' => $model?->folder?->name,
                'created_at' => fmt_date($model?->content?->created_at, 'd M Y H:i'),
            ],
            'questions' => $this->quizzes->questionList($quiz),
        ]);
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

    public function update(QuizRequest $request, int $quiz): RedirectResponse
    {
        $this->assertAccess($this->quizzes->find($quiz));

        $data = $request->validated();
        $data['is_published'] = $request->boolean('is_published');
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

    public function destroy(Request $request, int $quiz): RedirectResponse|JsonResponse
    {
        $this->assertAccess($this->quizzes->find($quiz));

        $this->quizzes->delete($quiz);

        if ($request->expectsJson()) {
            return response()->json(['message' => 'Quiz silindi.']);
        }

        return redirect()->route('teacher.quizzes.index')->with('success', 'Quiz silindi.');
    }

    // ─────────────────────────────────────────────────────────────────────────
    // JSON əməliyyatları — frontend JS bu endpointləri çağırır (web controller).
    // ─────────────────────────────────────────────────────────────────────────

    /** Quiz seçim pəncərələri üçün bütün quizlər + qovluq yolları (JSON). */
    public function picker(): JsonResponse
    {
        return response()->json([
            'quizzes' => $this->quizFolders->quizPicker((int) auth()->id()),
        ]);
    }

    /** Yeni quiz qovluğu yaradır. */
    public function storeFolder(StoreFolderRequest $request): JsonResponse
    {
        $data = $request->validated();

        $folder = $this->quizFolders->createFolder(
            (int) auth()->id(),
            $data['name'],
            $data['parent_id'] ?? null,
        );

        return response()->json([
            'data' => ['id' => (int) $folder->id, 'name' => $folder->name],
        ], 201);
    }

    /** Quiz qovluğunun adını dəyişir. */
    public function renameFolder(RenameFolderRequest $request, int $folderId): JsonResponse
    {
        $this->assertFolderAccess($folderId);

        $data = $request->validated();

        $this->quizFolders->renameFolder((int) auth()->id(), $folderId, $data['name']);

        return response()->json(['message' => 'Qovluq adı yeniləndi.']);
    }

    /** Quiz qovluğunu daşıyır (null → kök). */
    public function moveFolder(MoveFolderRequest $request, int $folderId): JsonResponse
    {
        $this->assertFolderAccess($folderId);

        $data = $request->validated();

        $this->quizFolders->moveFolder((int) auth()->id(), $folderId, $data['parent_id'] ?? null);

        return response()->json(['message' => 'Qovluq daşındı.']);
    }

    /** Quiz qovluğunu silir (quizlər kökə qayıdır). */
    public function destroyFolder(int $folderId): JsonResponse
    {
        $this->assertFolderAccess($folderId);

        $this->quizFolders->deleteFolder((int) auth()->id(), $folderId);

        return response()->json(['message' => 'Qovluq silindi.']);
    }

    /** Quiz-i qovluğa daşıyır (null → kök). */
    public function moveQuizToFolder(MoveQuizRequest $request): JsonResponse
    {
        $data = $request->validated();

        $this->assertAccess($this->quizzes->find((int) $data['content_id']));

        $this->quizFolders->moveQuiz(
            (int) auth()->id(),
            (int) $data['content_id'],
            $data['folder_id'] ?? null,
        );

        return response()->json(['message' => 'Quiz daşındı.']);
    }

    /** Quiz-ə bankdan sual əlavə edir. */
    public function addQuestion(AddQuizQuestionRequest $request, int $quiz): JsonResponse
    {
        $this->assertAccess($this->quizzes->find($quiz));

        $data = $request->validated();

        $this->quizzes->addQuestion($quiz, (int) $data['question_id'], (int) auth()->id());

        return response()->json(['message' => 'Sual əlavə edildi.']);
    }

    /** Sualı quizdən çıxarır (bankda qalır). */
    public function removeQuestion(int $quiz, int $questionId): JsonResponse
    {
        $this->assertAccess($this->quizzes->find($quiz));

        $this->quizzes->removeQuestion($quiz, $questionId, (int) auth()->id());

        return response()->json(['message' => 'Sual çıxarıldı.']);
    }

    /** Quizdəki sualın sırasını dəyişir (yuxarı/aşağı). */
    public function moveQuestion(MoveQuizQuestionRequest $request, int $quiz, int $questionId): JsonResponse
    {
        $this->assertAccess($this->quizzes->find($quiz));

        $data = $request->validated();

        $this->quizzes->moveQuestion($quiz, $questionId, $data['direction'], (int) auth()->id());

        return response()->json(['message' => 'Sıralama yeniləndi.']);
    }

    /** Quiz qovluğunun sahibliyini yoxla. */
    protected function assertFolderAccess(int $folderId): void
    {
        $folder = $this->quizFolders->find($folderId);
        if ($folder === null) {
            abort(404);
        }
        $user = auth()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $folder->teacher_id !== (int) $user->id) {
            abort(403);
        }
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
