<?php

namespace App\Domain\Content;

use App\Domain\User\User;
use App\Domain\Node\Node;
use App\Domain\Lesson\Lesson;
use App\Domain\Quiz\Quiz;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\Relations\HasOne;
use Illuminate\Database\Eloquent\SoftDeletes;

class Content extends Model
{
    use SoftDeletes;

    public const TYPE_LESSON = 0;
    public const TYPE_QUIZ = 1;
    public const TYPE_PDF = 2;
    public const TYPE_VIDEO = 3;
    public const TYPE_LINK = 4;

    public const ALL_TYPES = [
        self::TYPE_LESSON,
        self::TYPE_QUIZ,
        self::TYPE_PDF,
        self::TYPE_VIDEO,
        self::TYPE_LINK,
    ];

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

    public function nodes(): HasMany
    {
        return $this->hasMany(Node::class, 'content_id');
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
        return match ($this->type) {
            self::TYPE_LESSON => 'Lesson',
            self::TYPE_QUIZ => 'Quiz',
            self::TYPE_PDF => 'Pdf',
            self::TYPE_VIDEO => 'Video',
            self::TYPE_LINK => 'Link',
            default => 'Unknown',
        };
    }
}
