/**
 * ADD — Workspace-ə əlavə et: qovluq / məzmun / tələbə.
 *
 * Niyə ayrı fayl: "add" ilə bağlı bütün API kodları (qovluq yarat, məzmun
 * bağla, tələbə əlavə et) burada saxlanılır ki, controller.js-də add
 * funksiya kodu olmasın.
 *
 *   POST /api/v1/workspaces/{id}/folders   (name, parent_id)
 *   POST /api/v1/workspaces/{id}/contents  (content_id, parent_id)
 *   POST /api/v1/workspaces/{id}/students  (student_ids[])
 *
 * parentId statik configdir (cari qovluğun yolundan gəlir) — getterə ehtiyac
 * yoxdur, modul konstruktorda birbaşa alır.
 */
export default function createWorkspaceAdder({ api, parentId }) {
    return {
        /**
         * addFolder(name) — Cari qovluğun altına yeni qovluq yaradır.
         *
         * Niyə validasiya burada: qovluq adı boş ola bilməz. Uğur → true,
         * uğursuz → false (xəta mesajı alert-də göstərilir).
         */
        async addFolder(name) {
            if (!name) {
                window.alert('Qovluq adı boş ola bilməz.');
                return false;
            }
            try {
                await KelaApi('POST', `${api}/folders`, { name, parent_id: parentId });
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },

        /**
         * addContent(contentId) — Kitabxanadan məzmunu cari qovluğa bağlayır.
         *
         * Niyə contentId parametr olaraq gəlir: seçim dəyəri controller
         * $refs-dən (DOM-dakı select) oxuyub bura ötürür — modul DOM-a
         * toxunmur.
         */
        async addContent(contentId) {
            if (!contentId) {
                window.alert('Məzmun seçin.');
                return false;
            }
            try {
                await KelaApi('POST', `${api}/contents`, {
                    content_id: Number(contentId),
                    parent_id: parentId,
                });
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },

        /**
         * addStudents(studentIds) — Seçilmiş tələbələri workspace-ə əlavə edir.
         *
         * Niyə studentIds parametr olaraq gəlir: çoxlu seçim (multi-select)
         * dəyərlərini controller DOM-dan toplayıb bura ötürür — modul yalnız
         * API əlaqəsi ilə məşğuldur.
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
