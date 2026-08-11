<?php

namespace App\Domain\User;

/**
 * İstifadəçi sorğuları üçün kontrakt.
 */
interface UserRepository
{
    /** Verilən roldakı istifadəçi sayı. */
    public function roleCount(string $role): int;
}
