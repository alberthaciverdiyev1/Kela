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
         * openQuestionEdit(form, question) — Sual düzləndirmə formunu doldurur.
         */
        openQuestionEdit(form, question) {
            Object.assign(form, {
                text: question.text || '',
                option_a: question.option_a || '',
                option_b: question.option_b || '',
                option_c: question.option_c || '',
                option_d: question.option_d || '',
                option_e: question.option_e || '',
                correct_option: question.correct_option ?? 0,
                explanation: question.explanation || '',
            });
        },

        /**
         * buildQuestionPayload(form) — Sual formunu API formatına yığır.
         * folder_id göndərilmir — sualın yeri dəyişməz, yalnız məzmun yenilənir.
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
            };
        },

        /**
         * updateQuestion(form, id) — Mövcud sualın məzmununu yeniləyir.
         */
        async updateQuestion(form, id) {
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
