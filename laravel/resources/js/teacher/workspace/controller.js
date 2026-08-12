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
        selectedContentIds: [],

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

        /** availableContents → bank qovluq ağacı (folder_path üzrə). */
        get contentTree() {
            const root = { key: '', name: '', depth: -1, children: [], contents: [] };
            const nodes = { '': root };

            for (const c of this.availableContents) {
                const path = c.folder_path || [];
                let parent = root;
                let acc = [];

                for (const name of path) {
                    acc.push(name);
                    const key = acc.join(' › ');
                    if (!nodes[key]) {
                        nodes[key] = {
                            key,
                            name,
                            depth: acc.length - 1,
                            children: [],
                            contents: [],
                        };
                        parent.children.push(nodes[key]);
                    }
                    parent = nodes[key];
                }
                parent.contents.push(c);
            }

            return root;
        },

        /**
         * Ağacı düz satırlara çevirir (qovluq başlıqları + içindəki məzmunlar).
         * Collapse yoxdur — bütün məzmunlar hər zaman görünür.
         * Axtarış/filtre aktiv olduqda boş qovluqlar gizlənir.
         */
        get visibleContentRows() {
            const filtering = this.contentSearch.trim() !== '' || this.contentTypeFilter !== '';
            const rows = [];

            // Hər qovluğun görünən (filtre uyğun) məzmun sayını bir dəfə hesabla
            const visibleCounts = {};
            const countVisible = (node) => {
                let n = node.contents.filter((c) => this.isContentVisible(c)).length;
                for (const child of node.children) {
                    n += countVisible(child);
                }
                visibleCounts[node.key] = n;
                return n;
            };
            countVisible(this.contentTree);

            const walk = (node) => {
                if (node.depth >= 0) {
                    const vis = visibleCounts[node.key];
                    if (vis === 0) {
                        return; // məzmunu olmayan qovluğu heç göstərmə
                    }
                    rows.push({
                        kind: 'folder',
                        key: 'f:' + node.key,
                        name: node.name,
                        depth: node.depth,
                        count: vis,
                    });
                }

                for (const c of node.contents) {
                    if (!this.isContentVisible(c)) {
                        continue;
                    }
                    rows.push({ kind: 'content', key: 'c:' + c.content_id, c, depth: node.depth + 1 });
                }
                for (const child of node.children) {
                    walk(child);
                }
            };
            walk(this.contentTree);

            return rows;
        },

        get visibleContentCount() {
            return this.visibleContentRows.filter((r) => r.kind === 'content').length;
        },

        // ── Seçim — ────────────────────────────────────────────────────────

        updateContentSelection(e) {
            const id = Number(e.target.value);
            if (e.target.checked) {
                if (!this.selectedContentIds.includes(id)) {
                    this.selectedContentIds.push(id);
                }
            } else {
                this.selectedContentIds = this.selectedContentIds.filter((x) => x !== id);
            }
        },

        get selectedContentCount() {
            return this.selectedContentIds.length;
        },

        openContentAdd() {
            this.contentSearch = '';
            this.contentTypeFilter = '';
            this.selectedContentIds = [];
            this.showContentAdd = true;
        },

        async saveContentAdd() {
            const targetFolder = this.$refs.contentAddSelect?.value
                ? Number(this.$refs.contentAddSelect.value)
                : null;

            if (this.selectedContentIds.length === 0) {
                window.alert('Ən azı bir məzmun seçin.');
                return;
            }

            try {
                for (const id of this.selectedContentIds) {
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
