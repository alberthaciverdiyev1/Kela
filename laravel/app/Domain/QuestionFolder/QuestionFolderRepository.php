<?php

namespace App\Domain\QuestionFolder;

use Illuminate\Support\Collection;

/**
 * Sual bankı qovluqları üçün məlumat girişi kontraktı.
 * Implementasiya Infrastructure katmanında (Eloquent) yerləşir.
 */
interface QuestionFolderRepository
{
    /** Teacher-ın kök qovluqları (parent_id null) və ya verilmiş ana qovluğun altındakılar. */
    public function foldersFor(int $teacherId, ?int $parentId = null): Collection;

    /** Move dropdown üçün teacher-ın bütün qovluqları. */
    public function allFoldersFor(int $teacherId): Collection;

    /** parentId-dən kökə qədər breadcrumb zənciri. */
    public function breadcrumbs(?int $parentId): array;

    public function find(int $id): ?QuestionFolder;

    public function create(int $teacherId, string $name, ?int $parentId): QuestionFolder;

    public function update(QuestionFolder $folder, array $attributes): QuestionFolder;

    public function deleteTree(int $folderId): void;

    /** Qovluq və bütün alt qovluqlarının id-ləri. */
    public function descendantIds(int $folderId): array;
}
