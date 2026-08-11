<?php

namespace App\Domain\Content\Values;

/**
 * Məzmun tipləri (value object) — model deyil.
 * Web/Blade bu sinfi import edərək Content modelinə toxunmaz.
 */
final class ContentType
{
    public const LESSON = 0;
    public const QUIZ = 1;
    public const PDF = 2;
    public const VIDEO = 3;
    public const LINK = 4;

    public const ALL = [
        self::LESSON,
        self::QUIZ,
        self::PDF,
        self::VIDEO,
        self::LINK,
    ];

    /** Tipin istifadəçi tərəfindən görünən etiketi. */
    public static function label(int $type): string
    {
        return match ($type) {
            self::LESSON => 'Lesson',
            self::QUIZ => 'Quiz',
            self::PDF => 'Pdf',
            self::VIDEO => 'Video',
            self::LINK => 'Link',
            default => 'Unknown',
        };
    }

    public static function isLesson(int $type): bool
    {
        return $type === self::LESSON;
    }

    public static function isQuiz(int $type): bool
    {
        return $type === self::QUIZ;
    }
}
