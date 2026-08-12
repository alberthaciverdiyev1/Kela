/**
 * Controller — Workspace səhifəsinin (base folder kataloqu) giriş nöqtəsi.
 *
 * Workspace base folder kimidir: içində qovluqlar, quiz-lər və dərslər
 * təşkil olunur. Qovluq CRUD və content daşıma backend /api/v1 üzərindəndir;
 * bu controller dialoqları idarə edir, KelaApi ilə sorğuları aparır və
 * bitdikdən sonra səhifəni təzələyir.
 *
 * Əməliyyatlar:
 *   POST   /api/v1/workspaces/{w}/folders             → yeni qovluq (name, parent_id)
 *   POST   /api/v1/workspaces/{w}/folders/{id}/rename → adı dəyiş
 *   POST   /api/v1/workspaces/{w}/folders/{id}/move   → qovluğu daşı
 *   DELETE /api/v1/workspaces/{w}/folders/{id}        → qovluğu sil (content kökə)
 *   POST   /api/v1/workspace-folders/move-content     → content-i qovluğa/kökə daşı
 *   POST   /api/v1/workspaces/{w}/students            → tələbə əlavə et
 *   DELETE /api/v1/workspaces/{w}/students/{sid}      → tələbə çıxar
 */
import Alpine from 'alpinejs';

export default function workspaceManager(config) {
    return {
        // ── Konfiqurasiya ──────────────────────────────────────────────────
        workspaceId: config.workspaceId,
        folderId: config.folderId ?? null,
        folderTree: config.folderTree || [],
        availableContents: config.availableContents || [],

        // ── UI vəziyyəti ───────────────────────────────────────────────────
        showFolderAdd: false,
        showFolderRename: false,
        showFolderMove: false,
        showContentMove: false,
        showContentAdd: false,
        showStudent: false,

        editingFolderId: null,
        moveFolderId: null,
        moveContentId: null,

        // ── Məzmun əlavə et: axtarış + tip filtri ──────────────────────────
        contentSearch: '',
        contentTypeFilter: '',

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
                await KelaApi('POST', `/api/v1/workspaces/${this.workspaceId}/folders`, {
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
                await KelaApi('POST', `/api/v1/workspaces/${this.workspaceId}/folders/${this.editingFolderId}/rename`, { name });
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
                await KelaApi('POST', `/api/v1/workspaces/${this.workspaceId}/folders/${this.moveFolderId}/move`, {
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
            if (!window.confirm(`'${name}' qovluğu silinsin? (İçindəki məzmun kökə daşınacaq.)`)) return;
            try {
                await KelaApi('DELETE', `/api/v1/workspaces/${this.workspaceId}/folders/${id}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Məzmun əməliyyatları ───────────────────────────────────────────

        openContentMove(btn) {
            this.moveContentId = Number(btn.dataset.contentId);
            this.showContentMove = true;
        },

        async saveContentMove() {
            const selected = this.$refs.contentMoveSelect?.value;
            try {
                await KelaApi('POST', '/api/v1/workspace-folders/move-content', {
                    content_id: this.moveContentId,
                    workspace_id: this.workspaceId,
                    folder_id: selected ? Number(selected) : null,
                });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Mövcud məzmunu əlavə et ───────────────────────────────────────

        /** Axtarış + tip filtri uyğun olan məzmun görünür. */
        isContentVisible(c) {
            if (this.contentTypeFilter !== '' && String(c.type) !== this.contentTypeFilter) {
                return false;
            }
            const q = this.contentSearch.trim().toLowerCase();
            if (q && !c.title.toLowerCase().includes(q)) {
                return false;
            }

            return true;
        },

        get filteredAvailableContents() {
            return this.availableContents.filter((c) => this.isContentVisible(c));
        },

        openContentAdd() {
            this.contentSearch = '';
            this.contentTypeFilter = '';
            this.showContentAdd = true;
        },

        async saveContentAdd() {
            const targetFolder = this.$refs.contentAddSelect?.value
                ? Number(this.$refs.contentAddSelect.value)
                : null;
            const checked = Array.from(
                this.$root.querySelectorAll('input[type="checkbox"]:checked'),
            ).map((el) => Number(el.value));

            if (checked.length === 0) {
                window.alert('Ən azı bir məzmun seçin.');
                return;
            }

            try {
                for (const id of checked) {
                    await KelaApi('POST', '/api/v1/workspace-folders/move-content', {
                        content_id: id,
                        workspace_id: this.workspaceId,
                        folder_id: targetFolder,
                    });
                }
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Tələbə əməliyyatları ───────────────────────────────────────────

        async saveStudents() {
            const select = this.$refs.studentSelect;
            const ids = Array.from(select?.selectedOptions ?? [])
                .map((o) => Number(o.value))
                .filter((n) => Number.isFinite(n));
            if (ids.length === 0) {
                window.alert('Ən azı bir tələbə seçin.');
                return;
            }
            try {
                await KelaApi('POST', `/api/v1/workspaces/${this.workspaceId}/students`, { student_ids: ids });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        async detachStudent(btn) {
            const id = btn.dataset.studentId;
            const name = btn.dataset.studentName || 'Tələbə';
            if (!window.confirm(`'${name}' workspace-dən çıxarılsın?`)) return;
            try {
                await KelaApi('DELETE', `/api/v1/workspaces/${this.workspaceId}/students/${id}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Delegasiya ─────────────────────────────────────────────────────

        /**
         * onTableClick(e) — Kataloq üzərində click hadisəsi.
         * data-folder-action / data-content-action düymələrini müvafiq əməliyyata ötürür.
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
            const cBtn = e.target.closest('[data-content-action]');
            if (cBtn) {
                const action = cBtn.dataset.contentAction;
                if (action === 'move') this.openContentMove(cBtn);
            }
        },

        closeAll() {
            this.showFolderAdd = false;
            this.showFolderRename = false;
            this.showFolderMove = false;
            this.showContentMove = false;
            this.showContentAdd = false;
            this.showStudent = false;
        },
    };
}

// Alpine-də qeydiyyat: blade-də x-data="workspaceManager(...)" işləyə bilsin.
Alpine.data('workspaceManager', workspaceManager);
// Alpine-i işə salır. Bu entry yalnız workspace səhifəsində yüklənir.
Alpine.start();
