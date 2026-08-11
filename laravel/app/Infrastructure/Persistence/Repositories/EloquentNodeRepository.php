<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Node\Node;
use App\Domain\Node\NodeRepository;
use Illuminate\Support\Collection;

class EloquentNodeRepository implements NodeRepository
{
    public function workspaceFolders(int $workspaceId, ?int $parentId = null): Collection
    {
        return Node::query()
            ->where('workspace_id', $workspaceId)
            ->where('kind', Node::KIND_FOLDER)
            ->where('parent_id', $parentId)
            ->orderBy('position')
            ->orderBy('name')
            ->get();
    }

    public function workspaceContents(int $workspaceId, ?int $parentId = null): Collection
    {
        return Node::query()
            ->where('workspace_id', $workspaceId)
            ->where('kind', Node::KIND_CONTENT)
            ->where('parent_id', $parentId)
            ->with([
                'content',
                'content.lesson',
                'content.quiz' => fn ($q) => $q->withCount('questions'),
            ])
            ->orderBy('position')
            ->orderBy('name')
            ->get();
    }

    public function allWorkspaceFolders(int $workspaceId): Collection
    {
        return Node::query()
            ->where('workspace_id', $workspaceId)
            ->where('kind', Node::KIND_FOLDER)
            ->orderBy('name')
            ->get();
    }

    public function breadcrumbs(?int $parentId): array
    {
        $crumbs = [];
        $id = $parentId;

        while ($id !== null) {
            $node = Node::withTrashed()->find($id);
            if (! $node) {
                break;
            }
            array_unshift($crumbs, ['id' => $node->id, 'name' => $node->name]);
            $id = $node->parent_id;
        }

        return $crumbs;
    }

    public function createFolder(int $teacherId, string $name, ?int $parentId, ?int $workspaceId = null): Node
    {
        $position = $this->nextPosition($teacherId, $parentId, $workspaceId, Node::KIND_FOLDER);

        return Node::create([
            'teacher_id' => $teacherId,
            'workspace_id' => $workspaceId,
            'parent_id' => $parentId,
            'name' => $name,
            'kind' => Node::KIND_FOLDER,
            'position' => $position,
            'content_id' => null,
        ]);
    }

    public function createContentNode(int $teacherId, int $contentId, string $name, ?int $parentId, ?int $workspaceId = null): Node
    {
        $position = $this->nextPosition($teacherId, $parentId, $workspaceId, Node::KIND_CONTENT);

        return Node::create([
            'teacher_id' => $teacherId,
            'workspace_id' => $workspaceId,
            'parent_id' => $parentId,
            'name' => $name,
            'kind' => Node::KIND_CONTENT,
            'position' => $position,
            'content_id' => $contentId,
        ]);
    }

    public function find(int $id): ?Node
    {
        return Node::with('content')->find($id);
    }

    public function update(Node $node, array $attributes): Node
    {
        $node->update($attributes);

        return $node;
    }

    public function delete(Node $node): bool
    {
        return (bool) $node->delete();
    }

    public function deleteTree(int $nodeId): void
    {
        $node = Node::with('children')->find($nodeId);
        if (! $node) {
            return;
        }

        foreach ($node->children as $child) {
            $this->deleteTree($child->id);
        }

        $node->delete();
    }

    public function children(int $nodeId): Collection
    {
        return Node::with('content')->where('parent_id', $nodeId)->get();
    }

    protected function nextPosition(int $teacherId, ?int $parentId, ?int $workspaceId, int $kind): int
    {
        $q = Node::query()
            ->where('kind', $kind)
            ->where('parent_id', $parentId);

        if ($workspaceId !== null) {
            $q->where('workspace_id', $workspaceId);
        } else {
            $q->whereNull('workspace_id')->where('teacher_id', $teacherId);
        }

        return (int) $q->max('position') + 1;
    }
}
