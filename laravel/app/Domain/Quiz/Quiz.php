<?php

namespace App\Domain\Quiz;

use App\Domain\Content\Content;
use App\Domain\User\User;
use App\Domain\Question\Question;
use App\Domain\QuizFolder\QuizFolder;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;

class Quiz extends Model
{
    public $incrementing = false;
    protected $primaryKey = 'content_id';

    protected $fillable = [
        'content_id',
        'teacher_id',
        'folder_id',
        'title',
        'description',
        'is_published',
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

    /** Quiz-in yerləşdiyi qovluq (null → kök/sahəsiz). */
    public function folder(): BelongsTo
    {
        return $this->belongsTo(QuizFolder::class, 'folder_id');
    }

    public function questions(): BelongsToMany
    {
        return $this->belongsToMany(Question::class, 'quiz_questions', 'quiz_id', 'question_id')
            ->withPivot('position')
            ->orderByPivot('position');
    }
}
