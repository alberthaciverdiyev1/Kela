<?php

namespace App\Application\WorkspaceFolder;

use App\Domain\Content\Content;
use App\Domain\Content\ContentRepository;
use App\Domain\Workspace\Workspace;
use App\Domain\Workspace\WorkspaceRepository;
use App\Domain\WorkspaceFolder\WorkspaceFolder;
use App\Domain\WorkspaceFolder\WorkspaceFolderRepository;

/**
 * Workspace qovluqları: workspace (base folder) daxilində qovluq ağacı.
 * Hər qovluqda quiz, dərs və digər content-lər ola bilər (contents.folder_id).
 * Bütün əməliyyatlar sahibə (teacher) görə doğrulanır.
 */
class WorkspaceFolderService
{
    public function __construct(
        private readonly WorkspaceFolderRepository $folders,
        private readonly WorkspaceRepository $workspaces,
        private readonly ContentRepository $contents,
    ) {
    }

    /** Workspace-in cari qovluğu: qovluqlar + içindəki content-lər. */
    public function directory(int $workspaceId, ?int $folderId, int $actingUserId): array
    {
        $workspace = $this->assertWorkspaceOwner($workspaceId, $actingUserId);

        if ($folderId !== null) {
            $this->assertFolderOwner($folderId, $workspaceId, $actingUserId);
        }

        $folders = $this->folders->foldersFor($workspaceId, $folderId);

        return [
            'workspace_name' => $workspace->name,
            'breadcrumbs' => $this->folders->breadcrumbs($folderId),
            'folders' => $folders->map(fn (WorkspaceFolder $f) => [
                'id' => (int) $f->id,
                'name' => $f->name,
                'position' => (int) $f->position,
                'content_count' => (int) $f->contents()->count(),
            ])->values()->all(),
            'contents' => $this->contents->contentsForWorkspace($workspaceId, $folderId)
                ->map(fn (Content $c) => [
                    'content_id' => (int) $c->id,
                    'title' => $c->title,
                    'type' => (int) $c->type,
                    'type_label' => $c->typeLabel(),
                    'is_published' => (bool) $c->is_published,
                    'created_at' => $c->created_at?->format('d M Y'),
                ])
                ->values()
                ->all(),
        ];
    }

    public function find(int $folderId): ?WorkspaceFolder
    {
        return $this->folders->find($folderId);
    }

    /** Content-i çıxarır (sahiblik yoxlanmadan — controller özü doğrulayır). */
    public function findContent(int $contentId): ?Content
    {
        return $this->contents->find($contentId);
    }

    public function createFolder(int $workspaceId, string $name, ?int $parentId, int $actingUserId): WorkspaceFolder
    {
        $this->assertWorkspaceOwner($workspaceId, $actingUserId);
        if ($parentId !== null) {
            $this->assertFolderOwner($parentId, $workspaceId, $actingUserId);
        }
        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Qovluq adı boş ola bilməz.');
        }

