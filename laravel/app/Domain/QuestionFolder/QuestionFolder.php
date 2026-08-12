<?php

namespace App\Domain\QuestionFolder;

use App\Domain\User\User;
use App\Domain\Question\Question;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\SoftDeletes;

/**
 * Sual bankı qovluğu — sualları təşkil etmək üçün istifadəçinin öz qovluq ağacı.
 * Workspace node-larından müstəqildir (ayrıca modul).
 */
class QuestionFolder extends Model
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
        return $this->belongsTo(QuestionFolder::class, 'parent_id');
    }

    public function children(): HasMany
    {
        return $this->hasMany(QuestionFolder::class, 'parent_id');
    }

    /** Bu qovluqdakı suallar. */
    public function questions(): HasMany
    {
        return $this->hasMany(Question::class, 'folder_id');
    }
}
