<?php

namespace App\Domain\Node;

use Illuminate\Support\Collection;

/**
 * Node (qovluq/məzmun ağacı) məlumat girişi üçün kontrakt.
 * Implementasiya Infrastructure katmanında (Eloquent) yerləşir.
 */
interface NodeRepository
{
    /** Workspace-dəki qovluqlar, opsional ana qovluq altında. */
    public function workspaceFolders(int $workspaceId, ?int $parentId = null): Collection;

    /** Workspace-dəki məzmun node-ları, opsional ana qovluq altında. */
    public function workspaceContents(int $workspaceId, ?int $parentId = null): Collection;

    /** Move dropdown üçün workspace-dəki bütün qovluqlar. */
    public function allWorkspaceFolders(int $workspaceId): Collection;

    /** parentId-dən yuxarıya kökə qədər breadcrumb zənciri. */
    public function breadcrumbs(?int $parentId): array;

    /** Qovluq node-u yaradır. */
    public function createFolder(int $teacherId, string $name, ?int $parentId, ?int $workspaceId = null): Node;

    /** Mövcud Content-i göstərən məzmun node-u yaradır. */
    public function createContentNode(int $teacherId, int $contentId, string $name, ?int $parentId, ?int $workspaceId = null): Node;

    public function find(int $id): ?Node;

    public function update(Node $node, array $attributes): Node;

    public function delete(Node $node): bool;

    /** Node və bütün alt node-ları silir. */
    public function deleteTree(int $nodeId): void;

    /** Node-un birbaşa uşaqları (qovluq + məzmun). */
    public function children(int $nodeId): Collection;
}
