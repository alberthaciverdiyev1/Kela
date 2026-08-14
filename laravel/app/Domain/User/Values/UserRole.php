<?php

namespace App\Domain\User\Values;

use App\Domain\User\Enums\UserRole as UserRoleEnum;

/**
 * @deprecated Use App\Domain\User\Enums\UserRole instead.
 */
final class UserRole
{
    public const ADMIN = UserRoleEnum::ADMIN->value;
    public const TEACHER = UserRoleEnum::TEACHER->value;
    public const STUDENT = UserRoleEnum::STUDENT->value;
    public const PARENT = UserRoleEnum::PARENT->value;

    public const ALL = [
        self::ADMIN,
        self::TEACHER,
        self::STUDENT,
        self::PARENT,
    ];
}
