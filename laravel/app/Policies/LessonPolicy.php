<?php

namespace App\Policies;

use App\Models\Lesson;
use App\Models\User;

class LessonPolicy
{
    public function viewAny(User $user): bool
    {
        return $user->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]);
    }

    public function view(User $user, Lesson $lesson): bool
    {
        return $user->isAdmin() || $lesson->teacher_id === $user->id;
    }

    public function create(User $user): bool
    {
        return $user->isAdmin() || $user->isTeacher();
    }

    public function update(User $user, Lesson $lesson): bool
    {
        return $user->isAdmin() || $lesson->teacher_id === $user->id;
    }

    public function delete(User $user, Lesson $lesson): bool
    {
        return $user->isAdmin() || $lesson->teacher_id === $user->id;
    }

    public function deleteAny(User $user): bool
    {
        return $user->isAdmin();
    }

    public function restore(User $user, Lesson $lesson): bool
    {
        return $user->isAdmin();
    }

    public function forceDelete(User $user, Lesson $lesson): bool
    {
        return $user->isAdmin();
    }
}
