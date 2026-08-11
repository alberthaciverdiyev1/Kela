<?php

namespace App\Application\Student;

use App\Domain\Student\StudentRepository;
use App\Domain\User\User;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;
use Illuminate\Support\Collection;

/**
 * Şagirdlərlə bağlı tətbiq səviyyəli əməliyyatlar (use cases).
 * Web/API bu servisi çağırır — modellərə birbaşa toxunmaz.
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

    /** Şagird redaktor formu üçün bütün sahə dəyərləri. */
    public function formData(int $id): array
    {
        $student = $this->students->find($id);
        if ($student === null) {
            return [];
        }

        return [
            'first_name' => $student->first_name,
            'last_name' => $student->last_name,
            'email' => $student->email,
            'status' => (int) $student->status,
            'city_id' => $student->studentProfile?->city_id,
            'birth_date' => $student->studentProfile?->birth_date?->format('Y-m-d'),
        ];
    }

    /** Admin cədvəli üçün axtarış + səhifələnmiş şagird siyahısı. */
    public function paginate(?string $search = null, int $perPage = 15): LengthAwarePaginator
    {
        return $this->students
            ->paginate($search, $perPage)
            ->through(fn (User $student): array => [
                'id' => (int) $student->id,
                'full_name' => $student->full_name,
                'email' => $student->email,
                'city' => $student->studentProfile?->city?->name('az'),
                'birth_date' => $student->studentProfile?->birth_date?->format('d M Y'),
                'status' => (int) $student->status,
            ]);
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
