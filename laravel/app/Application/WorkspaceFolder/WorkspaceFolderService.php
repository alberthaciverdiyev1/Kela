<?php

namespace App\Application\WorkspaceFolder;

use App\Domain\Content\Content;
use App\Domain\Content\ContentRepository;
use App\Domain\Lesson\Lesson;
use App\Domain\Lesson\LessonRepository;
use App\Domain\LessonFolder\LessonFolderRepository;
use App\Domain\Quiz\Quiz;
use App\Domain\Quiz\QuizRepository;
use App\Domain\QuizFolder\QuizFolderRepository;
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
        private readonly QuizRepository $quizzes,
        private readonly QuizFolderRepository $quizFolders,
        private readonly LessonRepository $lessons,
        private readonly LessonFolderRepository $lessonFolders,
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

    /** Teacher-in heç bir workspace-ə bağlanmamış quiz/dərs məzmunları (əlavə et dialoqu üçün). */
    public function availableContents(int $actingUserId): array
    {
        return $this->contents
            ->availableForWorkspace($actingUserId, [Content::TYPE_QUIZ, Content::TYPE_LESSON])
            ->map(function (Content $c): array {
                $chain = $this->bankFolderChainFor($c);

                return [
                    'content_id' => (int) $c->id,
                    'title' => $c->title,
                    'type' => (int) $c->type,
                    'type_label' => $c->typeLabel(),
                    'is_published' => (bool) $c->is_published,
                    'folder_id' => $c->folder_id ? (int) $c->folder_id : null,
                    'folder_path' => $chain['names'],
                    'folder_path_ids' => $chain['ids'],
                ];
            })
            ->values()
            ->all();
    }

    /**
     * Content-in bank qovluğunun kökdən yarpağa zənciri.
     * names: qovluq adları; ids: müvafiq bank qovluq id-ləri (boş = kökdədir).
     * Bank qovluğu contents.folder_id DEYİL — quizzes/lessons.folder_id-dir.
     */
    protected function bankFolderChainFor(Content $content): array
    {
        $folderId = $content->isQuiz()
            ? $this->quizzes->find((int) $content->id)?->folder_id
            : $this->lessons->find((int) $content->id)?->folder_id;

        if ($folderId === null) {
            return ['names' => [], 'ids' => []];
        }

        $folderRepo = $content->isQuiz() ? $this->quizFolders : $this->lessonFolders;
        $crumbs = $folderRepo->breadcrumbs((int) $folderId);

        return [
            'names' => array_map(fn (array $f): string => $f['name'], $crumbs),
            'ids' => array_map(fn (array $f): int => (int) $f['id'], $crumbs),
        ];
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

        // Alt ağacdakı bütün qovluq id-ləri (özləri də daxil).
        $ids = $this->folders->descendantIds($folderId);

        // İçindəki məzmunları quiz/lesson qeydləri ilə birlikdə sil.
        $this->contents->deleteContentsInFolders($ids);

        // Qovluq ağacını sil.
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
     * Bank qovluğunu bütün alt qovluqları və içindəki məzmunları ilə workspace-ə əlavə edir.
     *
     * - Qovluq strukturu workspace-də əks olunur (eyni adlı qovluq varsa yenidən yaradılmır).
     * - Yalnız heç bir workspace-ə bağlanmamış (kütüphanədəki) məzmunlar daşınır.
     * - Əlavə edilmiş qovluq sayı və daşınan məzmun sayını qaytarır.
     */
    public function addFolderToWorkspace(
        string $bankType,       // 'quiz' | 'lesson'
        int $bankFolderId,
        int $workspaceId,
        ?int $targetFolderId,   // workspace qovluğu (null = workspace kökü)
        int $actingUserId,
    ): array {
        $this->assertWorkspaceOwner($workspaceId, $actingUserId);
        if ($targetFolderId !== null) {
            $this->assertFolderOwner($targetFolderId, $workspaceId, $actingUserId);
        }

        $bankRepo = $bankType === 'quiz' ? $this->quizFolders : $this->lessonFolders;
        $bankFolder = $bankRepo->find($bankFolderId);
        if ($bankFolder === null || $bankFolder->teacher_id !== $actingUserId) {
            throw new \RuntimeException('Qovluq tapılmadı.');
        }

        $bankFolders = $bankRepo->allFoldersFor($actingUserId)->keyBy('id');
        $subtreeIds = $bankRepo->descendantIds($bankFolderId);

        // Bank qovluq id → workspace qovluq id xəritəsi.
        $mapping = [];
        $walk = function (int $fid, ?int $wsParentId) use (&$walk, &$mapping, $bankFolders, $subtreeIds, $workspaceId): void {
            $folder = $bankFolders->get($fid);
            if ($folder === null) {
                return;
            }
            $wsFolder = $this->findOrCreateWorkspaceFolder($workspaceId, $wsParentId, $folder->name);
            $mapping[$fid] = (int) $wsFolder->id;

            foreach ($bankFolders as $candidate) {
                if ((int) $candidate->parent_id === $fid && in_array((int) $candidate->id, $subtreeIds, true)) {
                    $walk((int) $candidate->id, (int) $wsFolder->id);
                }
            }
        };
        $walk($bankFolderId, $targetFolderId);

        // Məzmunları müvafiq workspace qovluğuna daşı (yalnız kütüphanədəkilər).
        $moved = 0;
        foreach ($mapping as $fid => $wsFolderId) {
            foreach ($this->contentsInBankFolder($fid, $bankType, $actingUserId) as $content) {
                $content->update([
                    'workspace_id' => $workspaceId,
                    'folder_id' => $wsFolderId,
                ]);
                $moved++;
            }
        }

        return [
            'folders' => count($mapping),
            'contents' => $moved,
        ];
    }

    /** Bank qovluğundakı hələ workspace-ə bağlanmamış content-lər. */
    protected function contentsInBankFolder(int $bankFolderId, string $bankType, int $actingUserId): \Illuminate\Support\Collection
    {
        $ids = ($bankType === 'quiz' ? Quiz::query() : Lesson::query())
            ->where('folder_id', $bankFolderId)
            ->pluck('content_id');

        return Content::query()
            ->whereIn('id', $ids)
            ->where('teacher_id', $actingUserId)
            ->whereNull('workspace_id')
            ->get();
    }

    /** Verilmiş ana qovluq altında eyni adlı qovluq varsa onu, yoxsa yenisini qaytarır. */
    protected function findOrCreateWorkspaceFolder(int $workspaceId, ?int $parentId, string $name): WorkspaceFolder
    {
        $existing = $this->folders->foldersFor($workspaceId, $parentId)
            ->first(fn (WorkspaceFolder $f): bool => $f->name === $name);

        if ($existing !== null) {
            return $existing;
        }

        return $this->folders->create($workspaceId, $name, $parentId);
    }

    /** İçeriği workspace-dən kütüphanəyə geri göndərir (workspace daxilindəki qovluq da təmizlənir). */
    public function removeContentFromWorkspace(int $contentId, int $actingUserId): void
    {
        $content = $this->assertContentOwner($contentId, $actingUserId);
        if ($content->workspace_id === null) {
            throw new \RuntimeException('Content workspace-də deyil.');
        }
        $content->update(['workspace_id' => null, 'folder_id' => null]);
    }

    /**
     * Qovluğu bütün alt qovluqları və içindəki məzmunlarla birlikdə workspace-dən
     * kütüphanəyə geri göndərir. Qovluq ağacı silinir, məzmunlar kütüphanəyə düşür.
     */
    public function removeFolderFromWorkspace(int $folderId, int $actingUserId): void
    {
        $folder = $this->folders->find($folderId);
        if ($folder === null) {
            throw new \RuntimeException('Qovluq tapılmadı.');
        }
        $this->assertWorkspaceOwner((int) $folder->workspace_id, $actingUserId);

        // Alt ağacdakı bütün qovluq id-ləri (özləri də daxil).
        $ids = $this->folders->descendantIds($folderId);

        // Bütün məzmunları kütüphanəyə geri göndər.
        foreach ($this->contents->contentsInFolders($ids) as $content) {
            $content->update(['workspace_id' => null, 'folder_id' => null]);
        }

        // Qovluq ağacını sil.
        $this->folders->deleteTree($folderId);
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
