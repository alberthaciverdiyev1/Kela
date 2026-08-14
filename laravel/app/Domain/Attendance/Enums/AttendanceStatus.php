<?php

namespace App\Domain\Attendance\Enums;

enum AttendanceStatus: int
{
    case UNKNOWN = 0;
    case PRESENT = 1;
    case ABSENT = 2;
    case LATE = 3;
    case EXCUSED = 4;

    public function label(): string
    {
        return match ($this) {
            self::UNKNOWN => 'unknown',
            self::PRESENT => 'present',
            self::ABSENT => 'absent',
            self::LATE => 'late',
            self::EXCUSED => 'excused',
        };
    }

    public static function labelFor(int|self $status): string
    {
        $enum = is_int($status) ? self::tryFrom($status) : $status;
        return $enum?->label() ?? 'unknown';
    }

    /**
     * @return array<int, string>
     */
    public static function labels(): array
    {
        $labels = [];
        foreach (self::cases() as $case) {
            $labels[$case->value] = $case->label();
        }
        return $labels;
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
