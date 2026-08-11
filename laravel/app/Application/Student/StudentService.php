<?php

namespace App\Application\Student;

use App\Domain\Student\StudentRepository;
use App\Domain\User\User;
use Illuminate\Database\Eloquent\Builder;
use Illuminate\Support\Collection;

/**
 * Şagirdlərlə bağlı tətbiq səviyyəli əməliyyatlar (use cases).
 * Filament bu servisi çağırır — modellərə birbaşa toxunmaz.
 */
class StudentService
{
    public function __construct(private readonly StudentRepository $students)
    {
    }

    /** Yeni şagird yaradır (User + Student rolu + isteğe bağlı profil). */
    public function create(array $data): User
    {
        return $this->students->create($data);
    }

    public function update(int $id, array $data): User
    {
        return $this->students->update($id, $data);
    }

    public function find(int $id): ?User
    {
        return $this->students->find($id);
    }

    public function delete(int $id): void
    {
        $this->students->delete($id);
    }

    /** @return Collection<int, User> */
    public function list(): Collection
    {
        return $this->students->all();
    }

    /** Cədvəl üçün sorğunu yalnız Student roluna məhdudlaşdırır. */
    public function scopeQueryFor(Builder $query): Builder
    {
        return $query
            ->role(User::ROLE_STUDENT)
            ->with('studentProfile.city');
    }
}
