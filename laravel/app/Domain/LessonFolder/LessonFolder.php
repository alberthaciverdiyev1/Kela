<?php

namespace App\Domain\LessonFolder;

use App\Domain\Lesson\Lesson;
use App\Domain\User\User;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\SoftDeletes;

/**
 * Dərs qovluğu — dərsləri təşkil etmək üçün istifadəçinin öz qovluq ağacı.
 * Sual/quiz bankı qovluqlarından (QuestionFolder/QuizFolder) müstəqildir.
 */
class LessonFolder extends Model
{
    use SoftDeletes;

    protected $fillable = [
        'teacher_id',
        'parent_id',
        'name',
        'position',
    ];

    public function teacher(): BelongsTo
    {
        return $this->belongsTo(User::class, 'teacher_id');
    }

    public function parent(): BelongsTo
    {
        return $this->belongsTo(LessonFolder::class, 'parent_id');
    }

    public function children(): HasMany
    {
        return $this->hasMany(LessonFolder::class, 'parent_id');
    }

    /** Bu qovluqdakı dərslər. */
    public function lessons(): HasMany
    {
        return $this->hasMany(Lesson::class, 'folder_id');
    }
}
