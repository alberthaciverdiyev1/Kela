<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Content\Content;
use App\Domain\Content\ContentRepository;

class EloquentContentRepository implements ContentRepository
{
    public function create(array $attributes): Content
    {
        return Content::create($attributes);
    }

    public function update(Content $content, array $attributes): Content
    {
        $content->update($attributes);
        return $content;
    }

    public function delete(Content $content): bool
    {
        return (bool) $content->delete();
    }

    public function find(int $id): ?Content
    {
        return Content::find($id);
    }

    public function countByType(int $teacherId): array
    {
        return Content::query()
            ->where('teacher_id', $teacherId)
            ->selectRaw('type, count(*) as total')
            ->groupBy('type')
            ->pluck('total', 'type')
            ->map(fn ($v) => (int) $v)
            ->all();
    }

    public function allForTeacher(int $teacherId): array
    {
        return Content::query()
            ->where('teacher_id', $teacherId)
            ->get(['id', 'title'])
            ->pluck('title', 'id')
            ->all();
    }

    public function contentsForWorkspace(int $workspaceId, ?int $folderId): \Illuminate\Support\Collection
    {
        $query = Content::query()->where('workspace_id', $workspaceId);

        if ($folderId === null) {
            $query->whereNull('folder_id');
        } else {
            $query->where('folder_id', $folderId);
        }

        return $query->orderByDesc('created_at')->get();
    }

    public function availableForWorkspace(int $teacherId, array $types): \Illuminate\Support\Collection
    {
        return Content::query()
            ->where('teacher_id', $teacherId)
            ->whereIn('type', $types)
            ->whereNull('workspace_id')
            ->orderByDesc('created_at')
            ->get(['id', 'title', 'type', 'is_published', 'folder_id']);
    }
}
