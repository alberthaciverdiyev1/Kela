<?php

namespace App\Domain\Student;

use App\Domain\User\User;
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
}
