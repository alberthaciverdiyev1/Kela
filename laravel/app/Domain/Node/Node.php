<?php

namespace App\Domain\Node;

use App\Domain\User\User;
use App\Domain\Node\Node;
use App\Domain\Content\Content;
use App\Domain\Workspace\Workspace;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\SoftDeletes;

class Node extends Model
{
    use SoftDeletes;

    public const KIND_FOLDER = 0;
    public const KIND_CONTENT = 1;

    protected $fillable = [
        'workspace_id',
        'teacher_id',
        'parent_id',
        'name',
        'kind',
        'position',
        'content_id',
    ];

    public function teacher(): BelongsTo
    {
        return $this->belongsTo(User::class, 'teacher_id');
    }

    public function parent(): BelongsTo
    {
        return $this->belongsTo(Node::class, 'parent_id');
    }

    public function children(): HasMany
    {
        return $this->hasMany(Node::class, 'parent_id');
    }

    public function content(): BelongsTo
    {
        return $this->belongsTo(Content::class, 'content_id');
    }

    public function workspace(): BelongsTo
    {
        return $this->belongsTo(Workspace::class, 'workspace_id');
    }

    public function isFolder(): bool
    {
        return $this->kind === self::KIND_FOLDER;
    }

    public function isContent(): bool
    {
        return $this->kind === self::KIND_CONTENT;
    }
}
