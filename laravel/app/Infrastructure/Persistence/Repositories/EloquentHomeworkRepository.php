<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Homework\Homework;
use App\Domain\Homework\HomeworkQuestion;
use App\Domain\Homework\HomeworkRepository;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;

class EloquentHomeworkRepository implements HomeworkRepository
{
    public function create(int $teacherId, array $data): Homework
    {
        return Homework::create([
            'teacher_id' => $teacherId,
            'title' => $data['title'] ?? '',
            'description' => $data['description'] ?? null,
            'is_published' => (bool) ($data['is_published'] ?? false),
        ]);
    }

    public function paginateForUser(int $actingUserId, bool $isAdmin, ?string $search = null, int $perPage = 15): LengthAwarePaginator
    {
        return Homework::query()
            ->when(! $isAdmin, fn (Builder $q): Builder => $q->where('teacher_id', $actingUserId))
            ->with('teacher')
            ->withCount('questions')
            ->when($search, fn (Builder $q): Builder => $q->where('homeworks.title', 'ilike', "%{$search}%"))
            ->orderByDesc('homeworks.created_at')
            ->paginate($perPage);
    }

    public function find(int $id): ?Homework
    {
        return Homework::with(['teacher', 'questions'])->find($id);
    }

    public function update(Homework $homework, array $data): Homework
    {
        $homework->update([
            'title' => $data['title'] ?? $homework->title,
            'description' => array_key_exists('description', $data)
                ? $data['description']
                : $homework->description,
            'is_published' => (bool) ($data['is_published'] ?? $homework->is_published),
        ]);

        return $homework;
    }

    public function delete(Homework $homework): bool
    {
        return (bool) $homework->delete();
    }

    public function questions(Homework $homework): array
    {
        return $homework->questions()->orderBy('position')->get()->all();
    }

    public function replaceQuestions(Homework $homework, array $questions): void
    {
        $homework->questions()->delete();

        foreach ($questions as $index => $q) {
            HomeworkQuestion::create([
                'homework_id' => $homework->id,
                'type' => (int) ($q['type'] ?? 0),
                'position' => $index + 1,
                'text' => $q['text'] ?? '',
                'option_a' => $q['option_a'] ?? null,
                'option_b' => $q['option_b'] ?? null,
                'option_c' => $q['option_c'] ?? null,
                'option_d' => $q['option_d'] ?? null,
                'option_e' => $q['option_e'] ?? null,
                'correct_option' => isset($q['correct_option']) && $q['correct_option'] !== ''
                    ? (int) $q['correct_option']
                    : null,
                'source_question_id' => $q['source_question_id'] ?? null,
                'source_quiz_id' => $q['source_quiz_id'] ?? null,
            ]);
        }
    }
}
