<?php

namespace App\Domain\User;

use App\Domain\Content\Content;
use App\Domain\Workspace\Workspace;
use App\Domain\Student\StudentProfile;
use App\Domain\Question\Question;
use App\Domain\Lesson\Lesson;
use App\Domain\Quiz\Quiz;
use App\Domain\User\Values\UserRole;
use App\Domain\User\Values\UserStatus;

// use Illuminate\Contracts\Auth\MustVerifyEmail;
use Database\Factories\UserFactory;
use Illuminate\Database\Eloquent\Attributes\Fillable;
use Illuminate\Database\Eloquent\Attributes\Hidden;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\Relations\HasOne;
use Illuminate\Database\Eloquent\SoftDeletes;
use Illuminate\Foundation\Auth\User as Authenticatable;
use Illuminate\Notifications\Notifiable;
use Laravel\Sanctum\HasApiTokens;
use Spatie\Permission\Traits\HasRoles;

#[Fillable(['first_name', 'last_name', 'email', 'password', 'status'])]
#[Hidden(['password', 'remember_token'])]
class User extends Authenticatable
{
    use HasApiTokens, HasFactory, Notifiable, HasRoles, SoftDeletes;

    public const ROLE_ADMIN = UserRole::ADMIN;
    public const ROLE_TEACHER = UserRole::TEACHER;
    public const ROLE_STUDENT = UserRole::STUDENT;
    public const ROLE_PARENT = UserRole::PARENT;

    public const STATUS_ACTIVE = UserStatus::ACTIVE;
    public const STATUS_INACTIVE = UserStatus::INACTIVE;
    public const STATUS_SUSPENDED = UserStatus::SUSPENDED;

    public const ALL_ROLES = UserRole::ALL;

    protected function casts(): array
    {
        return [
            'email_verified_at' => 'datetime',
            'password' => 'hashed',
        ];
    }

    public function getFullNameAttribute(): string
    {
        return trim($this->first_name . ' ' . ($this->last_name ?? ''));
    }

    public function isAdmin(): bool
    {
        return $this->hasRole(self::ROLE_ADMIN);
    }

    public function isTeacher(): bool
    {
        return $this->hasRole(self::ROLE_TEACHER);
    }

    public function isStudent(): bool
    {
        return $this->hasRole(self::ROLE_STUDENT);
    }

    public function isParent(): bool
    {
        return $this->hasRole(self::ROLE_PARENT);
    }

    /** Home route for the first role, mirroring AppConstants.HomeRouteFor. */
    public function homeRoute(): string
    {
        return match (true) {
            // Admin/Müəllim özəl (blade) teacher panelinə gedir.
            $this->isAdmin() => '/teacher/dashboard',
            $this->isTeacher() => '/teacher/dashboard',
            // Öğrenci/veli özəl blade panellərində qalır.
            $this->isStudent() => '/student/dashboard',
            $this->isParent() => '/parent/dashboard',
            default => '/blocked',
        };
    }

    // --- Relationships ---

    public function contents(): HasMany
    {
        return $this->hasMany(Content::class, 'teacher_id');
    }

    public function workspaces(): HasMany
    {
        return $this->hasMany(Workspace::class, 'teacher_id');
    }

    public function studentProfile(): HasOne
    {
        return $this->hasOne(StudentProfile::class, 'user_id');
    }

    public function questions(): HasMany
    {
        return $this->hasMany(Question::class, 'teacher_id');
    }

    public function lessons(): HasMany
    {
        return $this->hasMany(Lesson::class, 'teacher_id');
    }

    public function quizzes(): HasMany
    {
        return $this->hasMany(Quiz::class, 'teacher_id');
    }

    public function taughtWorkspaces(): HasMany
    {
        return $this->hasMany(Workspace::class, 'teacher_id');
    }

    /** Students (users with Student role) that this teacher has in workspaces. */
    public function students(): BelongsToMany
    {
        return $this->belongsToMany(
            User::class,
            'workspace_students',
            'workspace_id',
            'student_id'
        );
    }
}
