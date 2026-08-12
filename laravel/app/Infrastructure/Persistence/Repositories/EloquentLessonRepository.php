<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Lesson\Lesson;
use App\Domain\Lesson\LessonRepository;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;
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

    public function paginateForUser(int $actingUserId, bool $isAdmin, ?string $search, int $folderId, int $perPage): LengthAwarePaginator
    {
        return Lesson::query()
            ->when(! $isAdmin, fn (Builder $q): Builder => $q->where('lessons.teacher_id', $actingUserId))
            ->with('content')
            ->when($folderId === 0, fn (Builder $q): Builder => $q->whereNull('lessons.folder_id'))
            ->when($folderId > 0, fn (Builder $q): Builder => $q->where('lessons.folder_id', $folderId))
            ->when($search, fn (Builder $q): Builder => $q->whereHas(
                'content',
                fn (Builder $c): Builder => $c->where('title', 'ilike', "%{$search}%")
            ))
            ->orderBy('lessons.order_index')
            ->paginate($perPage);
    }

    public function moveToFolder(Lesson $lesson, ?int $folderId): Lesson
    {
        $lesson->update(['folder_id' => $folderId]);

        return $lesson;
    }
}
