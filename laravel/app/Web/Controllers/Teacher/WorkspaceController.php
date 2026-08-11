<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Content\ContentService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\Workspace\Workspace;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * İş sahəsi səhifələri — server-rendered Blade.
 * Qovluq naviqasiyası GET linkləri (server), node əməliyyatları JS → /api/v1,
 * kataloq isə server-rendered fragment ilə yenilənir.
 */
class WorkspaceController
{
    public function __construct(
        private readonly WorkspaceService $workspaces,
        private readonly ContentService $contents,
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

    public function create(): View
    {
        return view('teacher.workspaces.form', [
            'heading' => 'Yeni Workspace',
            'subtitle' => 'Yeni iş sahəsi yarat',
            'creating' => true,
            'workspace' => null,
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
        $parentId = $request->integer('parent_id') ?: null;
        $dir = $this->workspaces->directory($actingId, $workspace, $parentId);
        $students = $this->workspaces->studentList($actingId, $workspace);

        return view('teacher.workspaces.show', [
            'workspaceId' => $workspace,
            'workspaceName' => $model->name,
            'parentId' => $parentId,
            'folders' => $dir['folders'],
            'contents' => $dir['contents'],
            'breadcrumbs' => $dir['breadcrumbs'],
            'students' => $students,
            'contentOptions' => $this->contents->allContentOptions($actingId),
            'availableStudents' => collect($this->workspaces->availableStudents($actingId, $workspace))
                ->pluck('name', 'id')
                ->map(fn ($name) => (string) $name)
                ->all(),
            'folderTree' => $this->workspaces->folderTree($actingId, $workspace),
        ]);
    }

    /** JS-in kataloqu yeniləməsi üçün server-rendered fragment. */
    public function directoryFragment(Request $request, int $workspace): View
    {
        $model = $this->workspaces->find($workspace);
        $this->assertAccess($model);

        $actingId = $this->actingId($model);
        $parentId = $request->integer('parent_id') ?: null;
        $dir = $this->workspaces->directory($actingId, $workspace, $parentId);

        return view('teacher.workspaces._directory', [
            'workspaceId' => $workspace,
            'parentId' => $parentId,
            'folders' => $dir['folders'],
            'contents' => $dir['contents'],
            'breadcrumbs' => $dir['breadcrumbs'],
            'folderTree' => $this->workspaces->folderTree($actingId, $workspace),
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
