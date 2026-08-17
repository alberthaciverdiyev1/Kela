<?php

namespace App\Domain\StudentPaymentTrack\Enums;

/**
 * Şagird ödəniş statusları üçün PHP 8.3 Backed Enum.
 * Dəyərlər migration kommenti ilə tutuşur:
 * 0: Pending, 1: Partial, 2: Paid, 3: Overdue, 4: Cancelled.
 */
enum PaymentStatus: int
{
    case PENDING = 0;
    case PARTIAL = 1;
    case PAID = 2;
    case OVERDUE = 3;
    case CANCELLED = 4;

    public function label(): string
    {
        return match ($this) {
            self::PENDING => 'Gözləyir',
            self::PARTIAL => 'Qismən ödənilib',
            self::PAID => 'Ödənilib',
            self::OVERDUE => 'Vaxtı keçib',
            self::CANCELLED => 'Ləğv edilib',
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
