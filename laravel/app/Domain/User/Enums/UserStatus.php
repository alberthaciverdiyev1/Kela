<?php

namespace App\Domain\User\Enums;

/**
 * İstifadəçi statusları üçün PHP 8.3 Backed Enum.
 */
enum UserStatus: int
{
    case ACTIVE = 1;
    case INACTIVE = 2;
    case SUSPENDED = 3;

    public function label(): string
    {
        return match ($this) {
            self::ACTIVE => 'Aktiv',
            self::INACTIVE => 'Qeyri-aktiv',
            self::SUSPENDED => 'Dondurulmuş',
        };
    }

    /**
     * @return list<int>
     */
    public static function values(): array
    {
        return array_column(self::cases(), 'value');
    }

    public static function isValid(int $status): bool
    {
        return self::tryFrom($status) !== null;
    }
}
