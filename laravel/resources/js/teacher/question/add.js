/**
 * ADD — Sual bankına qovluq / sual əlavə et.
 *
 *   POST /api/v1/question-folders            (name, parent_id)
 *   POST /api/v1/questions                   (text, options, correct_option, folder_id)
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
                await KelaApi('POST', '/api/v1/question-folders', payload);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },

        /**
         * openQuestion(fields, folderId) — Sual dialoqunda formu təmizləyir.
         */
        openQuestion(fields) {
            fields.qText.value = '';
            fields.qOptionA.value = '';
            fields.qOptionB.value = '';
            fields.qOptionC.value = '';
            fields.qOptionD.value = '';
            fields.qOptionE.value = '';
            fields.qCorrectOption.value = '0';
        },

        /**
         * buildQuestionPayload(fields, folderId) — Sual formunu API formatına yığır.
         */
        buildQuestionPayload(fields, folderId) {
            return {
                text: fields.qText.value.trim(),
                option_a: fields.qOptionA.value.trim(),
                option_b: fields.qOptionB.value.trim(),
                option_c: fields.qOptionC.value.trim() || null,
                option_d: fields.qOptionD.value.trim() || null,
                option_e: fields.qOptionE.value.trim() || null,
                correct_option: Number(fields.qCorrectOption.value),
                folder_id: folderId ?? null,
            };
        },

        /**
         * addQuestion(fields, folderId) — Yeni sualı API-ə göndərir.
         */
        async addQuestion(fields, folderId) {
            const payload = this.buildQuestionPayload(fields, folderId);
            if (!payload.text || !payload.option_a || !payload.option_b) {
                window.alert('Sual mətni və ən azı A, B seçimləri tələb olunur.');
                return false;
            }
            try {
                await KelaApi('POST', '/api/v1/questions', payload);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },
    };
}
