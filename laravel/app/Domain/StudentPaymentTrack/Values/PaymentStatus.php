<?php

namespace App\Domain\StudentPaymentTrack\Values;

use App\Domain\StudentPaymentTrack\Enums\PaymentStatus as PaymentStatusEnum;

/**
 * @deprecated Use App\Domain\StudentPaymentTrack\Enums\PaymentStatus instead.
 */
final class PaymentStatus
{
    public const PENDING = PaymentStatusEnum::PENDING->value;
    public const PAID = PaymentStatusEnum::PAID->value;
    public const OVERDUE = PaymentStatusEnum::OVERDUE->value;
    public const CANCELLED = PaymentStatusEnum::CANCELLED->value;

    public const ALL = [
        self::PENDING,
        self::PAID,
        self::OVERDUE,
        self::CANCELLED,
    ];

    public static function label(int $status): string
    {
        return PaymentStatusEnum::labelFor($status);
    }

    public static function isValid(int $status): bool
    {
        return PaymentStatusEnum::isValid($status);
    }
}
