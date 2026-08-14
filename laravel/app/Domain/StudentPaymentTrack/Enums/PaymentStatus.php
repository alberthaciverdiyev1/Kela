<?php

namespace App\Domain\StudentPaymentTrack\Enums;

/**
 * Şagird ödəniş statusları üçün PHP 8.3 Backed Enum.
 */
enum PaymentStatus: int
{
    case PENDING = 0;
    case PAID = 1;
    case OVERDUE = 2;
    case CANCELLED = 3;

    public function label(): string
    {
        return match ($this) {
            self::PENDING => 'pending',
            self::PAID => 'paid',
            self::OVERDUE => 'overdue',
            self::CANCELLED => 'cancelled',
        };
    }

    public static function labelFor(int|self $status): string
    {
        $enum = is_int($status) ? self::tryFrom($status) : $status;
        return $enum?->label() ?? 'unknown';
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
