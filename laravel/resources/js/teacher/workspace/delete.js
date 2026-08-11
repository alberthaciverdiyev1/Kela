/**
 * DELETE — Workspace-dən sil: node (qovluq/məzmun) və ya tələbə çıxar.
 *
 *   DELETE /api/v1/workspaces/{id}/nodes/{nodeId}      (ağac silmə)
 *   DELETE /api/v1/workspaces/{id}/students/{studentId}
 */
export default function createWorkspaceRemover({ api }) {
    async function deleteNode(id, name = 'Element') {
        if (!window.confirm(`'${name}' silinsin?`)) return false;
        try {
            await KelaApi('DELETE', `${api}/nodes/${id}`);
            return true;
        } catch (err) {
            window.alert(err.message);
            return false;
        }
    }

    async function detachStudent(id, name) {
        if (!window.confirm(`'${name}' workspace-dən çıxarılsın?`)) return false;
        try {
            await KelaApi('DELETE', `${api}/students/${id}`);
            return true;
        } catch (err) {
            window.alert(err.message);
            return false;
        }
    }

    return { deleteNode, detachStudent };
}
