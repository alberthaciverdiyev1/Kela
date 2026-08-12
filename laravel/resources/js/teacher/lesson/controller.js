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

export default function lessonFolders(config) {
    return {
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
                await KelaApi('POST', `/api/v1/lesson-folders/${this.editingFolderId}/rename`, { name });
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
                await KelaApi('POST', `/api/v1/lesson-folders/${this.moveFolderId}/move`, {
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
            if (!window.confirm(`'${name}' qovluğu silinsin? (İçindəki dərslər kökə daşınacaq.)`)) return;
            try {
                await KelaApi('DELETE', `/api/v1/lesson-folders/${id}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Dərs əməliyyatları ─────────────────────────────────────────────

        openLessonMove(btn) {
            this.moveLessonId = Number(btn.dataset.lessonId);
            this.showLessonMove = true;
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

        // ── Delegasiya ─────────────────────────────────────────────────────

        /**
         * onTableClick(e) — Kataloq üzərində click hadisəsi.
         * data-folder-action / data-lesson-action düymələrini müvafiq əməliyyata ötürür.
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
            const lBtn = e.target.closest('[data-lesson-action]');
            if (lBtn) {
                const action = lBtn.dataset.lessonAction;
                if (action === 'move') this.openLessonMove(lBtn);
            }
        },

        closeAll() {
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
