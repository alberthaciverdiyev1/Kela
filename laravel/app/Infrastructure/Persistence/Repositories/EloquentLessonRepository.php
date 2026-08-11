<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Lesson\Lesson;
use App\Domain\Lesson\LessonRepository;
use Illuminate\Database\Eloquent\Builder;

class EloquentLessonRepository implements LessonRepository
{
    public function create(array $attributes): Lesson
    {
        return Lesson::create($attributes);
    }

    public function update(Lesson $lesson, array $attributes): Lesson
    {
        $lesson->update($attributes);
        return $lesson;
    }

    public function delete(Lesson $lesson): bool
    {
        return (bool) $lesson->delete();
    }

    public function find(int $contentId): ?Lesson
    {
        return Lesson::with('content')->find($contentId);
    }

    public function scopeForUser(Builder $query, int $actingUserId, bool $isAdmin): Builder
    {
        return $query
            ->when(! $isAdmin, fn (Builder $q): Builder => $q->where('teacher_id', $actingUserId))
            ->with('content');
    }
}
