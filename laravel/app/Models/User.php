<?php

namespace App\Models;

// use Illuminate\Contracts\Auth\MustVerifyEmail;
use Database\Factories\UserFactory;
use Filament\Models\Contracts\FilamentUser;
use Filament\Models\Contracts\HasName;
use Filament\Panel;
use Illuminate\Database\Eloquent\Attributes\Fillable;
use Illuminate\Database\Eloquent\Attributes\Hidden;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\Relations\HasOne;
use Illuminate\Database\Eloquent\SoftDeletes;
use Illuminate\Foundation\Auth\User as Authenticatable;
use Illuminate\Notifications\Notifiable;
use Spatie\Permission\Traits\HasRoles;

#[Fillable(['first_name', 'last_name', 'email', 'password', 'status'])]
#[Hidden(['password', 'remember_token'])]
class User extends Authenticatable implements FilamentUser, HasName
{
    use HasFactory, Notifiable, HasRoles, SoftDeletes;

    public function canAccessPanel(Panel $panel): bool
    {
        return $this->hasAnyRole([self::ROLE_ADMIN, self::ROLE_TEACHER]);
    }

    public const ROLE_ADMIN = 'Admin';
    public const ROLE_TEACHER = 'Teacher';
    public const ROLE_STUDENT = 'Student';
    public const ROLE_PARENT = 'Parent';

    public const STATUS_ACTIVE = 1;
    public const STATUS_INACTIVE = 0;

    public const ALL_ROLES = [
        self::ROLE_ADMIN,
        self::ROLE_TEACHER,
        self::ROLE_STUDENT,
        self::ROLE_PARENT,
    ];

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

    /** Filament user menüsü/avatarı üçün görüntülenen ad. */
    public function getFilamentName(): string
    {
        return $this->full_name ?: ($this->email ?? '');
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
            $this->isAdmin() => '/admin/dashboard',
            $this->isTeacher() => '/teacher/dashboard',
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
