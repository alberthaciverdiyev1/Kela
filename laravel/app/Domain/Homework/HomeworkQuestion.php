<?php

namespace App\Domain\Homework;

use App\Domain\Homework\Values\HomeworkQuestionType;
use App\Domain\Question\Question;
use App\Domain\Quiz\Quiz;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class HomeworkQuestion extends Model
{
    protected $fillable = [
        'homework_id',
        'type',
        'position',
        'text',
        'option_a',
        'option_b',
        'option_c',
        'option_d',
        'option_e',
        'correct_option',
        'source_question_id',
        'source_quiz_id',
    ];

    public function homework(): BelongsTo
    {
        return $this->belongsTo(Homework::class, 'homework_id');
    }

    public function sourceQuestion(): BelongsTo
    {
        return $this->belongsTo(Question::class, 'source_question_id');
    }

    public function sourceQuiz(): BelongsTo
    {
        return $this->belongsTo(Quiz::class, 'source_quiz_id', 'content_id');
    }

    /** İstifadə olunmuş variantlar (A–E, yalnız quiz sualları üçün). */
    public function options(): array
    {
        return array_filter([
            'A' => $this->option_a,
            'B' => $this->option_b,
            'C' => $this->option_c,
            'D' => $this->option_d,
            'E' => $this->option_e,
        ], fn ($v) => $v !== null && $v !== '');
    }

    public function isTask(): bool
    {
        return HomeworkQuestionType::isTask((int) $this->type);
    }

    public function isQuizSourced(): bool
    {
        return HomeworkQuestionType::isQuiz((int) $this->type);
    }
}
