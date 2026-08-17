/**
 * DELETE — Workspace-dən tələbə çıxar.
 *
 *   DELETE /teacher/workspaces/{id}/students/{studentId}
 */
export default function createWorkspaceRemover({ api }) {
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

    return { detachStudent };
}
