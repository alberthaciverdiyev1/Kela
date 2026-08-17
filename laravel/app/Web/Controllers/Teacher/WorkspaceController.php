<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Workspace\WorkspaceService;
use App\Application\WorkspaceFolder\WorkspaceFolderService;
use App\Domain\Content\Content;
use App\Domain\Workspace\Workspace;
use App\Domain\WorkspaceFolder\WorkspaceFolder;
use App\Http\Requests\AddFolderToWorkspaceRequest;
use App\Http\Requests\AttachStudentsRequest;
use App\Http\Requests\MoveContentRequest;
use App\Http\Requests\MoveFolderRequest;
use App\Http\Requests\RemoveContentRequest;
use App\Http\Requests\RenameFolderRequest;
use App\Http\Requests\StoreFolderRequest;
use App\Http\Requests\WorkspaceRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * İş sahəsi səhifələri — server-rendered Blade.
 * İş sahəsi base folder kimidir: içində qovluq, quiz, dərs təşkil olunur.
 * Qovluq/quiz/dərs əməliyyatları JS vasitəsilə web controller üzərindən gedir.
 */
class WorkspaceController
{
    public function __construct(
        private readonly WorkspaceService $workspaces,
        private readonly WorkspaceFolderService $workspaceFolders,
    ) {
    }

    public function index(Request $request): View
    {
        $search = (string) $request->string('search');

        return view('teacher.workspaces.index', [
            'workspaces' => $this->workspaces->paginate((int) auth()->id(), $search ?: null, 15),
            'search' => $search,
        ]);
    }

    public function store(WorkspaceRequest $request): RedirectResponse
    {
        $data = $request->validated();

        $workspace = $this->workspaces->create(
            (int) auth()->id(),
            $data['name'],
            isset($data['monthly_price']) ? (float) $data['monthly_price'] : null,
        );

        return redirect()->route('teacher.workspaces.show', $workspace->id)
            ->with('success', 'Workspace yaradıldı.');
    }

