<?php

namespace App\Domain\Homework\Values;

/**
 * Ev tapşırığı sual tipləri (value object) — model deyil.
 *
 *   TASK — əl ilə yazılmış, variantsız tapşırıq sualı.
 *   QUIZ — quizdən götürülmüş, variantlı (çoxseçimli) sual.
 */
final class HomeworkQuestionType
{
    public const TASK = 0;
    public const QUIZ = 1;

    public const ALL = [
        self::TASK,
        self::QUIZ,
    ];

    /** Tipin istifadəçi tərəfindən görünən etiketi. */
    public static function label(int $type): string
    {
        return match ($type) {
            self::TASK => 'Tapşırıq',
            self::QUIZ => 'Quiz sualı',
            default => 'Naməlum',
        };
    }

    public static function isTask(int $type): bool
    {
        return $type === self::TASK;
    }

    public static function isQuiz(int $type): bool
    {
        return $type === self::QUIZ;
    }
}
