<?php

namespace App\Domain\Note;

use App\Domain\User\User;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\SoftDeletes;

/**
 * Şəxsi qeyd (Google Keep üslubu): rəng, sabitləmə, çöp qutusu.
 */
class Note extends Model
{
    use SoftDeletes;

    public const array COLORS = [
        'default',
        'yellow',
        'blue',
        'green',
        'red',
        'purple',
        'teal',
        'orange',
        'gray',
    ];

    protected $fillable = [
        'user_id',
        'title',
        'body',
        'color',
        'is_pinned',
    ];

    protected function casts(): array
    {
        return [
            'is_pinned' => 'boolean',
        ];
    }

    public function user(): BelongsTo
    {
        return $this->belongsTo(User::class, 'user_id');
    }
}
