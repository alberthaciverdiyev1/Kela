<?php

namespace App\Policies;

use App\Domain\User\User;

/**
 * İstifadəçi (şagird) resursu üçün siyasət.
 * Admin və Teacher şagirdləri görə/yarada/dəyişə bilər; silmə yalnız Admin.
 */
class UserPolicy
{
    public function viewAny(User $actor): bool
    {
        return $actor->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]);
    }

    public function view(User $actor, User $user): bool
    {
        return $actor->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]);
    }

    public function create(User $actor): bool
    {
        return $actor->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]);
    }

    public function update(User $actor, User $user): bool
    {
        return $actor->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]);
    }

    public function delete(User $actor, User $user): bool
    {
        return $actor->isAdmin();
    }

    public function deleteAny(User $actor): bool
    {
        return $actor->isAdmin();
    }

    public function restore(User $actor, User $user): bool
    {
        return $actor->isAdmin();
    }

    public function forceDelete(User $actor, User $user): bool
    {
        return $actor->isAdmin();
    }
}
