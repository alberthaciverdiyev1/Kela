/**
 * EDIT — Workspace node-u düzləndir: ad dəyiş / daşı.
 *
 *   POST /api/v1/workspaces/{id}/nodes/{nodeId}/rename  (name)
 *   POST /api/v1/workspaces/{id}/nodes/{nodeId}/move    (parent_id)
 */
export default function createWorkspaceEditor({ api }) {
    async function renameNode(id, name) {
        if (!name) return false;
        try {
            await KelaApi('POST', `${api}/nodes/${id}/rename`, { name });
            return true;
        } catch (err) {
            window.alert(err.message);
            return false;
        }
    }

    async function moveNode(id, parentId) {
        try {
            await KelaApi('POST', `${api}/nodes/${id}/move`, { parent_id: parentId ?? null });
            return true;
        } catch (err) {
            window.alert(err.message);
            return false;
        }
    }

    return { renameNode, moveNode };
}
