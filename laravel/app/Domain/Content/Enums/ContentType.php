<?php

namespace App\Domain\Content\Enums;

/**
 * Məzmun tipləri üçün PHP 8.3 Backed Enum.
 */
enum ContentType: int
{
    case LESSON = 0;
    case QUIZ = 1;
    case PDF = 2;
    case VIDEO = 3;
    case LINK = 4;

    public function label(): string
    {
        return match ($this) {
            self::LESSON => 'Lesson',
            self::QUIZ => 'Quiz',
            self::PDF => 'Pdf',
            self::VIDEO => 'Video',
            self::LINK => 'Link',
        };
    }

    public static function labelFor(int|self $type): string
    {
        $enum = is_int($type) ? self::tryFrom($type) : $type;
        return $enum?->label() ?? 'Unknown';
    }

    public function isLesson(): bool
    {
        return $this === self::LESSON;
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
