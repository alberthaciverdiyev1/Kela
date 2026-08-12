<?php

namespace App\Api\Controllers;

use App\Application\Lesson\LessonService;
use App\Application\LessonFolder\LessonFolderService;
use App\Application\WorkspaceFolder\WorkspaceFolderService;
use App\Domain\Lesson\Lesson;
use App\Api\Resources\LessonResource;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;
use Illuminate\Pagination\LengthAwarePaginator;

class LessonController
{
    public function __construct(
        private readonly LessonService $lessons,
        private readonly LessonFolderService $lessonFolders,
        private readonly WorkspaceFolderService $workspaceFolders,
    ) {
    }

    public function index(Request $request): LengthAwarePaginator
    {
        return $this->lessons->paginate(
            (int) $request->user()->id,
            $request->string('search')->toString() ?: null,
            (int) $request->integer('folder_id'),
            (int) $request->integer('per_page', 15),
        );
    }

    public function store(Request $request): JsonResponse
    {
        $data = $request->validate([
            'title' => ['required', 'string', 'max:255'],
            'description' => ['nullable', 'string'],
            'video_path' => ['nullable', 'string'],
            'is_published' => ['nullable', 'boolean'],
            'order_index' => ['nullable', 'integer'],
            'folder_id' => ['nullable', 'integer'],
            'workspace_id' => ['nullable', 'integer'],
            'ws_folder_id' => ['nullable', 'integer'],
        ]);

        $data['folder_id'] = $this->lessonFolders->resolveFolderFor((int) $request->user()->id, $data['folder_id'] ?? null);

        $context = $this->workspaceFolders->resolveContextFor(
            (int) ($data['workspace_id'] ?? 0) ?: null,
            (int) ($data['ws_folder_id'] ?? 0) ?: null,
            (int) $request->user()->id,
        );
        if ($context !== null) {
            $data['workspace_id'] = $context['workspace_id'];
            $data['ws_folder_id'] = $context['folder_id'];
        }

        $lesson = $this->lessons->create((int) $request->user()->id, $data);

        return (new LessonResource($lesson))->response()->setStatusCode(201);
    }

    public function show(int $contentId): JsonResponse
    {
        $lesson = $this->lessons->find($contentId);
        $this->authorizeAccess($lesson);

        return response()->json([
            'data' => new LessonResource($lesson),
            'viewer' => [
                'has_video' => (bool) $lesson->has_video,
                'stream_url' => $lesson->has_video ? url("/api/v1/lessons/{$contentId}/stream") : null,
                'thumbnail_url' => $lesson->thumbnail_path ? url("/api/v1/lessons/{$contentId}/thumbnail") : null,
            ],
        ]);
    }

    public function update(Request $request, int $contentId): LessonResource
    {
        $this->authorizeAccess($this->lessons->find($contentId));

        $data = $request->validate([
            'title' => ['required', 'string', 'max:255'],
            'description' => ['nullable', 'string'],
            'video_path' => ['nullable', 'string'],
            'is_published' => ['nullable', 'boolean'],
            'order_index' => ['nullable', 'integer'],
            'folder_id' => ['nullable', 'integer'],
            'workspace_id' => ['nullable', 'integer'],
            'ws_folder_id' => ['nullable', 'integer'],
        ]);

        $data['folder_id'] = $this->lessonFolders->resolveFolderFor((int) $request->user()->id, $data['folder_id'] ?? null);

        $context = $this->workspaceFolders->resolveContextFor(
            (int) ($data['workspace_id'] ?? 0) ?: null,
            (int) ($data['ws_folder_id'] ?? 0) ?: null,
            (int) $request->user()->id,
        );
        if ($context !== null) {
            $data['workspace_id'] = $context['workspace_id'];
            $data['ws_folder_id'] = $context['folder_id'];
        }

        $lesson = $this->lessons->update($contentId, $data);

        return new LessonResource($lesson);
    }

    public function destroy(int $contentId): JsonResponse
    {
        $this->authorizeAccess($this->lessons->find($contentId));
        $this->lessons->delete($contentId);

        return response()->json(['message' => 'Dərs silindi.']);
    }

    private function authorizeAccess(?Lesson $lesson): void
    {
        if ($lesson === null) {
            abort(404);
        }
        $user = request()->user();
        if ($user->isAdmin()) {
            return;
        }
        if ($lesson->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
