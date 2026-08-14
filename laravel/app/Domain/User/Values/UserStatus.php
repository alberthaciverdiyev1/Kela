<?php

namespace App\Domain\User\Values;

use App\Domain\User\Enums\UserStatus as UserStatusEnum;

/**
 * @deprecated Use App\Domain\User\Enums\UserStatus instead.
 */
final class UserStatus
{
    public const ACTIVE = UserStatusEnum::ACTIVE->value;
    public const INACTIVE = UserStatusEnum::INACTIVE->value;
    public const SUSPENDED = UserStatusEnum::SUSPENDED->value;
}
