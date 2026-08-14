<?php

namespace App\Application\Teacher;

use App\Domain\User\Enums\UserRole;
use App\Domain\User\UserRepository;

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
            'teachers' => $this->users->roleCount(UserRole::TEACHER->value),
            'students' => $this->users->roleCount(UserRole::STUDENT->value),
        ];
    }
}
