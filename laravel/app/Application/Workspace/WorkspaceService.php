<?php

namespace App\Application\Workspace;

use App\Domain\Content\ContentRepository;
use App\Domain\Node\Node;
use App\Domain\Node\NodeRepository;
use App\Domain\Student\StudentRepository;
use App\Domain\User\User;
use App\Domain\Workspace\Workspace;
use App\Domain\Workspace\WorkspaceRepository;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;

/**
 * Workspace əməliyyatları: CRUD + workspace node ağacı (file-manager tərzi).
 */
class WorkspaceService
{
    public function __construct(
        private readonly WorkspaceRepository $workspaces,
        private readonly NodeRepository $nodes,
        private readonly ContentRepository $contents,
        private readonly StudentRepository $students,
    ) {
    }

    /** Teacher-in workspaceləri: [id, name, student_count, created_at]. */
    public function listForTeacher(int $teacherId): array
    {
        return $this->workspaces->listForTeacher($teacherId)
            ->map(fn (Workspace $w) => [
                'id' => $w->id,
                'name' => $w->name,
                'student_count' => (int) $w->students_count,
                'created_at' => $w->created_at?->toDateString(),
            ])
            ->values()
            ->all();
    }

    public function find(int $workspaceId): ?Workspace
    {
        return $this->workspaces->find($workspaceId);
    }

    /** Cədvəl üçün istifadəçinin görə biləcəyi workspacelərlə məhdud sorğu. */
    public function scopeQueryFor(Builder $query, int $actingUserId): Builder
    {
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;

        return $this->workspaces->scopeForUser($query, $actingUserId, $isAdmin);
    }

    /** Admin cədvəli üçün axtarış + səhifələnmiş workspace siyahısı (array). */
    public function paginate(int $actingUserId, ?string $search = null, int $perPage = 15): LengthAwarePaginator
    {
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;

        return $this->workspaces
            ->paginateForUser($actingUserId, $isAdmin, $search, $perPage)
            ->through(fn (Workspace $w): array => [
                'id' => (int) $w->id,
                'name' => $w->name,
                'student_count' => (int) $w->students_count,
                'content_count' => (int) $w->content_count,
                'created_at' => $w->created_at?->toDateString(),
            ]);
    }

    /** Workspace redaktor formu üçün ad + yaradılma tarixi. */
    public function formData(int $workspaceId): array
    {
        $workspace = $this->workspaces->find($workspaceId);
        if ($workspace === null) {
            return [];
        }

        return [
            'name' => $workspace->name,
            'created_at' => $workspace->created_at?->toDateString(),
        ];
    }

    public function create(int $teacherId, string $name): Workspace
    {
        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Workspace adı boş ola bilməz.');
        }

