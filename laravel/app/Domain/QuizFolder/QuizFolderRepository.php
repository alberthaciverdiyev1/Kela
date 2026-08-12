<?php

namespace App\Domain\QuizFolder;

use Illuminate\Support\Collection;

/**
 * Quiz qovluqları üçün məlumat girişi kontraktı.
 * Implementasiya Infrastructure katmanında (Eloquent) yerləşir.
 */
interface QuizFolderRepository
{
    /** Teacher-ın kök qovluqları (parent_id null) və ya verilmiş ana qovluğun altındakılar. */
    public function foldersFor(int $teacherId, ?int $parentId = null): Collection;

    /** Move dropdown üçün teacher-ın bütün qovluqları. */
    public function allFoldersFor(int $teacherId): Collection;

    /** parentId-dən kökə qədər breadcrumb zənciri. */
    public function breadcrumbs(?int $parentId): array;

    public function find(int $id): ?QuizFolder;

    public function create(int $teacherId, string $name, ?int $parentId): QuizFolder;

    public function update(QuizFolder $folder, array $attributes): QuizFolder;

    public function deleteTree(int $folderId): void;

    /** Qovluq və bütün alt qovluqlarının id-ləri. */
    public function descendantIds(int $folderId): array;
}
