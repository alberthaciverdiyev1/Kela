<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Question\QuestionService;
use App\Application\QuestionFolder\QuestionFolderService;
use App\Domain\Question\Question;
use App\Domain\QuestionFolder\QuestionFolder;
use App\Http\Requests\MoveFolderRequest;
use App\Http\Requests\MoveQuestionRequest;
use App\Http\Requests\QuestionRequest;
use App\Http\Requests\RenameFolderRequest;
use App\Http\Requests\StoreFolderRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Sual bankı səhifələri — server-rendered Blade.
 * Kataloq (qovluq + sual) GET ilə server tərəfindən render olunur;
 * qovluq/sual əməliyyatları da web controller üzərindən (JSON) aparılır —
 * frontend /api/v1-ə birbaşa toxunmur.
 */
class QuestionController
{
    public function __construct(
        private readonly QuestionService $questions,
        private readonly QuestionFolderService $folders,
    ) {
    }

    public function index(Request $request): View
    {
        $folderId = $request->integer('folder_id') ?: null;
        $this->assertAccess($folderId);

        $dir = $this->folders->directory((int) auth()->id(), $folderId);

        return view('teacher.questions.index', [
            'folderId' => $folderId,
            'folders' => $dir['folders'],
            'questions' => $dir['questions'],
            'breadcrumbs' => $dir['breadcrumbs'],
            'folderTree' => $this->folders->folderTree((int) auth()->id()),
            'fragmentUrl' => route('teacher.questions.table', ['folder_id' => $folderId ?? null]),
        ]);
    }

    /** Sual detay səhifəsi. */
    public function show(int $question): View
    {
        $model = $this->questions->find($question);
        $this->assertQuestionAccess($model);

        return view('teacher.questions.show', [
            'question' => $model,
            'data' => $this->questions->formData($question),
            'usedInQuizzes' => $this->questions->usedInQuizzes($question),
            'folderTree' => $this->folders->folderTree((int) auth()->id()),
        ]);
    }

    /** JS-in kataloqu yeniləməsi üçün server-rendered fragment. */
    public function tableFragment(Request $request): View
    {
        $folderId = $request->integer('folder_id') ?: null;
        $this->assertAccess($folderId);

        $dir = $this->folders->directory((int) auth()->id(), $folderId);

        return view('teacher.questions._table', [
            'folderId' => $folderId,
            'folders' => $dir['folders'],
            'questions' => $dir['questions'],
            'breadcrumbs' => $dir['breadcrumbs'],
            'folderTree' => $this->folders->folderTree((int) auth()->id()),
        ]);
    }

    /** Sualın sahibliyini yoxla (admin hər zaman, başqası 403/404). */
    protected function assertQuestionAccess(?Question $question): void
    {
        if ($question === null) {
            abort(404);
        }
        $user = auth()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $question->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }

    /** Qovluq verilərsə sahibliyi yoxla (admin hər zaman, başqası 403). */
    protected function assertAccess(?int $folderId): void
    {
        if ($folderId === null) {
            return;
        }
        $folder = $this->folders->find($folderId);
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

    // ─────────────────────────────────────────────────────────────────────────
    // JSON əməliyyatları — frontend JS bu endpointləri çağırır (web controller).
    // ─────────────────────────────────────────────────────────────────────────

    /** Yeni sual yaradır (JS modalı üçün). */
    public function storeJson(QuestionRequest $request): JsonResponse
    {
        $data = $request->validated();

        $data['folder_id'] = $this->folders->resolveFolderFor(
            (int) auth()->id(),
            $data['folder_id'] ?? null,
        );

        $question = $this->questions->create((int) auth()->id(), $data);

        return response()->json([
            'message' => 'Sual yaradıldı.',
            'data' => ['id' => (int) $question->id],
        ], 201);
    }

    /** Sualın məzmununu yeniləyir (JS modalı üçün). */
    public function updateJson(QuestionRequest $request, int $question): JsonResponse
    {
        $model = $this->questions->find($question);
        $this->assertQuestionAccess($model);

        $data = $request->validated();
        $data['folder_id'] = $this->folders->resolveFolderFor(
            (int) auth()->id(),
            $data['folder_id'] ?? null,
        );

        $this->questions->update($question, $data, (int) auth()->id());

        return response()->json(['message' => 'Sual yeniləndi.']);
    }

    /** Sualı silir (JS kontekst menyusu üçün). */
    public function destroyJson(int $question): JsonResponse
    {
        $model = $this->questions->find($question);
        $this->assertQuestionAccess($model);

        $this->questions->delete($question, (int) auth()->id());

        return response()->json(['message' => 'Sual silindi.']);
    }

    /** Yeni qovluq yaradır. */
    public function storeFolder(StoreFolderRequest $request): JsonResponse
    {
        $data = $request->validated();

        $folder = $this->folders->createFolder(
            (int) auth()->id(),
            $data['name'],
            $data['parent_id'] ?? null,
        );

        return response()->json([
            'data' => ['id' => (int) $folder->id, 'name' => $folder->name],
        ], 201);
    }

    /** Qovluq adını dəyişir. */
    public function renameFolder(RenameFolderRequest $request, int $folderId): JsonResponse
    {
        $this->assertFolderAccess($folderId);

        $data = $request->validated();

        $this->folders->renameFolder((int) auth()->id(), $folderId, $data['name']);

        return response()->json(['message' => 'Qovluq adı yeniləndi.']);
    }

    /** Qovluğu başqa qovluğa daşıyır (null → kök). */
    public function moveFolder(MoveFolderRequest $request, int $folderId): JsonResponse
    {
        $this->assertFolderAccess($folderId);

        $data = $request->validated();

        $this->folders->moveFolder((int) auth()->id(), $folderId, $data['parent_id'] ?? null);

        return response()->json(['message' => 'Qovluq daşındı.']);
    }

    /** Qovluğu silir (suallar kökə qayıdır). */
    public function destroyFolder(int $folderId): JsonResponse
    {
        $this->assertFolderAccess($folderId);

        $this->folders->deleteFolder((int) auth()->id(), $folderId);

        return response()->json(['message' => 'Qovluq silindi.']);
    }

    /** Sualı qovluğa daşıyır (null → kök). */
    public function moveQuestionToFolder(MoveQuestionRequest $request): JsonResponse
    {
        $data = $request->validated();

        $this->assertQuestionAccess($this->questions->find((int) $data['question_id']));

        $this->folders->moveQuestion(
            (int) auth()->id(),
            (int) $data['question_id'],
            $data['folder_id'] ?? null,
        );

        return response()->json(['message' => 'Sual daşındı.']);
    }

    /** Qovluğun sahibliyini yoxla. */
    protected function assertFolderAccess(int $folderId): void
    {
        $folder = $this->folders->find($folderId);
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
}
