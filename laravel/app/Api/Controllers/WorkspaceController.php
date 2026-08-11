<?php

namespace App\Api\Controllers;

use App\Application\Workspace\WorkspaceService;
use App\Domain\Workspace\Workspace;
use App\Api\Resources\WorkspaceResource;
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

    public function store(Request $request): JsonResponse
    {
        $data = $request->validate([
            'name' => ['required', 'string', 'max:255'],
        ]);

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
            'directory' => $this->workspaces->directory((int) $request->user()->id, $workspace),
        ]);
    }

    public function update(Request $request, int $workspace): WorkspaceResource
    {
        $this->authorizeAccess($this->workspaces->find($workspace));

        $data = $request->validate([
            'name' => ['required', 'string', 'max:255'],
        ]);

        $this->workspaces->rename((int) $request->user()->id, $workspace, $data['name']);

        return new WorkspaceResource($this->workspaces->find($workspace));
    }

    public function destroy(Request $request, int $workspace): JsonResponse
    {
        $this->workspaces->delete((int) $request->user()->id, $workspace);

        return response()->json(['message' => 'İş sahəsi silindi.']);
    }

    public function createFolder(Request $request, int $workspace): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace));

        $data = $request->validate([
            'name' => ['required', 'string', 'max:255'],
            'parent_id' => ['nullable', 'integer'],
        ]);

        $folder = $this->workspaces->createFolder(
            (int) $request->user()->id,
            $workspace,
            $data['name'],
            $data['parent_id'] ?? null,
        );

        return response()->json([
            'data' => ['id' => (int) $folder->id, 'name' => $folder->name],
        ], 201);
    }

    public function addContent(Request $request, int $workspace): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace));

        $data = $request->validate([
            'content_id' => ['required', 'integer'],
            'parent_id' => ['nullable', 'integer'],
        ]);

        $node = $this->workspaces->addContent(
            (int) $request->user()->id,
            $workspace,
            (int) $data['content_id'],
            $data['parent_id'] ?? null,
        );

        return response()->json([
            'data' => ['id' => (int) $node->id, 'content_id' => (int) $node->content_id],
        ], 201);
    }

    public function removeNode(Request $request, int $workspace, int $nodeId): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace));

        // Məzmun node-u və ya qovluq ağacı silinir (məzmun kitabxanada qalır).
        $this->workspaces->deleteNode((int) $request->user()->id, $workspace, $nodeId);

        return response()->json(['message' => 'Element silindi.']);
    }

    public function renameNode(Request $request, int $workspace, int $nodeId): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace));

        $data = $request->validate([
            'name' => ['required', 'string', 'max:255'],
        ]);

        $this->workspaces->renameNode((int) $request->user()->id, $workspace, $nodeId, $data['name']);

        return response()->json(['message' => 'Ad yeniləndi.']);
    }

    public function moveNode(Request $request, int $workspace, int $nodeId): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace));

        $data = $request->validate([
            'parent_id' => ['nullable', 'integer'],
        ]);

        $this->workspaces->moveNode((int) $request->user()->id, $workspace, $nodeId, $data['parent_id'] ?? null);

        return response()->json(['message' => 'Daşındı.']);
    }

    public function attachStudents(Request $request, int $workspace): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace));

        $data = $request->validate([
            'student_ids' => ['required', 'array'],
            'student_ids.*' => ['integer'],
        ]);

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
