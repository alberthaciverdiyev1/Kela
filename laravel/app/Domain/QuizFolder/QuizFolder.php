<?php

namespace App\Domain\QuizFolder;

use App\Domain\User\User;
use App\Domain\Quiz\Quiz;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\SoftDeletes;

/**
 * Quiz qovluğu — quizləri təşkil etmək üçün istifadəçinin öz qovluq ağacı.
 * Sual bankı qovluqlarından (QuestionFolder) müstəqildir.
 */
class QuizFolder extends Model
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
        return $this->belongsTo(QuizFolder::class, 'parent_id');
    }

    public function children(): HasMany
    {
        return $this->hasMany(QuizFolder::class, 'parent_id');
    }

    /** Bu qovluqdakı quizlər. */
    public function quizzes(): HasMany
    {
        return $this->hasMany(Quiz::class, 'folder_id');
    }
}
