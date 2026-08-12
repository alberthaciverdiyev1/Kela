<?php

namespace App\Api\Controllers;

use App\Application\QuestionFolder\QuestionFolderService;
use App\Domain\QuestionFolder\QuestionFolder;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

/**
 * Sual bankı qovluqları üçün API.
 * Web səhifəsi (server-rendered) bu endpointləri JS vasitəsilə çağırır.
 */
class QuestionFolderController
{
    public function __construct(private readonly QuestionFolderService $folders)
    {
    }

    /** Cari qovluğun kataloqu: qovluqlar + suallar (fragment üçün deyil, JSON). */
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

    /** Sualı qovluğa daşıyır (null → kökə). */
    public function moveQuestion(Request $request): JsonResponse
    {
        $data = $request->validate([
            'question_id' => ['required', 'integer'],
            'folder_id' => ['nullable', 'integer'],
        ]);

        $this->folders->moveQuestion(
            (int) $request->user()->id,
            (int) $data['question_id'],
            $data['folder_id'] ?? null,
        );

        return response()->json(['message' => 'Sual daşındı.']);
    }

    private function authorizeAccess(?QuestionFolder $folder, Request $request): void
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
}
