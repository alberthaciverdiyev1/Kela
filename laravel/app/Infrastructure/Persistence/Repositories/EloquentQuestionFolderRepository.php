<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\QuestionFolder\QuestionFolder;
use App\Domain\QuestionFolder\QuestionFolderRepository;
use Illuminate\Support\Collection;

class EloquentQuestionFolderRepository implements QuestionFolderRepository
{
    public function foldersFor(int $teacherId, ?int $parentId = null): Collection
    {
        return QuestionFolder::query()
            ->where('teacher_id', $teacherId)
            ->where('parent_id', $parentId)
            ->orderBy('position')
            ->orderBy('name')
            ->get();
    }

    public function allFoldersFor(int $teacherId): Collection
    {
        return QuestionFolder::query()
            ->where('teacher_id', $teacherId)
            ->orderBy('name')
            ->get();
    }

    public function breadcrumbs(?int $parentId): array
    {
        $crumbs = [];
        $id = $parentId;

        while ($id !== null) {
            $folder = QuestionFolder::withTrashed()->find($id);
            if (! $folder) {
                break;
            }
            array_unshift($crumbs, ['id' => $folder->id, 'name' => $folder->name]);
            $id = $folder->parent_id;
        }

        return $crumbs;
    }

    public function find(int $id): ?QuestionFolder
    {
        return QuestionFolder::find($id);
    }

    public function create(int $teacherId, string $name, ?int $parentId): QuestionFolder
    {
        $position = $this->nextPosition($teacherId, $parentId);

        return QuestionFolder::create([
            'teacher_id' => $teacherId,
            'parent_id' => $parentId,
            'name' => $name,
            'position' => $position,
        ]);
    }

    public function update(QuestionFolder $folder, array $attributes): QuestionFolder
    {
        $folder->update($attributes);

        return $folder;
    }

    public function deleteTree(int $folderId): void
    {
        $folder = QuestionFolder::with('children')->find($folderId);
        if (! $folder) {
            return;
        }

        foreach ($folder->children as $child) {
            $this->deleteTree($child->id);
        }

        // Sualları kökə qaytar (qovluq silinəndə sual itməsin).
        $folder->questions()->update(['folder_id' => null]);

        $folder->delete();
    }

    public function descendantIds(int $folderId): array
    {
        $ids = [$folderId];
        $toVisit = [$folderId];

        while ($toVisit !== []) {
            $current = array_shift($toVisit);
            $children = QuestionFolder::query()
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
        return (int) QuestionFolder::query()
            ->where('teacher_id', $teacherId)
            ->where('parent_id', $parentId)
            ->max('position') + 1;
    }
}
