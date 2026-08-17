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

    /** Mövcud istifadəçinin məlumatlarını yeniləyir. */
    public function update(User $user, array $data): User
    {
        if (isset($data['first_name'])) $user->first_name = $data['first_name'];
        if (array_key_exists('last_name', $data)) $user->last_name = $data['last_name'];
        if (isset($data['email'])) $user->email = $data['email'];
        if (isset($data['password'])) $user->password = $data['password'];
        if (array_key_exists('avatar', $data)) $user->avatar = $data['avatar'];

        $user->save();

        return $user;
    }
}
