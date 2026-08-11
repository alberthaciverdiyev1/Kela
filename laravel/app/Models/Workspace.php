<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\BelongsToMany;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\SoftDeletes;

class Workspace extends Model
{
    use SoftDeletes;

    protected $fillable = ['name', 'teacher_id'];

    public function teacher(): BelongsTo
    {
        return $this->belongsTo(User::class, 'teacher_id');
    }

    public function students(): BelongsToMany
    {
        return $this->belongsToMany(User::class, 'workspace_students', 'workspace_id', 'student_id');
    }

    public function nodes(): HasMany
    {
        return $this->hasMany(Node::class, 'workspace_id');
    }

    public function attendances(): HasMany
    {
        return $this->hasMany(Attendance::class, 'workspace_id');
    }
}
