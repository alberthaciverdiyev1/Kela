/**
 * EDIT — Sual bankı düzləndirmə: qovluq adı/silah, sual düzləndir/daşı.
 *
 *   POST /api/v1/question-folders/{id}/rename    (name)
 *   POST /api/v1/question-folders/{id}/move      (parent_id)
 *   PUT  /api/v1/questions/{id}                  (text, options, correct_option)
 *   POST /api/v1/question-folders/move-question  (question_id, folder_id)
 */
export default function createBankEditor() {
    return {
        /**
         * openFolderRename(fields, folder) — Ad dəyiş dialoqunda inputu doldurur.
         */
        openFolderRename(fields, folder) {
            fields.folderRename.value = folder.name || '';
        },

        /**
         * renameFolder(fields, id) — Qovluq adını dəyişir.
         */
        async renameFolder(fields, id) {
            const name = fields.folderRename.value.trim();
            if (!name) {
                window.alert('Qovluq adı boş ola bilməz.');
                return false;
            }
            try {
                await KelaApi('POST', `/api/v1/question-folders/${id}/rename`, { name });
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },

        /**
         * moveFolder(id, newParentId) — Qovluğu başqa qovluğa daşıyır (null → kök).
         */
        async moveFolder(id, newParentId) {
            try {
                await KelaApi('POST', `/api/v1/question-folders/${id}/move`, { parent_id: newParentId ?? null });
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },

        /**
         * openQuestionEdit(fields, question) — Sual düzləndirmə formunu doldurur.
         */
        openQuestionEdit(fields, question) {
            fields.qText.value = question.text || '';
            fields.qOptionA.value = question.option_a || '';
            fields.qOptionB.value = question.option_b || '';
            fields.qOptionC.value = question.option_c || '';
            fields.qOptionD.value = question.option_d || '';
            fields.qOptionE.value = question.option_e || '';
            fields.qCorrectOption.value = String(question.correct_option ?? 0);
        },

        /**
         * buildQuestionPayload(fields) — Sual formunu API formatına yığır.
         * folder_id göndərilmir — sualın yeri dəyişməz, yalnız məzmun yenilənir.
         */
        buildQuestionPayload(fields) {
            return {
                text: fields.qText.value.trim(),
                option_a: fields.qOptionA.value.trim(),
                option_b: fields.qOptionB.value.trim(),
                option_c: fields.qOptionC.value.trim() || null,
                option_d: fields.qOptionD.value.trim() || null,
                option_e: fields.qOptionE.value.trim() || null,
                correct_option: Number(fields.qCorrectOption.value),
            };
        },

        /**
         * updateQuestion(fields, id) — Mövcud sualın məzmununu yeniləyir.
         */
        async updateQuestion(fields, id) {
            const payload = this.buildQuestionPayload(fields);
            if (!payload.text || !payload.option_a || !payload.option_b) {
                window.alert('Sual mətni və ən azı A, B seçimləri tələb olunur.');
                return false;
            }
            try {
                await KelaApi('PUT', `/api/v1/questions/${id}`, payload);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },

        /**
         * moveQuestion(id, folderId) — Sualı qovluğa daşıyır (null → kök).
         */
        async moveQuestion(id, folderId) {
            try {
                await KelaApi('POST', '/api/v1/question-folders/move-question', {
                    question_id: Number(id),
                    folder_id: folderId ?? null,
                });
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },
    };
}
