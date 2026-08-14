<?php

namespace App\Domain\Question\Enums;

/**
 * Sual variantları üçün PHP 8.3 Backed Enum.
 */
enum QuestionOption: string
{
    case A = 'A';
    case B = 'B';
    case C = 'C';
    case D = 'D';
    case E = 'E';

    /**
     * @return list<string>
     */
    public static function values(): array
    {
        return array_column(self::cases(), 'value');
    }

    public static function isValid(?string $option): bool
    {
        if ($option === null) {
            return false;
        }

        return self::tryFrom(strtoupper($option)) !== null;
    }
}
