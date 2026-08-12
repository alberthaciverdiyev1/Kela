/**
 * Controller — Quizlər səhifəsinin (qovluqlu kataloq) giriş nöqtəsi.
 *
 * Quiz qovluqları backend /api/v1/quiz-folders üzərindəndir; bu controller
 * dialoqları idarə edir, KelaApi ilə sorğuları aparır və bitdikdən sonra
 * səhifəni təzələyir (quiz siyahısı paginated server-rendered-dir).
 *
 * Əməliyyatlar:
 *   POST   /api/v1/quiz-folders             → yeni qovluq (name, parent_id)
 *   POST   /api/v1/quiz-folders/{id}/rename → adı dəyiş
 *   POST   /api/v1/quiz-folders/{id}/move   → qovluğu daşı
 *   DELETE /api/v1/quiz-folders/{id}        → qovluğu sil (quizlər kökə)
 *   POST   /api/v1/quiz-folders/move-quiz   → quizi qovluğa daşı
 */
import Alpine from 'alpinejs';

export default function quizFolders(config) {
    return {
        // ── Konfiqurasiya ──────────────────────────────────────────────────
        folderId: config.folderId ?? null,
        folderTree: config.folderTree || [],

        // ── UI vəziyyəti ───────────────────────────────────────────────────
        showFolderAdd: false,
        showFolderRename: false,
        showFolderMove: false,
        showQuizMove: false,

        editingFolderId: null,
        moveFolderId: null,
        moveQuizId: null,

        // ── Qovluq əməliyyatları ───────────────────────────────────────────

        openFolderAdd() {
            this.showFolderAdd = true;
            this.$nextTick(() => {
                if (this.$refs.folderNameInput) this.$refs.folderNameInput.focus();
            });
        },

        async saveFolder() {
            const name = this.$refs.folderNameInput?.value.trim();
            if (!name) {
                window.alert('Qovluq adı boş ola bilməz.');
                return;
            }
            try {
                await KelaApi('POST', '/api/v1/quiz-folders', {
                    name,
                    parent_id: this.folderId,
                });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        openFolderRename(btn) {
            this.editingFolderId = Number(btn.dataset.folderId);
            this.showFolderRename = true;
            this.$nextTick(() => {
                if (this.$refs.folderRenameInput) {
                    this.$refs.folderRenameInput.value = btn.dataset.folderName || '';
                    this.$refs.folderRenameInput.focus();
                }
            });
        },

        async saveFolderRename() {
            const name = this.$refs.folderRenameInput?.value.trim();
            if (!name) {
                window.alert('Qovluq adı boş ola bilməz.');
                return;
            }
            try {
                await KelaApi('POST', `/api/v1/quiz-folders/${this.editingFolderId}/rename`, { name });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        openFolderMove(btn) {
            this.moveFolderId = Number(btn.dataset.folderId);
            this.showFolderMove = true;
        },

        async saveFolderMove() {
            const selected = this.$refs.folderMoveSelect?.value;
            try {
                await KelaApi('POST', `/api/v1/quiz-folders/${this.moveFolderId}/move`, {
                    parent_id: selected ? Number(selected) : null,
                });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        async handleFolderDelete(btn) {
            const id = btn.dataset.folderId;
            const name = btn.dataset.folderName || 'Qovluq';
            if (!window.confirm(`'${name}' qovluğu silinsin? (İçindəki quizlər kökə daşınacaq.)`)) return;
            try {
                await KelaApi('DELETE', `/api/v1/quiz-folders/${id}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Quiz əməliyyatları ─────────────────────────────────────────────

        openQuizMove(btn) {
            this.moveQuizId = Number(btn.dataset.quizId);
            this.showQuizMove = true;
        },

        async saveQuizMove() {
            const selected = this.$refs.quizMoveSelect?.value;
            try {
                await KelaApi('POST', '/api/v1/quiz-folders/move-quiz', {
                    content_id: this.moveQuizId,
                    folder_id: selected ? Number(selected) : null,
                });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Delegasiya ─────────────────────────────────────────────────────

        /**
         * onTableClick(e) — Kataloq üzərində click hadisəsi.
         * data-folder-action / data-quiz-action düymələrini müvafiq əməliyyata ötürür.
         */
        onTableClick(e) {
            const fBtn = e.target.closest('[data-folder-action]');
            if (fBtn) {
                const action = fBtn.dataset.folderAction;
                if (action === 'rename') this.openFolderRename(fBtn);
                else if (action === 'move') this.openFolderMove(fBtn);
                else if (action === 'delete') this.handleFolderDelete(fBtn);
                return;
            }
            const qBtn = e.target.closest('[data-quiz-action]');
            if (qBtn) {
                const action = qBtn.dataset.quizAction;
                if (action === 'move') this.openQuizMove(qBtn);
            }
        },

        closeAll() {
            this.showFolderAdd = false;
            this.showFolderRename = false;
            this.showFolderMove = false;
            this.showQuizMove = false;
        },
    };
}

// Alpine-də qeydiyyat: blade-də x-data="quizFolders(...)" işləyə bilsin.
Alpine.data('quizFolders', quizFolders);
// Alpine-i işə salır. Bu entry yalnız quizlər səhifəsində yüklənir.
Alpine.start();
