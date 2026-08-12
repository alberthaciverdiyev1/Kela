<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\QuizFolder\QuizFolder;
use App\Domain\QuizFolder\QuizFolderRepository;
use Illuminate\Support\Collection;

class EloquentQuizFolderRepository implements QuizFolderRepository
{
    public function foldersFor(int $teacherId, ?int $parentId = null): Collection
    {
        return QuizFolder::query()
            ->where('teacher_id', $teacherId)
            ->where('parent_id', $parentId)
            ->orderBy('position')
            ->orderBy('name')
            ->get();
    }

    public function allFoldersFor(int $teacherId): Collection
    {
        return QuizFolder::query()
            ->where('teacher_id', $teacherId)
            ->orderBy('name')
            ->get();
    }

    public function breadcrumbs(?int $parentId): array
    {
        $crumbs = [];
        $id = $parentId;

        while ($id !== null) {
            $folder = QuizFolder::withTrashed()->find($id);
            if (! $folder) {
                break;
            }
            array_unshift($crumbs, ['id' => $folder->id, 'name' => $folder->name]);
            $id = $folder->parent_id;
        }

        return $crumbs;
    }

    public function find(int $id): ?QuizFolder
    {
        return QuizFolder::find($id);
    }

    public function create(int $teacherId, string $name, ?int $parentId): QuizFolder
    {
        $position = $this->nextPosition($teacherId, $parentId);

        return QuizFolder::create([
            'teacher_id' => $teacherId,
            'parent_id' => $parentId,
            'name' => $name,
            'position' => $position,
        ]);
    }

    public function update(QuizFolder $folder, array $attributes): QuizFolder
    {
        $folder->update($attributes);

        return $folder;
    }

    public function deleteTree(int $folderId): void
    {
        $folder = QuizFolder::with('children')->find($folderId);
        if (! $folder) {
            return;
        }

        foreach ($folder->children as $child) {
            $this->deleteTree($child->id);
        }

        // Quizləri kökə qaytar (qovluq silinəndə quiz itməsin).
        $folder->quizzes()->update(['folder_id' => null]);

        $folder->delete();
    }

    public function descendantIds(int $folderId): array
    {
        $ids = [$folderId];
        $toVisit = [$folderId];

        while ($toVisit !== []) {
            $current = array_shift($toVisit);
            $children = QuizFolder::query()
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
        return (int) QuizFolder::query()
            ->where('teacher_id', $teacherId)
            ->where('parent_id', $parentId)
            ->max('position') + 1;
    }
}
