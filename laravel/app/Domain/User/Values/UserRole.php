<?php

namespace App\Domain\User\Values;

/**
 * İstifadəçi rolları (value object) — model deyil.
 * Web/Blade bu sinfi import edərək User modelinə toxunmaz.
 */
final class UserRole
{
    public const ADMIN = 'Admin';
    public const TEACHER = 'Teacher';
    public const STUDENT = 'Student';
    public const PARENT = 'Parent';

    public const ALL = [
        self::ADMIN,
        self::TEACHER,
        self::STUDENT,
        self::PARENT,
    ];
}
