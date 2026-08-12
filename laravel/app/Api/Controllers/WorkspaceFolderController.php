<?php

namespace App\Api\Controllers;

use App\Application\Workspace\WorkspaceService;
use App\Application\WorkspaceFolder\WorkspaceFolderService;
use App\Domain\Content\Content;
use App\Domain\Workspace\Workspace;
use App\Domain\WorkspaceFolder\WorkspaceFolder;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

/**
 * Workspace qovluqları üçün API.
 * Web səhifəsi (server-rendered) bu endpointləri JS vasitəsilə çağırır.
 */
class WorkspaceFolderController
{
    public function __construct(
        private readonly WorkspaceFolderService $folders,
        private readonly WorkspaceService $workspaces,
    ) {
    }

    /** Cari qovluğun kataloqu: qovluqlar + content-lər (JSON). */
    public function directory(Request $request, int $workspace): JsonResponse
    {
        $folderId = $request->integer('folder_id') ?: null;

        return response()->json([
            'data' => $this->folders->directory($workspace, $folderId, (int) $request->user()->id),
        ]);
    }

    /** Teacher-in workspace-ə bağlanmamış quiz/dərsləri (əlavə et dialoqu üçün). */
    public function availableContents(Request $request, int $workspace): JsonResponse
    {
        $this->authorizeWorkspace($this->workspaces->find($workspace), $request);

        return response()->json([
            'data' => $this->folders->availableContents((int) $request->user()->id),
        ]);
    }

    public function store(Request $request, int $workspace): JsonResponse
    {
        $this->authorizeWorkspace($this->workspaces->find($workspace), $request);

        $data = $request->validate([
            'name' => ['required', 'string', 'max:255'],
            'parent_id' => ['nullable', 'integer'],
        ]);

        $folder = $this->folders->createFolder(
            $workspace,
            $data['name'],
            $data['parent_id'] ?? null,
            (int) $request->user()->id,
        );

        return response()->json([
            'data' => ['id' => (int) $folder->id, 'name' => $folder->name],
        ], 201);
    }

    public function rename(Request $request, int $workspace, int $folderId): JsonResponse
    {
        $this->authorizeWorkspace($this->workspaces->find($workspace), $request);
        $this->authorizeFolder($this->folders->find($folderId), $workspace);

        $data = $request->validate([
            'name' => ['required', 'string', 'max:255'],
        ]);

        $this->folders->renameFolder($workspace, $folderId, $data['name'], (int) $request->user()->id);

        return response()->json(['message' => 'Qovluq adı yeniləndi.']);
    }

    public function move(Request $request, int $workspace, int $folderId): JsonResponse
    {
        $this->authorizeWorkspace($this->workspaces->find($workspace), $request);
        $this->authorizeFolder($this->folders->find($folderId), $workspace);

        $data = $request->validate([
            'parent_id' => ['nullable', 'integer'],
        ]);

        $this->folders->moveFolder($workspace, $folderId, $data['parent_id'] ?? null, (int) $request->user()->id);

        return response()->json(['message' => 'Qovluq daşındı.']);
    }

    public function destroy(Request $request, int $workspace, int $folderId): JsonResponse
    {
        $this->authorizeWorkspace($this->workspaces->find($workspace), $request);
        $this->authorizeFolder($this->folders->find($folderId), $workspace);

        $this->folders->deleteFolder($workspace, $folderId, (int) $request->user()->id);

        return response()->json(['message' => 'Qovluq silindi.']);
    }

    /** Bank qovluğunu (içindəki məzmunlarla birlikdə) workspace-ə əlavə edir. */
    public function addFolder(Request $request): JsonResponse
    {
        $data = $request->validate([
            'folder_type' => ['required', 'string', 'in:quiz,lesson'],
            'bank_folder_id' => ['required', 'integer'],
            'workspace_id' => ['required', 'integer'],
            'folder_id' => ['nullable', 'integer'],
        ]);

        $this->authorizeWorkspace($this->workspaces->find((int) $data['workspace_id']), $request);

        $result = $this->folders->addFolderToWorkspace(
            $data['folder_type'],
            (int) $data['bank_folder_id'],
            (int) $data['workspace_id'],
            $data['folder_id'] ?? null,
            (int) $request->user()->id,
        );

        return response()->json([
            'message' => 'Qovluq və içindəki məzmunlar əlavə edildi.',
            'folders' => $result['folders'],
            'contents' => $result['contents'],
        ]);
    }

    /** Content-i workspace qovluğuna daşıyır (null → workspace kökü). */
    public function moveContent(Request $request): JsonResponse
    {
        $data = $request->validate([
            'content_id' => ['required', 'integer'],
            'workspace_id' => ['nullable', 'integer'],
            'folder_id' => ['nullable', 'integer'],
        ]);

        $this->authorizeContentAccess($this->folders->findContent((int) $data['content_id']));

        $this->folders->moveContent(
            (int) $data['content_id'],
            $data['workspace_id'] ?? null,
            $data['folder_id'] ?? null,
            (int) $request->user()->id,
        );

        return response()->json(['message' => 'Content daşındı.']);
    }

    private function authorizeWorkspace(?Workspace $workspace, Request $request): void
    {
        if ($workspace === null) {
            abort(404);
        }
        $user = $request->user();
        if ($user->isAdmin()) {
            return;
        }
        if ($workspace->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }

    private function authorizeFolder(?WorkspaceFolder $folder, int $workspaceId): void
    {
        if ($folder === null || $folder->workspace_id !== $workspaceId) {
            abort(404);
        }
    }

    /** Content sahibə aid deyilsə və ya mövcud deyilsə rədd edir. */
    private function authorizeContentAccess(?Content $content): void
    {
        if ($content === null) {
            abort(404);
        }
        $user = request()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $content->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
