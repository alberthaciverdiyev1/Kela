<?php

namespace App\Domain\User\Enums;

/**
 * İstifadəçi rolları üçün PHP 8.3 Backed Enum.
 */
enum UserRole: string
{
    case ADMIN = 'Admin';
    case TEACHER = 'Teacher';
    case STUDENT = 'Student';
    case PARENT = 'Parent';

    /**
     * Bütün rol dəyərləri siyahısı.
     *
     * @return list<string>
     */
    public static function values(): array
    {
        return array_column(self::cases(), 'value');
    }

    public static function isValid(string $role): bool
    {
        return self::tryFrom($role) !== null;
    }
}
