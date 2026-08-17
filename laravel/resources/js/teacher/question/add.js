/**
 * ADD — Sual bankına qovluq / sual əlavə et.
 *
 *   POST /teacher/questions/folders          (name, parent_id)
 *   POST /teacher/questions                  (text, options, correct_option,
 *                                             explanation, folder_id)
 *
 * Hər iki sorğu web controller-i (QuestionController) işlədir — frontend
 * /api/v1-ə birbaşa toxunmur. Qovluq formu $refs ilə DOM-dan oxunur (fields),
 * sual formu isə reaktiv qForm obyektidir — həm saxla üçün payload, həm də
 * canlı önizləmə mənbəyi. Sual mətni rich text HTML ola bilər
 * (contenteditable editörü ilə yazılır).
 */
export default function createBankAdder({ parentId }) {
    return {
        /**
         * openFolder(fields) — Yeni qovluq dialoqunda formu təmizləyir.
         */
        openFolder(fields) {
            fields.folderName.value = '';
        },

        /**
         * buildFolderPayload(fields) — Qovluq formunu API formatına yığır.
         */
        buildFolderPayload(fields) {
            return {
                name: fields.folderName.value.trim(),
                parent_id: parentId,
            };
        },

        /**
         * addFolder(fields) — Cari qovluğun altına yeni qovluq yaradır.
         */
        async addFolder(fields) {
            const payload = this.buildFolderPayload(fields);
            if (!payload.name) {
                window.alert('Qovluq adı boş ola bilməz.');
                return false;
            }
            try {
                await KelaApi('POST', '/teacher/questions/folders', payload);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },

        /**
         * openQuestion(form) — Sual dialoqunda formu təmizləyir (default dəyərlər).
         */
        openQuestion(form) {
            Object.assign(form, {
                text: '',
                option_a: '',
                option_b: '',
                option_c: '',
                option_d: '',
                option_e: '',
                correct_option: 0,
                explanation: '',
            });
        },

        /**
         * buildQuestionPayload(form) — Sual formunu API formatına yığır.
         * Boş seçimlər null göndərilir; doğru cavab seçimə işarə edir.
         * text rich text HTML ola bilər — boşluq yoxlaması düz mətnə görədir.
         */
        buildQuestionPayload(form) {
            const correct = Number(form.correct_option) || 0;

            return {
                text: (form.text || '').trim(),
                option_a: form.option_a.trim(),
                option_b: form.option_b.trim(),
                option_c: form.option_c.trim() || null,
                option_d: form.option_d.trim() || null,
                option_e: form.option_e.trim() || null,
                correct_option: correct,
                explanation: (form.explanation || '').trim() || null,
                folder_id: parentId ?? null,
            };
        },

        /**
         * addQuestion(form) — Yeni sualı API-ə göndərir.
         * Doğru cavab işarələnmiş seçimin metni boşdursa xəbərdarlıq edir.
         */
        async addQuestion(form) {
            const payload = this.buildQuestionPayload(form);
            const plainText = payload.text.replace(/<[^>]*>/g, '');
            if (!plainText.trim() || !payload.option_a || !payload.option_b) {
                window.alert('Sual mətni və ən azı A, B seçimləri tələb olunur.');
                return false;
            }
            const correctText = [payload.option_a, payload.option_b, payload.option_c, payload.option_d, payload.option_e][payload.correct_option];
            if (!correctText) {
                window.alert('Doğru cavab kimi işarələnmiş seçim boşdur — əvvəlcə onu doldurun.');
                return false;
            }
            try {
                await KelaApi('POST', '/teacher/questions', payload);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },
    };
}
