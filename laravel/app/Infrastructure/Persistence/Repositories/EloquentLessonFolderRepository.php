<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\LessonFolder\LessonFolder;
use App\Domain\LessonFolder\LessonFolderRepository;
use Illuminate\Support\Collection;

class EloquentLessonFolderRepository implements LessonFolderRepository
{
    public function foldersFor(int $teacherId, ?int $parentId = null): Collection
    {
        return LessonFolder::query()
            ->where('teacher_id', $teacherId)
            ->where('parent_id', $parentId)
            ->orderBy('position')
            ->orderBy('name')
            ->get();
    }

    public function allFoldersFor(int $teacherId): Collection
    {
        return LessonFolder::query()
            ->where('teacher_id', $teacherId)
            ->orderBy('name')
            ->get();
    }

    public function breadcrumbs(?int $parentId): array
    {
        $crumbs = [];
        $id = $parentId;

        while ($id !== null) {
            $folder = LessonFolder::withTrashed()->find($id);
            if (! $folder) {
                break;
            }
            array_unshift($crumbs, ['id' => $folder->id, 'name' => $folder->name]);
            $id = $folder->parent_id;
        }

        return $crumbs;
    }

    public function find(int $id): ?LessonFolder
    {
        return LessonFolder::find($id);
    }

    public function create(int $teacherId, string $name, ?int $parentId): LessonFolder
    {
        $position = $this->nextPosition($teacherId, $parentId);

        return LessonFolder::create([
            'teacher_id' => $teacherId,
            'parent_id' => $parentId,
            'name' => $name,
            'position' => $position,
        ]);
    }

    public function update(LessonFolder $folder, array $attributes): LessonFolder
    {
        $folder->update($attributes);

        return $folder;
    }

    public function deleteTree(int $folderId): void
    {
        $folder = LessonFolder::with('children')->find($folderId);
        if (! $folder) {
            return;
        }

        foreach ($folder->children as $child) {
            $this->deleteTree($child->id);
        }

        // Dərsləri kökə qaytar (qovluq silinəndə dərs itməsin).
        $folder->lessons()->update(['folder_id' => null]);

        $folder->delete();
    }

    public function descendantIds(int $folderId): array
    {
        $ids = [$folderId];
        $toVisit = [$folderId];

        while ($toVisit !== []) {
            $current = array_shift($toVisit);
            $children = LessonFolder::query()
                ->where('parent_id', $current)
                ->pluck('id');

            foreach ($children as $child) {
                $ids[] = $child;
                $toVisit[] = $child;
            }
        }

        return $ids;
    }

    protected function nextPosition(int $teacherId, ?int $parentId): int
    {
        return (int) LessonFolder::query()
            ->where('teacher_id', $teacherId)
            ->where('parent_id', $parentId)
            ->max('position') + 1;
    }
}
