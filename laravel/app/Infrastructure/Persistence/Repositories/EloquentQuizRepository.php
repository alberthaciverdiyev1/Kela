<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Quiz\Quiz;
use App\Domain\Quiz\QuizRepository;

class EloquentQuizRepository implements QuizRepository
{
    public function create(int $contentId, int $teacherId, array $data): Quiz
    {
        return Quiz::create([
            'content_id' => $contentId,
            'teacher_id' => $teacherId,
            'title' => $data['title'] ?? '',
            'description' => $data['description'] ?? null,
            'is_published' => (bool) ($data['is_published'] ?? false),
        ]);
    }

    public function find(int $contentId): ?Quiz
    {
        return Quiz::with(['content', 'questions'])->find($contentId);
    }

    public function update(Quiz $quiz, array $data): Quiz
    {
        $quiz->update([
            'title' => $data['title'] ?? $quiz->title,
            'description' => $data['description'] ?? $quiz->description,
            'is_published' => (bool) ($data['is_published'] ?? $quiz->is_published),
        ]);

        return $quiz;
    }

    public function delete(Quiz $quiz): bool
    {
        return (bool) $quiz->delete();
    }

    public function addQuestion(Quiz $quiz, int $questionId, int $position): void
    {
        $quiz->questions()->attach($questionId, ['position' => $position]);
    }

    public function removeQuestion(Quiz $quiz, int $questionId): void
    {
        $quiz->questions()->detach($questionId);
    }

    public function questionIds(Quiz $quiz): array
    {
        return $quiz->questions()->orderByPivot('position')->pluck('questions.id')->map(fn ($id) => (int) $id)->all();
    }
}
