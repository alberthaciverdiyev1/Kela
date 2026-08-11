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
}
