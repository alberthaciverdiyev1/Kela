<?php

namespace App\Domain\Question;

use App\Domain\Question\Enums\QuestionOption;
use App\Domain\QuestionFolder\QuestionFolder;
use App\Domain\Quiz\Quiz;
use App\Domain\User\User;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;
use Illuminate\Database\Eloquent\SoftDeletes;

class Question extends Model
{
    use SoftDeletes;

    protected $fillable = [
        'teacher_id',
        'folder_id',
        'text',
        'option_a',
        'option_b',
        'option_c',
        'option_d',
        'option_e',
        'correct_option',
        'explanation',
    ];

    public function teacher(): BelongsTo
    {
        return $this->belongsTo(User::class, 'teacher_id');
    }

    /** Sualın yerləşdiyi bank qovluğu (null → kök/sahəsiz). */
    public function folder(): BelongsTo
    {
        return $this->belongsTo(QuestionFolder::class, 'folder_id');
    }

    public function quizzes(): BelongsToMany
    {
        return $this->belongsToMany(Quiz::class, 'quiz_questions', 'question_id', 'quiz_id')
            ->withPivot('position');
    }

    /** All option columns in order (A-E). */
    public function options(): array
    {
        $options = [];
        foreach (QuestionOption::cases() as $case) {
            $key = 'option_' . strtolower($case->value);
            $val = $this->{$key};
            if ($val !== null && $val !== '') {
                $options[$case->value] = $val;
            }
        }

        return $options;
    }
}
