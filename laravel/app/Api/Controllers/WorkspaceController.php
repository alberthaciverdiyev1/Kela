<?php

namespace App\Api\Controllers;

use App\Application\Workspace\WorkspaceService;
use App\Domain\Workspace\Workspace;
use App\Api\Resources\WorkspaceResource;
use App\Http\Requests\AttachStudentsRequest;
use App\Http\Requests\WorkspaceRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Pagination\LengthAwarePaginator;

class WorkspaceController
{
    public function __construct(private readonly WorkspaceService $workspaces)
    {
    }

    public function index(Request $request): LengthAwarePaginator
    {
        return $this->workspaces->paginate(
            (int) $request->user()->id,
            $request->string('search')->toString() ?: null,
            (int) $request->integer('per_page', 15),
        );
    }

    public function store(WorkspaceRequest $request): JsonResponse
    {
        $data = $request->validated();

        $workspace = $this->workspaces->create((int) $request->user()->id, $data['name']);

        return (new WorkspaceResource($workspace))->response()->setStatusCode(201);
    }

    public function show(Request $request, int $workspace): JsonResponse
    {
        $model = $this->workspaces->find($workspace);
        $this->authorizeAccess($model);

        return response()->json([
            'data' => new WorkspaceResource($model),
            'students' => $this->workspaces->studentList((int) $request->user()->id, $workspace),
        ]);
    }

    public function update(WorkspaceRequest $request, int $workspace): WorkspaceResource
    {
        $this->authorizeAccess($this->workspaces->find($workspace));

        $data = $request->validated();

        $this->workspaces->rename((int) $request->user()->id, $workspace, $data['name']);

        return new WorkspaceResource($this->workspaces->find($workspace));
    }

    public function destroy(Request $request, int $workspace): JsonResponse
    {
        $this->workspaces->delete((int) $request->user()->id, $workspace);

        return response()->json(['message' => 'İş sahəsi silindi.']);
    }

    public function attachStudents(AttachStudentsRequest $request, int $workspace): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace));

        $data = $request->validated();

        $this->workspaces->attachStudents((int) $request->user()->id, $workspace, $data['student_ids']);

        return response()->json(['message' => 'Şagirdlər əlavə edildi.']);
    }

    public function detachStudent(Request $request, int $workspace, int $studentId): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace));
        $this->workspaces->detachStudent((int) $request->user()->id, $workspace, $studentId);

        return response()->json(['message' => 'Şagird çıxarıldı.']);
    }

    private function authorizeAccess(?Workspace $workspace): void
    {
        if ($workspace === null) {
            abort(404);
        }
        $user = request()->user();
        if ($user->isAdmin()) {
            return;
        }
        if ($workspace->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
