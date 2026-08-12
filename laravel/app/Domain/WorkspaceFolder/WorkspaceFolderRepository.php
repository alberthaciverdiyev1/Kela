<?php

namespace App\Domain\WorkspaceFolder;

use Illuminate\Support\Collection;

interface WorkspaceFolderRepository
{
    public function foldersFor(int $workspaceId, ?int $parentId = null): Collection;

    public function allFoldersFor(int $workspaceId): Collection;

    public function breadcrumbs(?int $parentId): array;

    public function find(int $id): ?WorkspaceFolder;

    public function create(int $workspaceId, string $name, ?int $parentId): WorkspaceFolder;

    public function update(WorkspaceFolder $folder, array $attributes): WorkspaceFolder;

    public function deleteTree(int $folderId): void;

    public function descendantIds(int $folderId): array;
}
