/**
 * DELETE — Sual bankından sil: qovluq və ya sual.
 *
 *   DELETE /teacher/questions/folders/{id}   (qovluq + sualları kökə qaytarır)
 *   DELETE /teacher/questions/{id}           (sualı silir)
 */
export default function createBankRemover() {
    return {
        /**
         * deleteFolder(id, name) — Qovluğu silir (sualları kökə qaytarır).
         */
        async deleteFolder(id, name = 'Qovluq') {
            if (!window.confirm(`'${name}' qovluğu silinsin? (İçindəki suallar kökə daşınacaq.)`)) return false;
            try {
                await KelaApi('DELETE', `/teacher/questions/folders/${id}`);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },

        /**
         * deleteQuestion(id, text) — Sualı silir.
         */
        async deleteQuestion(id, text = 'Sual') {
            if (!window.confirm(`'${text}' silinsin?`)) return false;
            try {
                await KelaApi('DELETE', `/teacher/questions/${id}`);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },
    };
}
