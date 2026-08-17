<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Workspace\WorkspaceService;
use App\Application\WorkspaceFolder\WorkspaceFolderService;
use App\Domain\Workspace\Workspace;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * İş sahəsi səhifələri — server-rendered Blade.
 * İş sahəsi base folder kimidir: içində qovluq, quiz, dərs təşkil olunur.
 * Qovluq/quiz/dərs əməliyyatları JS vasitəsilə /api/v1-ə gedir.
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

    public function store(Request $request): RedirectResponse
    {
        $data = $request->validate([
            'name' => ['required', 'string', 'max:200'],
        ]);

        $workspace = $this->workspaces->create((int) auth()->id(), $data['name']);

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
            'workspace' => ['id' => $workspace, 'name' => $model->name],
        ]);
    }

    public function update(Request $request, int $workspace): RedirectResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);

        $data = $request->validate([
            'name' => ['required', 'string', 'max:200'],
        ]);

        $this->workspaces->rename((int) auth()->id(), $workspace, $data['name']);

        return redirect()->route('teacher.workspaces.show', $workspace)
            ->with('success', 'Workspace yeniləndi.');
    }

    public function destroy(int $workspace): RedirectResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);

        $this->workspaces->delete((int) auth()->id(), $workspace);

        return redirect()->route('teacher.workspaces.index')->with('success', 'Workspace silindi.');
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
