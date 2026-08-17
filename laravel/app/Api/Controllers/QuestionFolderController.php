<?php

namespace App\Api\Controllers;

use App\Application\QuestionFolder\QuestionFolderService;
use App\Domain\QuestionFolder\QuestionFolder;
use App\Http\Requests\MoveFolderRequest;
use App\Http\Requests\MoveQuestionRequest;
use App\Http\Requests\RenameFolderRequest;
use App\Http\Requests\StoreFolderRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

/**
 * Sual bankı qovluqları üçün API.
 * Doğrulama qaydaları FormRequest siniflərindədir — web controller ilə ortaqdır.
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

    /** Sualı qovluğa daşıyır (null → kökə). */
    public function moveQuestion(MoveQuestionRequest $request): JsonResponse
    {
        $data = $request->validated();

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
