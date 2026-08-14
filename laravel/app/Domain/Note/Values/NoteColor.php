<?php

namespace App\Domain\Note\Values;

use App\Domain\Note\Enums\NoteColor as NoteColorEnum;

/**
 * @deprecated Use App\Domain\Note\Enums\NoteColor instead.
 */
final class NoteColor
{
    public const DEFAULT = NoteColorEnum::DEFAULT->value;
    public const YELLOW = NoteColorEnum::YELLOW->value;
    public const BLUE = NoteColorEnum::BLUE->value;
    public const GREEN = NoteColorEnum::GREEN->value;
    public const RED = NoteColorEnum::RED->value;
    public const PURPLE = NoteColorEnum::PURPLE->value;
    public const TEAL = NoteColorEnum::TEAL->value;
    public const ORANGE = NoteColorEnum::ORANGE->value;
    public const GRAY = NoteColorEnum::GRAY->value;

    public const ALL = [
        self::DEFAULT,
        self::YELLOW,
        self::BLUE,
        self::GREEN,
        self::RED,
        self::PURPLE,
        self::TEAL,
        self::ORANGE,
        self::GRAY,
    ];

    public static function isValid(string $color): bool
    {
        return NoteColorEnum::isValid($color);
    }
}
