<?php

namespace App\Domain\User\Values;

/**
 * İstifadəçi statusları (value object) — model deyil.
 * Web/Blade bu sinfi import edərək User modelinə toxunmaz.
 */
final class UserStatus
{
    public const ACTIVE = 1;
    public const INACTIVE = 2;
    public const SUSPENDED = 3;
}
