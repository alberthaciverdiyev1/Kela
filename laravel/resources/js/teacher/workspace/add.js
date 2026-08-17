/**
 * ADD — Workspace-ə tələbə əlavə et.
 *
 *   POST /teacher/workspaces/{id}/students  (student_ids[])
 */
export default function createWorkspaceAdder({ api }) {
    return {
        /**
         * addStudents(studentIds, agreedPrice, startDate) — Seçilmiş tələbələri workspace-ə əlavə edir.
         */
        async addStudents(studentIds, agreedPrice = null, startDate = null) {
            if (!studentIds.length) {
                window.alert('Tələbə seçin.');
                return false;
            }
            
            const payload = { student_ids: studentIds };
            if (agreedPrice) payload.agreed_price = agreedPrice;
            if (startDate) payload.start_date = startDate;
            
            try {
                await KelaApi('POST', `${api}/students`, payload);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },
    };
}
