/**
 * Controller — Quizlər səhifəsinin (qovluqlu kataloq) giriş nöqtəsi.
 *
 * Quiz qovluqları web controller-i (QuizController) işləyir; bu controller
 * dialoqları idarə edir, KelaApi ilə sorğuları aparır və bitdikdən sonra
 * səhifəni təzələyir (quiz siyahısı paginated server-rendered-dir).
 *
 * Əməliyyatlar (hamısı web controller — /api/v1 yoxdur):
 *   POST   /teacher/quizzes/folders             → yeni qovluq (name, parent_id)
 *   POST   /teacher/quizzes/folders/{id}/rename → adı dəyiş
 *   POST   /teacher/quizzes/folders/{id}/move   → qovluğu daşı
 *   DELETE /teacher/quizzes/folders/{id}        → qovluğu sil (quizlər kökə)
 *   POST   /teacher/quizzes/folders/move-quiz   → quizi qovluğa daşı
 */
import Alpine from 'alpinejs';
import createContextMenu from '../context-menu';

export default function quizFolders(config) {
    return {
        // ── Ortaq sağ-tık kontekst menyusu ────────────────────────────────
        ...createContextMenu(),

        // ── Konfiqurasiya ──────────────────────────────────────────────────
        folderId: config.folderId ?? null,
        folderTree: config.folderTree || [],

        // ── UI vəziyyəti ───────────────────────────────────────────────────
        showFolderAdd: false,
        showFolderRename: false,
        showFolderMove: false,
        showQuizMove: false,
        // Kataloq görünümü: 'list' | 'grid' (localStorage-da saxlanılır).
        viewMode: localStorage.getItem('workspace-view') || 'list',

        editingFolderId: null,
        moveFolderId: null,
        moveQuizId: null,

        // ── Kataloq görünümü ───────────────────────────────────────────────

        setViewMode(mode) {
            this.viewMode = mode;
            try {
                localStorage.setItem('workspace-view', mode);
            } catch (e) { /* localStorage əlçatan deyilsə sadəcə seans üçün qalır */ }
        },

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
                await KelaApi('POST', '/teacher/quizzes/folders', {
                    name,
                    parent_id: this.folderId,
                });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        openFolderRename(folderId, folderName = '') {
            this.editingFolderId = Number(folderId);
            this.showFolderRename = true;
            this.$nextTick(() => {
                if (this.$refs.folderRenameInput) {
                    this.$refs.folderRenameInput.value = folderName || '';
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
                await KelaApi('POST', `/teacher/quizzes/folders/${this.editingFolderId}/rename`, { name });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        openFolderMove(folderId) {
            this.moveFolderId = Number(folderId);
            this.showFolderMove = true;
        },

        async saveFolderMove() {
            const selected = this.$refs.folderMoveSelect?.value;
            try {
                await KelaApi('POST', `/teacher/quizzes/folders/${this.moveFolderId}/move`, {
                    parent_id: selected ? Number(selected) : null,
                });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        async handleFolderDelete(folderId, folderName = 'Qovluq') {
            const id = Number(folderId);
            const name = folderName || 'Qovluq';
            if (!window.confirm(`'${name}' qovluğu silinsin? (İçindəki quizlər kökə daşınacaq.)`)) return;
            try {
                await KelaApi('DELETE', `/teacher/quizzes/folders/${id}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Quiz əməliyyatları ─────────────────────────────────────────────

        openQuizMove(quizId) {
            this.moveQuizId = Number(quizId);
            this.showQuizMove = true;
        },

        async deleteQuiz(quizId, quizTitle = 'Quiz') {
            const id = Number(quizId);
            const title = quizTitle || 'Quiz';
            if (!window.confirm(`'${title}' quiz silinsin?`)) return;
            try {
                await KelaApi('DELETE', `/teacher/quizzes/${id}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        async saveQuizMove() {
            const selected = this.$refs.quizMoveSelect?.value;
            try {
                await KelaApi('POST', '/teacher/quizzes/folders/move-quiz', {
                    content_id: this.moveQuizId,
                    folder_id: selected ? Number(selected) : null,
                });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Sağ-tık kontekst menyusu ───────────────────────────────────────

        /** Sətirə/karta sağ-tık — data-* atributlarından menyu qurur. */
        openRowContextMenu(e, kind, el) {
            const d = el.dataset;

            if (kind === 'folder') {
                this.openCtxMenu(e, d.folderName, [
                    { icon: 'pencil-square', iconClass: 'bg-base-200 text-base-content/70', label: 'Adını dəyiş', cls: 'text-base-content hover:bg-base-200', action: () => this.openFolderRename(d.folderId, d.folderName) },
                    { icon: 'arrows-right-left', iconClass: 'bg-info/10 text-info', label: 'Daşı', cls: 'text-base-content hover:bg-info/10 hover:text-info', action: () => this.openFolderMove(d.folderId) },
                    { divider: true },
                    { icon: 'trash', iconClass: 'bg-error/10 text-error', label: 'Sil', danger: true, cls: 'text-error hover:bg-error/10 hover:text-error', action: () => this.handleFolderDelete(d.folderId, d.folderName) },
                ]);
                return;
            }

            if (kind === 'quiz') {
                this.openCtxMenu(e, d.quizTitle, [
                    { icon: 'arrows-right-left', iconClass: 'bg-info/10 text-info', label: 'Qovluğa daşı', cls: 'text-base-content hover:bg-info/10 hover:text-info', action: () => this.openQuizMove(d.quizId) },
                    { divider: true },
                    { icon: 'eye', iconClass: 'bg-primary/10 text-primary', label: 'Profil', cls: 'text-base-content hover:bg-primary/10 hover:text-primary', href: d.openUrl },
                    { icon: 'pencil-square', iconClass: 'bg-base-200 text-base-content/70', label: 'Redaktə et', cls: 'text-base-content hover:bg-base-200', href: d.editUrl },
                    { icon: 'trash', iconClass: 'bg-error/10 text-error', label: 'Sil', danger: true, cls: 'text-error hover:bg-error/10 hover:text-error', action: () => this.deleteQuiz(d.quizId, d.quizTitle) },
                ]);
            }
        },

        closeAll() {
            this.closeCtxMenu();
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
