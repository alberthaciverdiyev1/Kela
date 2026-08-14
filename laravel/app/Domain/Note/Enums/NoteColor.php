<?php

namespace App\Domain\Note\Enums;

/**
 * Qeyd rəngləri üçün PHP 8.3 Backed Enum (Google Keep üslubu).
 */
enum NoteColor: string
{
    case DEFAULT = 'default';
    case YELLOW = 'yellow';
    case BLUE = 'blue';
    case GREEN = 'green';
    case RED = 'red';
    case PURPLE = 'purple';
    case TEAL = 'teal';
    case ORANGE = 'orange';
    case GRAY = 'gray';

    /**
     * @return list<string>
     */
    public static function values(): array
    {
        return array_column(self::cases(), 'value');
    }

    public static function isValid(string $color): bool
    {
        return self::tryFrom($color) !== null;
    }
}
