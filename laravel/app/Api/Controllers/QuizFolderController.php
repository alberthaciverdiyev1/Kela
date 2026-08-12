<?php

namespace App\Api\Controllers;

use App\Application\Quiz\QuizService;
use App\Application\QuizFolder\QuizFolderService;
use App\Domain\Quiz\Quiz;
use App\Domain\QuizFolder\QuizFolder;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

/**
 * Quiz qovluqları üçün API.
 * Web səhifəsi (server-rendered) bu endpointləri JS vasitəsilə çağırır.
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

    public function store(Request $request): JsonResponse
    {
        $data = $request->validate([
            'name' => ['required', 'string', 'max:255'],
            'parent_id' => ['nullable', 'integer'],
        ]);

        $folder = $this->folders->createFolder(
            (int) $request->user()->id,
            $data['name'],
            $data['parent_id'] ?? null,
        );

        return response()->json([
            'data' => ['id' => (int) $folder->id, 'name' => $folder->name],
        ], 201);
    }

    public function rename(Request $request, int $folderId): JsonResponse
    {
        $this->authorizeAccess($this->folders->find($folderId), $request);

        $data = $request->validate([
            'name' => ['required', 'string', 'max:255'],
        ]);

        $this->folders->renameFolder((int) $request->user()->id, $folderId, $data['name']);

        return response()->json(['message' => 'Qovluq adı yeniləndi.']);
    }

    public function move(Request $request, int $folderId): JsonResponse
    {
        $this->authorizeAccess($this->folders->find($folderId), $request);

        $data = $request->validate([
            'parent_id' => ['nullable', 'integer'],
        ]);

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
    public function moveQuiz(Request $request): JsonResponse
    {
        $data = $request->validate([
            'content_id' => ['required', 'integer'],
            'folder_id' => ['nullable', 'integer'],
        ]);

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