    public function show(Request $request, int $workspace): View
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);

        $actingId = $this->actingId($model);
        $currentFolderId = (int) $request->integer('folder_id') ?: null;

        // Qovluq sahibini doğrulamaq üçün service-ə acting id ötürürük.
        $directory = $this->workspaceFolders->directory($workspace, $currentFolderId, $actingId);

        return view('teacher.workspaces.show', [
            'workspaceId' => $workspace,
            'workspaceName' => $model->name,
            'currentFolderId' => $currentFolderId,
            'directory' => $directory,
            'folderTree' => $this->workspaceFolders->folderTree($workspace, $actingId, $currentFolderId),
            'availableContents' => $this->workspaceFolders->availableContents($actingId),
            'students' => $this->workspaces->studentList($actingId, $workspace),
            'availableStudents' => collect($this->workspaces->availableStudents($actingId, $workspace))
                ->pluck('name', 'id')
                ->map(fn ($name) => (string) $name)
                ->all(),
        ]);
    }

    public function edit(int $workspace): View
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);

        return view('teacher.workspaces.form', [
            'heading' => 'Workspace-i Redaktə Et',
            'subtitle' => $model->name,
            'creating' => false,
            'workspace' => [
                'id' => $workspace,
                'name' => $model->name,
                'monthly_price' => $model->monthly_price !== null ? (float) $model->monthly_price : null,
            ],
        ]);
    }

    public function update(WorkspaceRequest $request, int $workspace): RedirectResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);

        $data = $request->validated();

        $this->workspaces->rename(
            (int) auth()->id(),
            $workspace,
            $data['name'],
            isset($data['monthly_price']) ? (float) $data['monthly_price'] : null,
        );

        return redirect()->route('teacher.workspaces.show', $workspace)
            ->with('success', 'Workspace yeniləndi.');
    }

    public function destroy(Request $request, int $workspace): RedirectResponse|JsonResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);

        $this->workspaces->delete((int) auth()->id(), $workspace);

        if ($request->expectsJson()) {
            return response()->json(['message' => 'Workspace silindi.']);
        }

        return redirect()->route('teacher.workspaces.index')->with('success', 'Workspace silindi.');
    }

    // ─────────────────────────────────────────────────────────────────────────
    // JSON əməliyyatları — frontend JS bu endpointləri çağırır (web controller).
    // ─────────────────────────────────────────────────────────────────────────

    /** Workspace-də yeni qovluq yaradır. */
    public function storeFolder(StoreFolderRequest $request, int $workspace): JsonResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);

        $data = $request->validated();

        $folder = $this->workspaceFolders->createFolder(
            $workspace,
            $data['name'],
            $data['parent_id'] ?? null,
            $this->actingId($model),
        );

        return response()->json([
            'data' => ['id' => (int) $folder->id, 'name' => $folder->name],
        ], 201);
    }

    /** Workspace qovluğunun adını dəyişir. */
    public function renameFolder(RenameFolderRequest $request, int $workspace, int $folderId): JsonResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);
        $this->assertFolderInWorkspace($folderId, $workspace);

        $data = $request->validated();

        $this->workspaceFolders->renameFolder($workspace, $folderId, $data['name'], $this->actingId($model));

        return response()->json(['message' => 'Qovluq adı yeniləndi.']);
    }

    /** Workspace qovluğunu daşıyır (null → workspace kökü). */
    public function moveFolder(MoveFolderRequest $request, int $workspace, int $folderId): JsonResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);
        $this->assertFolderInWorkspace($folderId, $workspace);

        $data = $request->validated();

        $this->workspaceFolders->moveFolder($workspace, $folderId, $data['parent_id'] ?? null, $this->actingId($model));

        return response()->json(['message' => 'Qovluq daşındı.']);
    }

    /** Workspace qovluğunu silir (içindəki məzmun kökə qayıdır). */
    public function destroyFolder(int $workspace, int $folderId): JsonResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);
        $this->assertFolderInWorkspace($folderId, $workspace);

        $this->workspaceFolders->deleteFolder($workspace, $folderId, $this->actingId($model));

        return response()->json(['message' => 'Qovluq silindi.']);
    }

    /** Workspace qovluğunu içindəki məzmunla birlikdə kütüphanəyə geri göndərir. */
    public function removeFolder(int $workspace, int $folderId): JsonResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);
        $this->assertFolderInWorkspace($folderId, $workspace);

        $this->workspaceFolders->removeFolderFromWorkspace($folderId, $this->actingId($model));

        return response()->json(['message' => 'Qovluq və içindəki məzmunlar çıxarıldı.']);
    }

    /** Content-i workspace qovluğuna daşıyır (null → workspace kökü). */
    public function moveContent(MoveContentRequest $request): JsonResponse
    {
        $data = $request->validated();

        $this->assertContentAccess($this->workspaceFolders->findContent((int) $data['content_id']));

        $this->workspaceFolders->moveContent(
            (int) $data['content_id'],
            $data['workspace_id'] ?? null,
            $data['folder_id'] ?? null,
            (int) auth()->id(),
        );

        return response()->json(['message' => 'Content daşındı.']);
    }

    /** Bank qovluğunu (içindəki məzmunlarla) workspace-ə əlavə edir. */
    public function addFolder(AddFolderToWorkspaceRequest $request): JsonResponse
    {
        $data = $request->validated();

        $model = $this->workspaces->find((int) $data['workspace_id']);
        $this->assertAccess($model);

        $result = $this->workspaceFolders->addFolderToWorkspace(
            $data['folder_type'],
            (int) $data['bank_folder_id'],
            (int) $data['workspace_id'],
            $data['folder_id'] ?? null,
            $this->actingId($model),
        );

        return response()->json([
            'message' => 'Qovluq və içindəki məzmunlar əlavə edildi.',
            'folders' => $result['folders'],
            'contents' => $result['contents'],
        ]);
    }

    /** Content-i workspace-dən kütüphanəyə geri göndərir. */
    public function removeContent(RemoveContentRequest $request): JsonResponse
    {
        $data = $request->validated();

        $content = $this->workspaceFolders->findContent((int) $data['content_id']);
        $this->assertContentAccess($content);
        if ($content->workspace_id === null) {
            return response()->json(['message' => 'Məzmun workspace-də deyil.'], 422);
        }

        $this->workspaceFolders->removeContentFromWorkspace((int) $data['content_id'], (int) auth()->id());

        return response()->json(['message' => 'Məzmun workspace-dən çıxarıldı.']);
    }

    /** Workspace-ə tələbələri əlavə edir. */
    public function attachStudents(AttachStudentsRequest $request, int $workspace): JsonResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);

        $data = $request->validated();

        $this->workspaces->attachStudents($this->actingId($model), $workspace, $data['student_ids']);

        return response()->json(['message' => 'Şagirdlər əlavə edildi.']);
    }

    /** Tələbəni workspace-dən çıxarır. */
    public function detachStudent(int $workspace, int $studentId): JsonResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);

        $this->workspaces->detachStudent($this->actingId($model), $workspace, $studentId);

        return response()->json(['message' => 'Şagird çıxarıldı.']);
    }

    /** Qovluğun bu workspace-ə aid olduğunu yoxla. */
    protected function assertFolderInWorkspace(int $folderId, int $workspaceId): void
    {
        $folder = $this->workspaceFolders->find($folderId);
        if ($folder === null || $folder->workspace_id !== $workspaceId) {
            abort(404);
        }
    }

    /** Content sahibə aid deyilsə və ya mövcud deyilsə rədd edir. */
    protected function assertContentAccess(?Content $content): void
    {
        if ($content === null) {
            abort(404);
        }
        $user = auth()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $content->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }

    protected function assertAccess(?Workspace $workspace): void
    {
        if ($workspace === null) {
            abort(404);
        }
        $user = auth()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $workspace->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }

    /** Admin başqasının workspace-inə baxanda sahibin id-si ilə işləyir. */
    protected function actingId(Workspace $workspace): int
    {
        $user = auth()->user();

        return $user?->isAdmin() ? (int) $workspace->teacher_id : (int) $user->id;
    }
}
