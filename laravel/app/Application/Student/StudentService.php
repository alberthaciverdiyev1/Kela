<?php

namespace App\Application\Student;

use App\Domain\Student\StudentRepository;
use App\Domain\User\User;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;
use Illuminate\Support\Collection;
use Illuminate\Support\Str;

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

    /**
     * Toplu şagird generasiyası.
     *
     * @param array $spec count, first_name (baza), last_name, email_prefix,
     *                    password (ortaq — boşsa hər şagirdə avtomatik), status, city_id
     * @return array{users: Collection<int, User>, rows: array<int, array{first_name: string, last_name: string, email: string, password: string}>}
     */
    public function generateMany(array $spec): array
    {
        $count = max(1, (int) ($spec['count'] ?? 1));

        $baseFirstName = trim((string) ($spec['first_name'] ?? ''));
        if ($baseFirstName === '') {
            $baseFirstName = 'Şagird';
        }
        $baseLastName = trim((string) ($spec['last_name'] ?? ''));

        $prefix = trim((string) ($spec['email_prefix'] ?? ''));
        if ($prefix === '') {
            $prefix = Str::slug($baseFirstName, '_') ?: 'sagird';
        }

        $sharedPassword = trim((string) ($spec['password'] ?? ''));
        $status = (int) ($spec['status'] ?? User::STATUS_ACTIVE);
        $cityId = ! empty($spec['city_id']) ? (int) $spec['city_id'] : null;

        $usedEmails = $this->students->allEmails();
        $users = collect();
        $rows = [];

        for ($i = 1; $i <= $count; $i++) {
            $name = $count > 1 ? "{$baseFirstName} {$i}" : $baseFirstName;
            $email = $this->uniqueEmail($prefix, $i, $usedEmails);
            $plainPassword = $sharedPassword !== '' ? $sharedPassword : Str::random(10);

            $user = $this->students->create([
                'first_name' => $name,
                'last_name' => $baseLastName !== '' ? $baseLastName : null,
                'email' => $email,
                'password' => $plainPassword,
                'status' => $status,
                'city_id' => $cityId,
            ]);

            $users->push($user);
            $rows[] = [
                'first_name' => $name,
                'last_name' => $baseLastName,
                'email' => $email,
                'password' => $plainPassword,
            ];
        }

        return ['users' => $users, 'rows' => $rows];
    }

    /** Verilən prefiksdən başlayaraq mövcud olmayan unikal e-poçt qaytarır. */
    protected function uniqueEmail(string $prefix, int $start, \Illuminate\Support\Collection $used): string
    {
        $domain = config('app.student_email_domain', 'kela.az');
        $i = $start;

        while (true) {
            $candidate = "{$prefix}{$i}@{$domain}";
            if (! $used->contains($candidate)) {
                $used->push($candidate);

                return $candidate;
            }
            $i++;
        }
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
                'first_name' => $student->first_name,
                'last_name' => $student->last_name,
                'full_name' => $student->full_name,
                'email' => $student->email,
                'city' => $student->studentProfile?->city?->name('az'),
                'city_id' => $student->studentProfile?->city_id,
                'birth_date' => $student->studentProfile?->birth_date?->format('d M Y'),
                'birth_date_iso' => $student->studentProfile?->birth_date?->format('Y-m-d'),
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
