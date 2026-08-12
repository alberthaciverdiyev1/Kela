<?php

namespace App\Domain\User;

/**
 * İstifadəçi sorğuları üçün kontrakt.
 */
interface UserRepository
{
    /** Verilən roldakı istifadəçi sayı. */
    public function roleCount(string $role): int;

    /** Email ilə istifadəçi axtarır (yoxdursa null). */
    public function findByEmail(string $email): ?User;

    /** Yeni müəllim hesabı yaradır (aktiv status + Teacher rolu). */
    public function createTeacher(array $data): User;
}