        return $this->workspaces->create($teacherId, $name);
    }

    public function rename(int $teacherId, int $workspaceId, string $name): void
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);
        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Ad boş ola bilməz.');
        }

        $this->workspaces->update($workspace, $name);
    }

    public function delete(int $teacherId, int $workspaceId): void
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);
        $this->workspaces->delete($workspace);
    }

    // --- Tələbələr ---

    public function attachStudents(int $teacherId, int $workspaceId, array $studentIds): void
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);
        $this->workspaces->attachStudents($workspace, $studentIds);
    }

    public function detachStudent(int $teacherId, int $workspaceId, int $studentId): void
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);
        $this->workspaces->detachStudent($workspace, $studentId);
    }

    /** Workspace-də olmayan (əlavə edilə bilən) tələbələr. */
    public function availableStudents(int $teacherId, int $workspaceId, ?string $search = null): array
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);
        $existing = $this->workspaces->studentIds($workspace);

        return $this->students->availableForWorkspace($search)
            ->reject(fn ($student) => in_array($student->id, $existing, true))
            ->map(fn ($student) => [
                'id' => $student->id,
                'name' => trim(($student->first_name ?? '').' '.($student->last_name ?? '')),
                'email' => $student->email,
            ])
            ->values()
            ->all();
    }

    /** Workspace-dəki tələbələr. */
    public function studentList(int $teacherId, int $workspaceId): array
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);

        return $this->workspaces->students($workspace)
            ->map(fn ($student) => [
                'id' => $student->id,
                'name' => trim(($student->first_name ?? '').' '.($student->last_name ?? '')),
                'email' => $student->email,
            ])
            ->values()
            ->all();
    }

    // --- Node ağacı (file-manager) ---

    public function directory(int $teacherId, int $workspaceId, ?int $parentId = null): array
    {
        $this->assertOwned($teacherId, $workspaceId);
        $this->assertWorkspaceParent($workspaceId, $parentId);

        $folders = $this->nodes->workspaceFolders($workspaceId, $parentId);
        $contents = $this->nodes->workspaceContents($workspaceId, $parentId);

        return [
            'breadcrumbs' => $this->nodes->breadcrumbs($parentId),
            'folders' => $folders->map(fn (Node $n) => [
                'node_id' => $n->id,
                'name' => $n->name,
                'position' => $n->position,
                'parent_id' => $n->parent_id,
            ])->values()->all(),
            'contents' => $contents->map(function (Node $n) {
                $content = $n->content;
                if ($content === null) {
                    return null;
                }

                return [
                    'node_id' => $n->id,
                    'content_id' => $content->id,
                    'title' => $content->title,
                    'type' => $content->type,
                    'type_label' => $content->typeLabel(),
                    'is_published' => $content->is_published,
                    'has_video' => $content->lesson?->has_video ?? false,
                    'duration_label' => $content->lesson?->duration_label ?? null,
                    'question_count' => (int) ($content->quiz?->questions_count ?? 0),
                    'url' => $content->url,
                ];
            })->filter()->values()->all(),
        ];
    }

    public function createFolder(int $teacherId, int $workspaceId, string $name, ?int $parentId = null): Node
    {
        $this->assertOwned($teacherId, $workspaceId);
        $this->assertWorkspaceParent($workspaceId, $parentId);
        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Qovluq adı boş ola bilməz.');
        }

        return $this->nodes->createFolder($teacherId, $name, $parentId, $workspaceId);
    }

    /** Kitabxanadan məzmunu workspace-a əlavə edir (yeni node). */
    public function addContent(int $teacherId, int $workspaceId, int $contentId, ?int $parentId = null): Node
    {
        $this->assertOwned($teacherId, $workspaceId);
        $this->assertWorkspaceParent($workspaceId, $parentId);

        $content = $this->contents->find($contentId);
        if ($content === null || $content->teacher_id !== $teacherId) {
            throw new \RuntimeException('Məzmun tapılmadı.');
        }

        return $this->nodes->createContentNode($teacherId, $content->id, $content->title, $parentId, $workspaceId);
    }

    /** Workspace-dən məzmun node-unu silir (məzmun kitabxanada qalır). */
    public function removeContent(int $teacherId, int $workspaceId, int $nodeId): void
    {
        $this->assertOwned($teacherId, $workspaceId);
        $node = $this->nodes->find($nodeId);
        if ($node === null || $node->workspace_id !== $workspaceId) {
            throw new \RuntimeException('Element tapılmadı.');
        }

        $this->nodes->delete($node);
    }

    /** Kitabxana qovluğunu workspace-a kopyalayır (məzmun referansları paylaşılır). */
    public function copyFolder(int $teacherId, int $workspaceId, int $sourceNodeId, ?int $parentId = null): void
    {
        $this->assertOwned($teacherId, $workspaceId);
        $this->assertWorkspaceParent($workspaceId, $parentId);

        $source = $this->nodes->find($sourceNodeId);
        if ($source === null || ! $source->isFolder() || $source->workspace_id !== null || $source->teacher_id !== $teacherId) {
            throw new \RuntimeException('Kaynak qovluq tapılmadı.');
        }

        $this->copyFolderRecursive($source, $workspaceId, $parentId);
    }

    public function renameNode(int $teacherId, int $workspaceId, int $nodeId, string $name): void
    {
        $this->assertOwned($teacherId, $workspaceId);
        $node = $this->nodes->find($nodeId);
        if ($node === null || $node->workspace_id !== $workspaceId) {
            throw new \RuntimeException('Element tapılmadı.');
        }

        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Ad boş ola bilməz.');
        }

        $this->nodes->update($node, ['name' => $name]);
    }

    public function moveNode(int $teacherId, int $workspaceId, int $nodeId, ?int $newParentId): void
    {
        $this->assertOwned($teacherId, $workspaceId);
        $node = $this->nodes->find($nodeId);
        if ($node === null || $node->workspace_id !== $workspaceId) {
            throw new \RuntimeException('Element tapılmadı.');
        }

        if ($newParentId !== null) {
            $parent = $this->nodes->find($newParentId);
            if ($parent === null || $parent->workspace_id !== $workspaceId) {
                throw new \RuntimeException('Hədəf qovluq tapılmadı.');
            }
            if (! $parent->isFolder()) {
                throw new \RuntimeException('Yalnız qovluğa daşına bilər.');
            }
            if ($node->isFolder() && $this->isSelfOrDescendant($parent, $nodeId)) {
                throw new \RuntimeException('Qovluq öz daxilinə daşına bilməz.');
            }
        }

        $this->nodes->update($node, ['parent_id' => $newParentId]);
    }

    /** Workspace node-u və ya qovluğunu silir (məzmunlar kitabxanada qalır). */
    public function deleteNode(int $teacherId, int $workspaceId, int $nodeId): void
    {
        $this->assertOwned($teacherId, $workspaceId);
        $node = $this->nodes->find($nodeId);
        if ($node === null || $node->workspace_id !== $workspaceId) {
            throw new \RuntimeException('Element tapılmadı.');
        }

        $this->nodes->deleteTree($nodeId);
    }

    /** Workspace-dəki bütün qovluqlar (move dropdown üçün). */
    public function folderTree(int $teacherId, int $workspaceId, ?int $excludeNodeId = null): array
    {
        $this->assertOwned($teacherId, $workspaceId);
        $folders = $this->nodes->allWorkspaceFolders($workspaceId);
        $byParent = $folders->groupBy(fn (Node $n) => $n->parent_id ?? 0);

        $result = [];
        $walk = function (int $parentKey, int $depth) use (&$walk, &$result, $byParent, $excludeNodeId): void {
            foreach ($byParent->get($parentKey, collect()) as $folder) {
                if ($folder->id === $excludeNodeId) {
                    continue;
                }
                $result[] = ['id' => $folder->id, 'name' => $folder->name, 'depth' => $depth];
                if ($folder->id !== $excludeNodeId) {
                    $walk($folder->id, $depth + 1);
                }
            }
        };

        $walk(0, 0);

        return $result;
    }

    protected function assertOwned(int $teacherId, int $workspaceId): Workspace
    {
        $workspace = $this->workspaces->find($workspaceId);
        if ($workspace === null || $workspace->teacher_id !== $teacherId) {
            throw new \RuntimeException('Workspace tapılmadı.');
        }

        return $workspace;
    }

    protected function assertWorkspaceParent(int $workspaceId, ?int $parentId): void
    {
        if ($parentId === null) {
            return;
        }

        $parent = $this->nodes->find($parentId);
        if ($parent === null || $parent->workspace_id !== $workspaceId) {
            throw new \RuntimeException('Qovluq tapılmadı.');
        }
        if (! $parent->isFolder()) {
            throw new \RuntimeException('Məzmun altında qovluq aça bilməz.');
        }
    }

    protected function isSelfOrDescendant(Node $node, int $ancestorId): bool
    {
        $current = $node;
        while ($current->parent_id !== null) {
            if ($current->parent_id === $ancestorId) {
                return true;
            }
            $current = $this->nodes->find($current->parent_id);
            if ($current === null) {
                return false;
            }
        }

        return false;
    }

    protected function copyFolderRecursive(Node $source, int $workspaceId, ?int $newParentId): void
    {
        $newFolder = $this->nodes->createFolder($source->teacher_id, $source->name, $newParentId, $workspaceId);

        foreach ($this->nodes->children($source->id) as $child) {
            if ($child->isFolder()) {
                $this->copyFolderRecursive($child, $workspaceId, $newFolder->id);
            } elseif ($child->content_id) {
                $this->nodes->createContentNode($child->teacher_id, $child->content_id, $child->name, $newFolder->id, $workspaceId);
            }
        }
    }
}
