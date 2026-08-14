<?php

namespace App\Domain\Attendance\Values;

use App\Domain\Attendance\Enums\AttendanceStatus as AttendanceStatusEnum;

/**
 * @deprecated Use App\Domain\Attendance\Enums\AttendanceStatus instead.
 */
final class AttendanceStatus
{
    public const UNKNOWN = AttendanceStatusEnum::UNKNOWN->value;
    public const PRESENT = AttendanceStatusEnum::PRESENT->value;
    public const ABSENT = AttendanceStatusEnum::ABSENT->value;
    public const LATE = AttendanceStatusEnum::LATE->value;
    public const EXCUSED = AttendanceStatusEnum::EXCUSED->value;

    public const ALL = [
        self::UNKNOWN,
        self::PRESENT,
        self::ABSENT,
        self::LATE,
        self::EXCUSED,
    ];

    public const LABELS = [
        self::UNKNOWN => 'unknown',
        self::PRESENT => 'present',
        self::ABSENT => 'absent',
        self::LATE => 'late',
        self::EXCUSED => 'excused',
    ];

    public static function label(int $status): string
    {
        return AttendanceStatusEnum::labelFor($status);
    }

    public static function isValid(int $status): bool
    {
        return AttendanceStatusEnum::isValid($status);
    }
}
