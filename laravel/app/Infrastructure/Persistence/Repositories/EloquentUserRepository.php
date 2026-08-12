<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\User\User;
use App\Domain\User\UserRepository;

class EloquentUserRepository implements UserRepository
{
    public function roleCount(string $role): int
    {
        return User::role($role)->count();
    }

    public function findByEmail(string $email): ?User
    {
        return User::query()->where('email', $email)->first();
    }

    /** Yeni müəllim hesabı yaradır (aktiv status + Teacher rolu). */
    public function createTeacher(array $data): User
    {
        $user = User::create([
            'first_name' => $data['first_name'],
            'last_name' => $data['last_name'] ?? null,
            'email' => $data['email'],
            'password' => $data['password'],
            'status' => User::STATUS_ACTIVE,
        ]);
        $user->assignRole(User::ROLE_TEACHER);

        return $user;
    }
}
