/**
 * ADD — Workspace-ə tələbə əlavə et.
 *
 *   POST /teacher/workspaces/{id}/students  (student_ids[])
 */
export default function createWorkspaceAdder({ api }) {
    return {
        /**
         * addStudents(studentIds) — Seçilmiş tələbələri workspace-ə əlavə edir.
         */
        async addStudents(studentIds) {
            if (!studentIds.length) {
                window.alert('Tələbə seçin.');
                return false;
            }
            try {
                await KelaApi('POST', `${api}/students`, { student_ids: studentIds });
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },
    };
}
