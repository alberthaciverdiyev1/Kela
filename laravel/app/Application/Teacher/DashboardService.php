<?php

namespace App\Application\Teacher;

use App\Domain\User\UserRepository;
use App\Domain\User\Values\UserRole;

/**
 * Admin panel özet veriləri.
 */
class DashboardService
{
    public function __construct(private readonly UserRepository $users)
    {
    }

    public function counts(): array
    {
        return [
            'teachers' => $this->users->roleCount(UserRole::TEACHER),
            'students' => $this->users->roleCount(UserRole::STUDENT),
        ];
    }
}
