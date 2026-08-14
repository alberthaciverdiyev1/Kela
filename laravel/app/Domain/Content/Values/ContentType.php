<?php

namespace App\Domain\Content\Values;

use App\Domain\Content\Enums\ContentType as ContentTypeEnum;

/**
 * @deprecated Use App\Domain\Content\Enums\ContentType instead.
 */
final class ContentType
{
    public const LESSON = ContentTypeEnum::LESSON->value;
    public const QUIZ = ContentTypeEnum::QUIZ->value;
    public const PDF = ContentTypeEnum::PDF->value;
    public const VIDEO = ContentTypeEnum::VIDEO->value;
    public const LINK = ContentTypeEnum::LINK->value;

    public const ALL = [
        self::LESSON,
        self::QUIZ,
        self::PDF,
        self::VIDEO,
        self::LINK,
    ];

    public static function label(int $type): string
    {
        return ContentTypeEnum::labelFor($type);
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
