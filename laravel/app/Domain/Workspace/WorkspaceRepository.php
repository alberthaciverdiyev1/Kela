<?php

namespace App\Domain\Workspace;

use Illuminate\Support\Collection;

/**
 * Workspace məlumat girişi üçün kontrakt.
 */
interface WorkspaceRepository
{
    /** Teacher-in workspaceləri (tələbə sayı ilə). */
    public function listForTeacher(int $teacherId): Collection;

    public function create(int $teacherId, string $name): Workspace;

    public function find(int $id): ?Workspace;

    public function update(Workspace $workspace, string $name): Workspace;

    public function delete(Workspace $workspace): bool;

    public function attachStudents(Workspace $workspace, array $studentIds): void;

    public function detachStudent(Workspace $workspace, int $studentId): void;

    public function studentIds(Workspace $workspace): array;

    /** Workspace-dəki tələbələrin kolleksiyası (ad, email). */
    public function students(Workspace $workspace): Collection;
}
