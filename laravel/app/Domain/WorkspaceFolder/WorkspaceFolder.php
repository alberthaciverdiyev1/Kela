<?php

namespace App\Domain\WorkspaceFolder;

use App\Domain\Content\Content;
use App\Domain\Workspace\Workspace;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;
use Illuminate\Database\Eloquent\SoftDeletes;

/**
 * Workspace qovluğu — workspace (base folder) daxilindəki qovluq ağacı.
 * İçərisində quiz, dərs və digər content-lər ola bilər (contents.folder_id).
 * Sual/quiz/dərs bankı qovluqlarından müstəqildir.
 */
class WorkspaceFolder extends Model
{
    use SoftDeletes;

    protected $fillable = [
        'workspace_id',
        'parent_id',
        'name',
        'position',
    ];

    public function workspace(): BelongsTo
    {
        return $this->belongsTo(Workspace::class, 'workspace_id');
    }

    public function parent(): BelongsTo
    {
        return $this->belongsTo(WorkspaceFolder::class, 'parent_id');
    }

    public function children(): HasMany
    {
        return $this->hasMany(WorkspaceFolder::class, 'parent_id');
    }

    /** Bu qovluqdakı content-lər (quiz, dərs və s.). */
    public function contents(): HasMany
    {
        return $this->hasMany(Content::class, 'folder_id');
    }
}
