<?php

namespace App\Domain\Homework\Enums;

/**
 * Ev tapşırığı sual tipləri üçün PHP 8.3 Backed Enum.
 */
enum HomeworkQuestionType: int
{
    case TASK = 0;
    case QUIZ = 1;

    public function label(): string
    {
        return match ($this) {
            self::TASK => 'Tapşırıq',
            self::QUIZ => 'Quiz sualı',
        };
    }

    public static function labelFor(int|self $type): string
    {
        $enum = is_int($type) ? self::tryFrom($type) : $type;
        return $enum?->label() ?? 'Naməlum';
    }

    public function isTask(): bool
    {
        return $this === self::TASK;
    }

    public function isQuiz(): bool
    {
        return $this === self::QUIZ;
    }

    /**
     * @return list<int>
     */
    public static function values(): array
    {
        return array_column(self::cases(), 'value');
    }

    public static function isValid(int $type): bool
    {
        return self::tryFrom($type) !== null;
    }
}
