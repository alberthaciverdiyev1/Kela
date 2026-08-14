<?php

namespace App\Domain\Homework;

use App\Domain\User\User;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Homework extends Model
{
    protected $table = 'homeworks';

    protected $fillable = [
        'teacher_id',
        'title',
        'description',
        'is_published',
    ];

    protected function casts(): array
    {
        return [
            'is_published' => 'boolean',
        ];
    }

    public function teacher(): BelongsTo
    {
        return $this->belongsTo(User::class, 'teacher_id');
    }

    /** Ev tapşırığının sualları (sıra ilə). */
    public function questions(): HasMany
    {
        return $this->hasMany(HomeworkQuestion::class, 'homework_id')->orderBy('position');
    }
}
