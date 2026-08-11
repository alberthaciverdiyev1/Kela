<?php

namespace App\Domain\Student;

use App\Domain\User\User;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Support\Collection;

/**
 * Şagird (User + Student rolu + profil) üçün kontrakt.
 */
interface StudentRepository
{
    public function create(array $data): User;

    public function update(int $id, array $data): User;

    public function find(int $id): ?User;

    public function delete(int $id): bool;

    /** @return Collection<int, User> */
    public function all(): Collection;

    /** Axtarış + səhifələmə ilə tələbə siyahısı (admin cədvəli üçün). */
    public function paginate(?string $search = null, int $perPage = 15): LengthAwarePaginator;

    /** Optional axtarışla tələbə siyahısı (workspace seçici üçün). */
    public function availableForWorkspace(?string $search = null): Collection;
}
