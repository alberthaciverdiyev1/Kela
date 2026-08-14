/**
 * Controller — Dərslər səhifəsinin (qovluqlu kataloq) giriş nöqtəsi.
 *
 * Dərs qovluqları backend /api/v1/lesson-folders üzərindəndir; bu controller
 * dialoqları idarə edir, KelaApi ilə sorğuları aparır və bitdikdən sonra
 * səhifəni təzələyir (dərs siyahısı paginated server-rendered-dir).
 *
 * Əməliyyatlar:
 *   POST   /api/v1/lesson-folders             → yeni qovluq (name, parent_id)
 *   POST   /api/v1/lesson-folders/{id}/rename → adı dəyiş
 *   POST   /api/v1/lesson-folders/{id}/move   → qovluğu daşı
 *   DELETE /api/v1/lesson-folders/{id}        → qovluğu sil (dərslər kökə)
 *   POST   /api/v1/lesson-folders/move-lesson → dərsi qovluğa daşı
 */
import Alpine from 'alpinejs';
import createContextMenu from '../context-menu';

export default function lessonFolders(config) {
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
        showLessonMove: false,
        // Kataloq görünümü: 'list' | 'grid' (localStorage-da saxlanılır).
        viewMode: localStorage.getItem('workspace-view') || 'list',

        editingFolderId: null,
        moveFolderId: null,
        moveLessonId: null,

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
                await KelaApi('POST', '/api/v1/lesson-folders', {
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
                await KelaApi('POST', `/api/v1/lesson-folders/${this.editingFolderId}/rename`, { name });
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
                await KelaApi('POST', `/api/v1/lesson-folders/${this.moveFolderId}/move`, {
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
            if (!window.confirm(`'${name}' qovluğu silinsin? (İçindəki dərslər kökə daşınacaq.)`)) return;
            try {
                await KelaApi('DELETE', `/api/v1/lesson-folders/${id}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Dərs əməliyyatları ─────────────────────────────────────────────

        openLessonMove(lessonId) {
            this.moveLessonId = Number(lessonId);
            this.showLessonMove = true;
        },

        async deleteLesson(lessonId, lessonTitle = 'Dərs') {
            const id = Number(lessonId);
            const title = lessonTitle || 'Dərs';
            if (!window.confirm(`'${title}' dərsini silmək istəyirsiniz?`)) return;
            try {
                await KelaApi('DELETE', `/api/v1/lessons/${id}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        async saveLessonMove() {
            const selected = this.$refs.lessonMoveSelect?.value;
            try {
                await KelaApi('POST', '/api/v1/lesson-folders/move-lesson', {
                    content_id: this.moveLessonId,
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

            if (kind === 'lesson') {
                this.openCtxMenu(e, d.lessonTitle, [
                    { icon: 'arrows-right-left', iconClass: 'bg-info/10 text-info', label: 'Qovluğa daşı', cls: 'text-base-content hover:bg-info/10 hover:text-info', action: () => this.openLessonMove(d.lessonId) },
                    { divider: true },
                    { icon: 'eye', iconClass: 'bg-primary/10 text-primary', label: 'Aç', cls: 'text-base-content hover:bg-primary/10 hover:text-primary', href: d.openUrl },
                    { icon: 'pencil-square', iconClass: 'bg-base-200 text-base-content/70', label: 'Redaktə et', cls: 'text-base-content hover:bg-base-200', href: d.editUrl },
                    { icon: 'trash', iconClass: 'bg-error/10 text-error', label: 'Sil', danger: true, cls: 'text-error hover:bg-error/10 hover:text-error', action: () => this.deleteLesson(d.lessonId, d.lessonTitle) },
                ]);
            }
        },

        closeAll() {
            this.closeCtxMenu();
            this.showFolderAdd = false;
            this.showFolderRename = false;
            this.showFolderMove = false;
            this.showLessonMove = false;
        },
    };
}

// Alpine-də qeydiyyat: blade-də x-data="lessonFolders(...)" işləyə bilsin.
Alpine.data('lessonFolders', lessonFolders);
// Alpine-i işə salır. Bu entry yalnız dərslər səhifəsində yüklənir.
Alpine.start();
