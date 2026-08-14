<?php

namespace App\Domain\Content;

use App\Domain\Content\Enums\ContentType;
use App\Domain\Lesson\Lesson;
use App\Domain\Quiz\Quiz;
use App\Domain\User\User;
use App\Domain\Workspace\Workspace;
use App\Domain\WorkspaceFolder\WorkspaceFolder;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasOne;
use Illuminate\Database\Eloquent\SoftDeletes;

class Content extends Model
{
    use SoftDeletes;

    public const TYPE_LESSON = ContentType::LESSON->value;
    public const TYPE_QUIZ = ContentType::QUIZ->value;
    public const TYPE_PDF = ContentType::PDF->value;
    public const TYPE_VIDEO = ContentType::VIDEO->value;
    public const TYPE_LINK = ContentType::LINK->value;

    public const ALL_TYPES = [
        self::TYPE_LESSON,
        self::TYPE_QUIZ,
        self::TYPE_PDF,
        self::TYPE_VIDEO,
        self::TYPE_LINK,
    ];

    protected $fillable = [
        'teacher_id',
        'workspace_id',
        'folder_id',
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

    /** Content-in aid olduğu workspace (base folder). */
    public function workspace(): BelongsTo
    {
        return $this->belongsTo(Workspace::class, 'workspace_id');
    }

    /** Workspace daxilindəki qovluq (null = workspace kökü). */
    public function folder(): BelongsTo
    {
        return $this->belongsTo(WorkspaceFolder::class, 'folder_id');
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
        return (int) $this->type === self::TYPE_LESSON;
    }

    public function isQuiz(): bool
    {
        return (int) $this->type === self::TYPE_QUIZ;
    }

    public function typeLabel(): string
    {
        return ContentType::labelFor((int) $this->type);
    }
}
