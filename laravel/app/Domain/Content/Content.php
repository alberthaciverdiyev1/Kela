<?php

namespace App\Domain\Content;

use App\Domain\Content\Values\ContentType;
use App\Domain\User\User;
use App\Domain\Lesson\Lesson;
use App\Domain\Quiz\Quiz;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasOne;
use Illuminate\Database\Eloquent\SoftDeletes;

class Content extends Model
{
    use SoftDeletes;

    public const TYPE_LESSON = ContentType::LESSON;
    public const TYPE_QUIZ = ContentType::QUIZ;
    public const TYPE_PDF = ContentType::PDF;
    public const TYPE_VIDEO = ContentType::VIDEO;
    public const TYPE_LINK = ContentType::LINK;

    public const ALL_TYPES = ContentType::ALL;

    protected $fillable = [
        'teacher_id',
        'title',
        'description',
        'type',
        'url',
        'is_published',
    ];

    protected function casts(): array
    {
        return [
            'is_published' => 'boolean',
        ];
    }

    public function teacher(): BelongsTo
    {
        return $this->belongsTo(User::class, 'teacher_id');
    }

    public function lesson(): HasOne
    {
        return $this->hasOne(Lesson::class, 'content_id');
    }

    public function quiz(): HasOne
    {
        return $this->hasOne(Quiz::class, 'content_id');
    }

    public function isLesson(): bool
    {
        return $this->type === self::TYPE_LESSON;
    }

    public function isQuiz(): bool
    {
        return $this->type === self::TYPE_QUIZ;
    }

    public function typeLabel(): string
    {
        return ContentType::label($this->type);
    }
}
