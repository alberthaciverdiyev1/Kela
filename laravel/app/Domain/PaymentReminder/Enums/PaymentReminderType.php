<?php

namespace App\Domain\PaymentReminder\Enums;

/** Ödəniş bildirişinin növü. */
enum PaymentReminderType: string
{
    case UPCOMING = 'upcoming';
    case DUE = 'due';

    public function label(): string
    {
        return match ($this) {
            self::UPCOMING => '5 gün qalıb',
            self::DUE => 'Ödəniş günü',
        };
    }
}
