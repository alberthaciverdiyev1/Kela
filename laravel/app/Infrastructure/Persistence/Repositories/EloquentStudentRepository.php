<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Student\StudentRepository;
use App\Domain\User\User;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Support\Collection;
use Illuminate\Support\Str;

class EloquentStudentRepository implements StudentRepository
{
    public function create(array $data): User
    {
        $user = User::create([
            'first_name' => $data['first_name'],
            'last_name' => $data['last_name'] ?? null,
            'email' => $data['email'],
            'password' => $data['password'] ?? Str::random(16),
            'status' => $data['status'] ?? User::STATUS_ACTIVE,
        ]);
        $user->assignRole(User::ROLE_STUDENT);

        // İsteğe bağlı profil məlumatları varsa StudentProfile yaradılır.
        if (isset($data['city_id']) || isset($data['birth_date'])) {
            $user->studentProfile()->create([
                'city_id' => $data['city_id'] ?? null,
                'birth_date' => $data['birth_date'] ?? null,
            ]);
        }

        return $user->load('studentProfile');
    }

    public function update(int $id, array $data): User
    {
        $user = User::findOrFail($id);
        $user->update([
            'first_name' => $data['first_name'] ?? $user->first_name,
            'last_name' => $data['last_name'] ?? $user->last_name,
            'email' => $data['email'] ?? $user->email,
            'status' => $data['status'] ?? $user->status,
        ]);

        // Şifrə verilibsə yenilənir (boş gələrsə dəyişilmir).
        if (! empty($data['password'])) {
            $user->update(['password' => $data['password']]);
        }

        // Profil yoxdursa yaradılır, varsa güncəllənir.
        $profile = $user->studentProfile()->first();
        $profileData = [
            'city_id' => $data['city_id'] ?? $profile?->city_id,
            'birth_date' => $data['birth_date'] ?? $profile?->birth_date,
        ];

        if ($profile) {
            $profile->update($profileData);
        } elseif (isset($data['city_id']) || isset($data['birth_date'])) {
            $user->studentProfile()->create($profileData);
        }

        return $user->load('studentProfile');
    }

    public function find(int $id): ?User
    {
        return User::with('studentProfile')->find($id);
    }

    public function delete(int $id): bool
    {
        $user = User::find($id);
        if (! $user) {
            return false;
        }

        $user->studentProfile()?->delete();

        return (bool) $user->delete();
    }

    public function all(): Collection
    {
        return User::role(User::ROLE_STUDENT)
            ->with('studentProfile')
            ->orderBy('first_name')
            ->get();
    }

    public function paginate(?string $search = null, int $perPage = 15): LengthAwarePaginator
    {
        return User::role(User::ROLE_STUDENT)
            ->with('studentProfile.city')
            ->when($search, fn ($q) => $q->where(function ($q) use ($search) {
                $q->where('first_name', 'ilike', "%{$search}%")
                    ->orWhere('last_name', 'ilike', "%{$search}%")
                    ->orWhere('email', 'ilike', "%{$search}%");
            }))
            ->orderBy('first_name')
            ->paginate($perPage);
    }

    public function availableForWorkspace(?string $search = null): Collection
    {
        return User::role(User::ROLE_STUDENT)
            ->when($search, fn ($q) => $q->where(function ($q) use ($search) {
                $q->where('first_name', 'ilike', "%{$search}%")
                    ->orWhere('last_name', 'ilike', "%{$search}%")
                    ->orWhere('email', 'ilike', "%{$search}%");
            }))
            ->orderBy('first_name')
            ->get(['users.id', 'users.first_name', 'users.last_name', 'users.email']);
    }

    public function allEmails(): Collection
    {
        return User::query()->pluck('email');
    }
}
