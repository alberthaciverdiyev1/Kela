<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\WorkspaceFolder\WorkspaceFolder;
use App\Domain\WorkspaceFolder\WorkspaceFolderRepository;
use Illuminate\Support\Collection;

class EloquentWorkspaceFolderRepository implements WorkspaceFolderRepository
{
    public function foldersFor(int $workspaceId, ?int $parentId = null): Collection
    {
        return WorkspaceFolder::query()
            ->where('workspace_id', $workspaceId)
            ->where('parent_id', $parentId)
            ->orderBy('position')
            ->orderBy('name')
            ->get();
    }

    public function allFoldersFor(int $workspaceId): Collection
    {
        return WorkspaceFolder::query()
            ->where('workspace_id', $workspaceId)
            ->orderBy('name')
            ->get();
    }

    public function breadcrumbs(?int $parentId): array
    {
        $crumbs = [];
        $id = $parentId;

        while ($id !== null) {
            $folder = WorkspaceFolder::withTrashed()->find($id);
            if (! $folder) {
                break;
            }
            array_unshift($crumbs, ['id' => $folder->id, 'name' => $folder->name]);
            $id = $folder->parent_id;
        }

        return $crumbs;
    }

    public function find(int $id): ?WorkspaceFolder
    {
        return WorkspaceFolder::find($id);
    }

    public function create(int $workspaceId, string $name, ?int $parentId): WorkspaceFolder
    {
        $position = $this->nextPosition($workspaceId, $parentId);

        return WorkspaceFolder::create([
            'workspace_id' => $workspaceId,
            'parent_id' => $parentId,
            'name' => $name,
            'position' => $position,
        ]);
    }

    public function update(WorkspaceFolder $folder, array $attributes): WorkspaceFolder
    {
        $folder->update($attributes);

        return $folder;
    }

    public function deleteTree(int $folderId): void
    {
        $folder = WorkspaceFolder::with('children')->find($folderId);
        if (! $folder) {
            return;
        }

        foreach ($folder->children as $child) {
            $this->deleteTree($child->id);
        }

        // Content-ləri kökə qaytar (qovluq silinəndə content itməsin).
        $folder->contents()->update(['folder_id' => null]);

        $folder->delete();
    }

    public function descendantIds(int $folderId): array
    {
        $ids = [$folderId];
        $toVisit = [$folderId];

        while ($toVisit !== []) {
            $current = array_shift($toVisit);
            $children = WorkspaceFolder::query()
                ->where('parent_id', $current)
                ->pluck('id');

            foreach ($children as $child) {
                $ids[] = $child;
                $toVisit[] = $child;
            }
        }

        return $ids;
    }

    protected function nextPosition(int $workspaceId, ?int $parentId): int
    {
        return (int) WorkspaceFolder::query()
            ->where('workspace_id', $workspaceId)
            ->where('parent_id', $parentId)
            ->max('position') + 1;
    }
}
