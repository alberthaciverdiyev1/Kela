<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;
use Illuminate\Database\Eloquent\SoftDeletes;

class Question extends Model
{
    use SoftDeletes;

    protected $fillable = [
        'teacher_id',
        'text',
        'option_a',
        'option_b',
        'option_c',
        'option_d',
        'option_e',
        'correct_option',
    ];

    public function teacher(): BelongsTo
    {
        return $this->belongsTo(User::class, 'teacher_id');
    }

    public function quizzes(): BelongsToMany
    {
        return $this->belongsToMany(Quiz::class, 'quiz_questions', 'question_id', 'quiz_id')
            ->withPivot('position');
    }

    /** All option columns in order. */
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
}
