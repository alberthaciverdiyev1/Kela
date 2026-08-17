<?php

namespace App\Api\Controllers;

use App\Application\Quiz\QuizService;
use App\Application\QuizFolder\QuizFolderService;
use App\Domain\Quiz\Quiz;
use App\Domain\QuizFolder\QuizFolder;
use App\Http\Requests\MoveFolderRequest;
use App\Http\Requests\MoveQuizRequest;
use App\Http\Requests\RenameFolderRequest;
use App\Http\Requests\StoreFolderRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

/**
 * Quiz qovluqları üçün API.
 * Doğrulama qaydaları FormRequest siniflərindədir — web controller ilə ortaqdır.
 */
class QuizFolderController
{
    public function __construct(
        private readonly QuizFolderService $folders,
        private readonly QuizService $quizzes,
    ) {
    }

    /** Cari qovluğun kataloqu: qovluqlar (JSON). */
    public function directory(Request $request): JsonResponse
    {
        $folderId = $request->integer('folder_id') ?: null;

        return response()->json([
            'data' => $this->folders->directory((int) $request->user()->id, $folderId),
        ]);
    }

    /**
     * Quiz seçim pəncərələri üçün bütün quizlər + qovluq yolları (JSON).
     * Kökdəki və iç-içə qovluqlardakı bütün quizlər qayıdır.
     */
    public function picker(Request $request): JsonResponse
    {
        return response()->json([
            'quizzes' => $this->folders->quizPicker((int) $request->user()->id),
        ]);
    }

    public function store(StoreFolderRequest $request): JsonResponse
    {
        $data = $request->validated();

        $folder = $this->folders->createFolder(
            (int) $request->user()->id,
            $data['name'],
            $data['parent_id'] ?? null,
        );

        return response()->json([
            'data' => ['id' => (int) $folder->id, 'name' => $folder->name],
        ], 201);
    }

    public function rename(RenameFolderRequest $request, int $folderId): JsonResponse
    {
        $this->authorizeAccess($this->folders->find($folderId), $request);

        $data = $request->validated();

        $this->folders->renameFolder((int) $request->user()->id, $folderId, $data['name']);

        return response()->json(['message' => 'Qovluq adı yeniləndi.']);
    }

    public function move(MoveFolderRequest $request, int $folderId): JsonResponse
    {
        $this->authorizeAccess($this->folders->find($folderId), $request);

        $data = $request->validated();

        $this->folders->moveFolder((int) $request->user()->id, $folderId, $data['parent_id'] ?? null);

        return response()->json(['message' => 'Qovluq daşındı.']);
    }

    public function destroy(Request $request, int $folderId): JsonResponse
    {
        $this->authorizeAccess($this->folders->find($folderId), $request);
        $this->folders->deleteFolder((int) $request->user()->id, $folderId);

        return response()->json(['message' => 'Qovluq silindi.']);
    }

    /** Quiz-i qovluğa daşıyır (null → kökə). */
    public function moveQuiz(MoveQuizRequest $request): JsonResponse
    {
        $data = $request->validated();

        $this->authorizeQuizAccess($this->quizzes->find((int) $data['content_id']));

        $this->folders->moveQuiz(
            (int) $request->user()->id,
            (int) $data['content_id'],
            $data['folder_id'] ?? null,
        );

        return response()->json(['message' => 'Quiz daşındı.']);
    }

    private function authorizeAccess(?QuizFolder $folder, Request $request): void
    {
        if ($folder === null) {
            abort(404);
        }
        $user = $request->user();
        if ($user->isAdmin()) {
            return;
        }
        if ($folder->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }

    /** Quiz sahibinə aid deyilsə və ya mövcud deyilsə rədd edir. */
    private function authorizeQuizAccess(?Quiz $quiz): void
    {
        if ($quiz === null) {
            abort(404);
        }
        $user = request()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $quiz->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
