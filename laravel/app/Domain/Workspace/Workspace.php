<?php

namespace App\Domain\Workspace;

use App\Domain\Attendance\Attendance;
use App\Domain\Content\Content;
use App\Domain\User\User;
use App\Domain\WorkspaceFolder\WorkspaceFolder;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\SoftDeletes;

class Workspace extends Model
{
    use SoftDeletes;

    protected $fillable = ['name', 'teacher_id', 'monthly_price'];

    protected function casts(): array
    {
        return [
            'monthly_price' => 'decimal:2',
        ];
    }

    public function teacher(): BelongsTo
    {
        return $this->belongsTo(User::class, 'teacher_id');
    }

    public function students(): BelongsToMany
    {
        return $this->belongsToMany(User::class, 'workspace_students', 'workspace_id', 'student_id')
            ->withPivot(['agreed_price', 'start_date']);
    }

    public function attendances(): HasMany
    {
        return $this->hasMany(Attendance::class, 'workspace_id');
    }

    /** Workspace-in qovluq ağacı. */
    public function folders(): HasMany
    {
        return $this->hasMany(WorkspaceFolder::class, 'workspace_id');
    }

    /** Workspace-ə aid content-lər (quiz, dərs və s.). */
    public function contents(): HasMany
    {
        return $this->hasMany(Content::class, 'workspace_id');
    }
}
