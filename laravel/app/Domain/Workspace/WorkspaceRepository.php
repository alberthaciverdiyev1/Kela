<?php

namespace App\Domain\Workspace;

use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;
use Illuminate\Support\Collection;

/**
 * Workspace məlumat girişi üçün kontrakt.
 */
interface WorkspaceRepository
{
    /** Teacher-in workspaceləri (tələbə sayı ilə). */
    public function listForTeacher(int $teacherId): Collection;

    /** İstifadəçinin görə biləcəyi bütün workspacelər (admin → hamısı, müəllim → özü). */
    public function listForUser(int $actingUserId, bool $isAdmin): Collection;

    /** Sorğunu verilən istifadəçinin görə biləcəyi workspacelərlə məhdudlaşdırır. */
    public function scopeForUser(Builder $query, int $actingUserId, bool $isAdmin): Builder;

    /** Axtarış + səhifələmə ilə istifadəçinin görə biləcəyi workspacelər (admin cədvəli üçün). */
    public function paginateForUser(int $actingUserId, bool $isAdmin, ?string $search = null, int $perPage = 15): LengthAwarePaginator;

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
