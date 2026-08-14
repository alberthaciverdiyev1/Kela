<?php

namespace App\Domain\Homework\Values;

use App\Domain\Homework\Enums\HomeworkQuestionType as HomeworkQuestionTypeEnum;

/**
 * @deprecated Use App\Domain\Homework\Enums\HomeworkQuestionType instead.
 */
final class HomeworkQuestionType
{
    public const TASK = HomeworkQuestionTypeEnum::TASK->value;
    public const QUIZ = HomeworkQuestionTypeEnum::QUIZ->value;

    public const ALL = [
        self::TASK,
        self::QUIZ,
    ];

    public static function label(int $type): string
    {
        return HomeworkQuestionTypeEnum::labelFor($type);
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
