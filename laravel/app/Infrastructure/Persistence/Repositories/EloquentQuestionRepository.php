<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Question\Question;
use App\Domain\Question\QuestionRepository;
use Illuminate\Support\Collection;

class EloquentQuestionRepository implements QuestionRepository
{
    public function create(int $teacherId, array $data): Question
    {
        return Question::create([
            'teacher_id' => $teacherId,
            'text' => $data['text'],
            'option_a' => $data['option_a'] ?? null,
            'option_b' => $data['option_b'] ?? null,
            'option_c' => $data['option_c'] ?? null,
            'option_d' => $data['option_d'] ?? null,
            'option_e' => $data['option_e'] ?? null,
            'correct_option' => (int) ($data['correct_option'] ?? 0),
        ]);
    }

    public function update(Question $question, array $data): Question
    {
        $question->update([
            'text' => $data['text'] ?? $question->text,
            'option_a' => $data['option_a'] ?? $question->option_a,
            'option_b' => $data['option_b'] ?? $question->option_b,
            'option_c' => $data['option_c'] ?? $question->option_c,
            'option_d' => $data['option_d'] ?? $question->option_d,
            'option_e' => $data['option_e'] ?? $question->option_e,
            'correct_option' => (int) ($data['correct_option'] ?? $question->correct_option),
        ]);

        return $question;
    }

    public function delete(Question $question): bool
    {
        return (bool) $question->delete();
    }

    public function find(int $id): ?Question
    {
        return Question::find($id);
    }

    public function listForTeacher(int $teacherId, ?string $search = null): Collection
    {
        return Question::query()
            ->where('teacher_id', $teacherId)
            ->when($search, fn ($q) => $q->where('text', 'ilike', "%{$search}%"))
            ->withCount('quizzes')
            ->orderBy('created_at', 'desc')
            ->get();
    }

    public function usedInQuizzes(Question $question): int
    {
        return (int) $question->quizzes()->count();
    }
}
