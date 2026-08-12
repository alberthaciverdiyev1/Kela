<?php

namespace App\Domain\LessonFolder;

use Illuminate\Support\Collection;

interface LessonFolderRepository
{
    public function foldersFor(int $teacherId, ?int $parentId = null): Collection;

    public function allFoldersFor(int $teacherId): Collection;

    public function breadcrumbs(?int $parentId): array;

    public function find(int $id): ?LessonFolder;

    public function create(int $teacherId, string $name, ?int $parentId): LessonFolder;

    public function update(LessonFolder $folder, array $attributes): LessonFolder;

    public function deleteTree(int $folderId): void;

    public function descendantIds(int $folderId): array;
}
