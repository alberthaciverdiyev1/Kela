<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Workspace\Workspace;
use App\Domain\Workspace\WorkspaceRepository;
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
