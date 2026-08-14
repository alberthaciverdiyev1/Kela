<?php

namespace App\Domain\Question\Values;

use App\Domain\Question\Enums\QuestionOption as QuestionOptionEnum;

/**
 * @deprecated Use App\Domain\Question\Enums\QuestionOption instead.
 */
final class QuestionOption
{
    public const A = QuestionOptionEnum::A->value;
    public const B = QuestionOptionEnum::B->value;
    public const C = QuestionOptionEnum::C->value;
    public const D = QuestionOptionEnum::D->value;
    public const E = QuestionOptionEnum::E->value;

    public const ALL = [
        self::A,
        self::B,
        self::C,
        self::D,
        self::E,
    ];

    public static function isValid(?string $option): bool
    {
        return QuestionOptionEnum::isValid($option);
    }
}
