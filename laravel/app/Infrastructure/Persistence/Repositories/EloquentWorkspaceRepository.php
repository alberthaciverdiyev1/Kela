<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Workspace\Workspace;
use App\Domain\Workspace\WorkspaceRepository;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;
use Illuminate\Support\Collection;

class EloquentWorkspaceRepository implements WorkspaceRepository
{
    public function listForTeacher(int $teacherId): Collection
    {
        return Workspace::query()
            ->where('teacher_id', $teacherId)
            ->withCount('students')
            ->orderBy('name')
            ->get();
    }

    public function scopeForUser(Builder $query, int $actingUserId, bool $isAdmin): Builder
    {
        return $query
            ->when(! $isAdmin, fn (Builder $q): Builder => $q->where('teacher_id', $actingUserId))
            ->withCount('students');
    }

    public function paginateForUser(int $actingUserId, bool $isAdmin, ?string $search = null, int $perPage = 15): LengthAwarePaginator
    {
        return Workspace::query()
            ->when(! $isAdmin, fn (Builder $q): Builder => $q->where('teacher_id', $actingUserId))
            ->when($search, fn (Builder $q) => $q->where('name', 'ilike', "%{$search}%"))
            ->withCount('students')
            ->orderBy('name')
            ->paginate($perPage);
    }

    public function create(int $teacherId, string $name): Workspace
    {
        return Workspace::create([
            'teacher_id' => $teacherId,
            'name' => $name,
        ]);
    }

    public function find(int $id): ?Workspace
    {
        return Workspace::find($id);
    }

    public function update(Workspace $workspace, string $name): Workspace
    {
        $workspace->update(['name' => $name]);

        return $workspace;
    }

    public function delete(Workspace $workspace): bool
    {
        return (bool) $workspace->delete();
    }

    public function attachStudents(Workspace $workspace, array $studentIds): void
    {
        $studentIds = array_values(array_filter(array_map('intval', $studentIds)));
        if ($studentIds !== []) {
            $workspace->students()->syncWithoutDetaching($studentIds);
        }
    }

    public function detachStudent(Workspace $workspace, int $studentId): void
    {
        $workspace->students()->detach($studentId);
    }

    public function studentIds(Workspace $workspace): array
    {
        return $workspace->students()->pluck('users.id')->map(fn ($id) => (int) $id)->all();
    }

    public function students(Workspace $workspace): Collection
    {
        return $workspace->students()
            ->orderBy('first_name')
            ->get(['users.id', 'users.first_name', 'users.last_name', 'users.email']);
    }
}
