<?php

namespace App\Api\Controllers;

use App\Application\Quiz\QuizService;
use App\Application\QuizFolder\QuizFolderService;
use App\Application\WorkspaceFolder\WorkspaceFolderService;
use App\Domain\Quiz\Quiz;
use App\Api\Resources\QuizResource;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Pagination\LengthAwarePaginator;

class QuizController
{
    public function __construct(
        private readonly QuizService $quizzes,
        private readonly QuizFolderService $quizFolders,
        private readonly WorkspaceFolderService $workspaceFolders,
    ) {
    }

    public function index(Request $request): LengthAwarePaginator
    {
        return $this->quizzes->paginate(
            (int) $request->user()->id,
            $request->string('search')->toString() ?: null,
            (int) $request->integer('folder_id'),
            (int) $request->integer('per_page', 15),
        );
    }

    public function store(Request $request): JsonResponse
    {
        $data = $request->validate([
            'title' => ['required', 'string', 'max:255'],
            'description' => ['nullable', 'string'],
            'is_published' => ['nullable', 'boolean'],
            'folder_id' => ['nullable', 'integer'],
            'workspace_id' => ['nullable', 'integer'],
            'ws_folder_id' => ['nullable', 'integer'],
        ]);

        $data['folder_id'] = $this->quizFolders->resolveFolderFor((int) $request->user()->id, $data['folder_id'] ?? null);

        $context = $this->workspaceFolders->resolveContextFor(
            (int) ($data['workspace_id'] ?? 0) ?: null,
            (int) ($data['ws_folder_id'] ?? 0) ?: null,
            (int) $request->user()->id,
        );
        if ($context !== null) {
            $data['workspace_id'] = $context['workspace_id'];
            $data['ws_folder_id'] = $context['folder_id'];
        }

        $quiz = $this->quizzes->create((int) $request->user()->id, $data);

        return (new QuizResource($quiz))->response()->setStatusCode(201);
    }

    public function show(int $contentId): JsonResponse
    {
        $quiz = $this->quizzes->find($contentId);
        $this->authorizeAccess($quiz);

        return response()->json([
            'data' => new QuizResource($quiz),
            'questions' => $this->quizzes->questionList($contentId),
        ]);
    }

    public function update(Request $request, int $contentId): QuizResource
    {
        $this->authorizeAccess($this->quizzes->find($contentId));

        $data = $request->validate([
            'title' => ['required', 'string', 'max:255'],
            'description' => ['nullable', 'string'],
            'is_published' => ['nullable', 'boolean'],
            'folder_id' => ['nullable', 'integer'],
            'workspace_id' => ['nullable', 'integer'],
            'ws_folder_id' => ['nullable', 'integer'],
        ]);

        $data['folder_id'] = $this->quizFolders->resolveFolderFor((int) $request->user()->id, $data['folder_id'] ?? null);

        $context = $this->workspaceFolders->resolveContextFor(
            (int) ($data['workspace_id'] ?? 0) ?: null,
            (int) ($data['ws_folder_id'] ?? 0) ?: null,
            (int) $request->user()->id,
        );
        if ($context !== null) {
            $data['workspace_id'] = $context['workspace_id'];
            $data['ws_folder_id'] = $context['folder_id'];
        }

        return new QuizResource($this->quizzes->update($contentId, $data));
    }

    public function destroy(int $contentId): JsonResponse
    {
        $this->authorizeAccess($this->quizzes->find($contentId));
        $this->quizzes->delete($contentId);

        return response()->json(['message' => 'Quiz silindi.']);
    }

    public function addQuestion(Request $request, int $contentId): JsonResponse
    {
        $this->authorizeAccess($this->quizzes->find($contentId));

        $data = $request->validate([
            'question_id' => ['required', 'integer'],
        ]);

        $this->quizzes->addQuestion($contentId, (int) $data['question_id'], (int) $request->user()->id);

        return response()->json(['message' => 'Sual əlavə edildi.']);
    }

    public function removeQuestion(Request $request, int $contentId, int $questionId): JsonResponse
    {
        $this->authorizeAccess($this->quizzes->find($contentId));
        $this->quizzes->removeQuestion($contentId, $questionId, (int) $request->user()->id);

        return response()->json(['message' => 'Sual çıxarıldı.']);
    }

    public function moveQuestion(Request $request, int $contentId, int $questionId): JsonResponse
    {
        $this->authorizeAccess($this->quizzes->find($contentId));

        $data = $request->validate([
            'direction' => ['required', 'in:up,down'],
        ]);

        $this->quizzes->moveQuestion($contentId, $questionId, $data['direction'], (int) $request->user()->id);

        return response()->json(['message' => 'Sıralama yeniləndi.']);
    }

    private function authorizeAccess(?Quiz $quiz): void
    {
        if ($quiz === null) {
            abort(404);
        }
        $user = request()->user();
        if ($user->isAdmin()) {
            return;
        }
        if ($quiz->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