        return $this->folders->create($workspaceId, $name, $parentId);
    }

    public function renameFolder(int $workspaceId, int $folderId, string $name, int $actingUserId): void
    {
        $folder = $this->assertFolderOwner($folderId, $workspaceId, $actingUserId);
        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Qovluq adı boş ola bilməz.');
        }

        $this->folders->update($folder, ['name' => $name]);
    }

    public function moveFolder(int $workspaceId, int $folderId, ?int $newParentId, int $actingUserId): void
    {
        $folder = $this->assertFolderOwner($folderId, $workspaceId, $actingUserId);

        if ($newParentId !== null) {
            $parent = $this->assertFolderOwner($newParentId, $workspaceId, $actingUserId);
            if (in_array($parent->id, $this->folders->descendantIds($folderId), true)) {
                throw new \RuntimeException('Qovluq öz daxilinə daşına bilməz.');
            }
        }

        $this->folders->update($folder, ['parent_id' => $newParentId]);
    }

    public function deleteFolder(int $workspaceId, int $folderId, int $actingUserId): void
    {
        $this->assertFolderOwner($folderId, $workspaceId, $actingUserId);
        $this->folders->deleteTree($folderId);
    }

    /** Move dropdown üçün workspace-in bütün qovluq ağacı. */
    public function folderTree(int $workspaceId, int $actingUserId, ?int $excludeFolderId = null): array
    {
        $this->assertWorkspaceOwner($workspaceId, $actingUserId);
        $folders = $this->folders->allFoldersFor($workspaceId);
        $byParent = $folders->groupBy(fn (WorkspaceFolder $f) => $f->parent_id ?? 0);

        $result = [];
        $walk = function (int $parentKey, int $depth) use (&$walk, &$result, $byParent, $excludeFolderId): void {
            foreach ($byParent->get($parentKey, collect()) as $folder) {
                if ($folder->id === $excludeFolderId) {
                    continue;
                }
                $result[] = ['id' => (int) $folder->id, 'name' => $folder->name, 'depth' => $depth];
                $walk($folder->id, $depth + 1);
            }
        };

        $walk(0, 0);

        return $result;
    }

    /**
     * Content-i workspace qovluğuna daşıyır.
     *
     * - $folderId verilərsə: həmin qovluğun workspace-inə yerləşir.
     * - yalnız $workspaceId verilərsə: workspace kökünə yerləşir (folder null).
     * - hər ikisi null olarsa: workspace-dən çıxarılır (kütüphanəyə qayıdır).
     */
    public function moveContent(int $contentId, ?int $workspaceId, ?int $folderId, int $actingUserId): Content
    {
        $content = $this->assertContentOwner($contentId, $actingUserId);

        if ($folderId !== null) {
            $folder = $this->folders->find($folderId);
            if ($folder === null) {
                throw new \RuntimeException('Qovluq tapılmadı.');
            }
            $this->assertFolderOwner($folderId, (int) $folder->workspace_id, $actingUserId);
            $content->update([
                'workspace_id' => $folder->workspace_id,
                'folder_id' => $folder->id,
            ]);

            return $content;
        }

        if ($workspaceId !== null) {
            $this->assertWorkspaceOwner($workspaceId, $actingUserId);
            $content->update(['workspace_id' => $workspaceId, 'folder_id' => null]);

            return $content;
        }

        $content->update(['workspace_id' => null, 'folder_id' => null]);

        return $content;
    }

    /**
     * Quiz/dərs yaratmaq üçün workspace kontekstini doğrulayıb qaytarır.
     * Keçərsiz id verilərsə null qayıdır (kontekst yoxdur).
     */
    public function resolveContextFor(?int $workspaceId, ?int $folderId, int $actingUserId): ?array
    {
        if ($workspaceId === null || $workspaceId === 0) {
            return null;
        }

        $workspace = $this->assertWorkspaceOwner($workspaceId, $actingUserId);

        $folder = null;
        if ($folderId !== null && $folderId !== 0) {
            $folder = $this->assertFolderOwner($folderId, $workspaceId, $actingUserId);
        }

        return [
            'workspace_id' => (int) $workspace->id,
            'folder_id' => $folder?->id ? (int) $folder->id : null,
            'workspace_name' => $workspace->name,
            'folder_name' => $folder?->name,
        ];
    }

    protected function assertWorkspaceOwner(int $workspaceId, int $actingUserId): Workspace
    {
        $workspace = $this->workspaces->find($workspaceId);
        if ($workspace === null) {
            throw new \RuntimeException('Workspace tapılmadı.');
        }
        $user = \App\Domain\User\User::find($actingUserId);
        if (! $user?->isAdmin() && $workspace->teacher_id !== $actingUserId) {
            throw new \RuntimeException('Bu workspace-ə icazəniz yoxdur.');
        }

        return $workspace;
    }

    protected function assertFolderOwner(int $folderId, int $workspaceId, int $actingUserId): WorkspaceFolder
    {
        $folder = $this->folders->find($folderId);
        if ($folder === null || $folder->workspace_id !== $workspaceId) {
            throw new \RuntimeException('Qovluq tapılmadı.');
        }
        $this->assertWorkspaceOwner($workspaceId, $actingUserId);

        return $folder;
    }

    protected function assertContentOwner(int $contentId, int $actingUserId): Content
    {
        $content = $this->contents->find($contentId);
        if ($content === null || $content->teacher_id !== $actingUserId) {
            throw new \RuntimeException('Content tapılmadı.');
        }

        return $content;
    }
}
