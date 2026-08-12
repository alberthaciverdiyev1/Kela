<?php

namespace App\Domain\Lesson;

use App\Domain\Content\Content;
use App\Domain\LessonFolder\LessonFolder;
use App\Domain\User\User;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class Lesson extends Model
{
    public $incrementing = false;
    protected $primaryKey = 'content_id';

    protected $fillable = [
        'content_id',
        'teacher_id',
        'folder_id',
        'video_path',
        'thumbnail_path',
        'duration_seconds',
        'is_published',
        'order_index',
    ];

    protected function casts(): array
    {
        return [
            'is_published' => 'boolean',
        ];
    }

    public function content(): BelongsTo
    {
        return $this->belongsTo(Content::class, 'content_id');
    }

    public function teacher(): BelongsTo
    {
        return $this->belongsTo(User::class, 'teacher_id');
    }

    public function folder(): BelongsTo
    {
        return $this->belongsTo(LessonFolder::class, 'folder_id');
    }

    public function getHasVideoAttribute(): bool
    {
        return !empty($this->video_path);
    }

    public function getDurationLabelAttribute(): string
    {
        $s = (int) $this->duration_seconds;
        $h = intdiv($s, 3600);
        $m = intdiv($s % 3600, 60);
        $sec = $s % 60;
        return $h > 0
            ? sprintf('%d:%02d:%02d', $h, $m, $sec)
            : sprintf('%02d:%02d', $m, $sec);
    }

    /** Dərs silinəndə əlaqəli Content də (soft) silinir — .NET davranışı ilə uyğun. */
    protected static function booted(): void
    {
        static::deleting(function (Lesson $lesson): void {
            $lesson->content?->delete();
        });
    }
}
