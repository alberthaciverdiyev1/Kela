/**
 * Controller — Workspace səhifəsinin (base folder kataloqu) giriş nöqtəsi.
 *
 * Workspace base folder kimidir: içində qovluqlar, quiz-lər və dərslər
 * təşkil olunur. Qovluq CRUD və content daşıma web controller-i
 * (WorkspaceController) işləyir; bu controller dialoqları idarə edir,
 * KelaApi ilə sorğuları aparır və bitdikdən sonra səhifəni təzələyir.
 *
 * Əməliyyatlar (hamısı web controller — /api/v1 yoxdur):
 *   POST   /teacher/workspaces/{w}/folders             → yeni qovluq (name, parent_id)
 *   POST   /teacher/workspaces/{w}/folders/{id}/rename → adı dəyiş
 *   POST   /teacher/workspaces/{w}/folders/{id}/move   → qovluğu daşı
 *   DELETE /teacher/workspaces/{w}/folders/{id}        → qovluğu sil (content kökə)
 *   POST   /teacher/workspace-folders/move-content     → content-i qovluğa/kökə daşı
 *   POST   /teacher/workspaces/{w}/students            → tələbə əlavə et
 *   DELETE /teacher/workspaces/{w}/students/{sid}      → tələbə çıxar
 */
import Alpine from 'alpinejs';
import createContextMenu from '../context-menu';

export default function workspaceManager(config) {
    return {
        // ── Ortaq sağ-tık kontekst menyusu ────────────────────────────────
        ...createContextMenu(),

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
        showGenerateStudent: false,
        // Kataloq görünümü: 'list' | 'grid' (localStorage-da saxlanılır).
        viewMode: localStorage.getItem('workspace-view') || 'list',

        editingFolderId: null,
        moveFolderId: null,
        moveContentId: null,

        // ── Məzmun əlavə et: axtarış + tip filtri ──────────────────────────
        contentSearch: '',
        contentTypeFilter: '',
        selectedContentIds: [],
        // Seçilmiş bank qovluqları: { folder_id, type, name } — bütöv qovluq əlavəsi üçün.
        selectedFolders: [],

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
                await KelaApi('POST', `/teacher/workspaces/${this.workspaceId}/folders`, {
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
                await KelaApi('POST', `/teacher/workspaces/${this.workspaceId}/folders/${this.editingFolderId}/rename`, { name });
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
                await KelaApi('POST', `/teacher/workspaces/${this.workspaceId}/folders/${this.moveFolderId}/move`, {
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
            if (!window.confirm(`'${name}' qovluğu və içindəki bütün məzmunlar silinsin? (Bu əməliyyat geri qaytarıla bilməz.)`)) return;
            try {
                await KelaApi('DELETE', `/teacher/workspaces/${this.workspaceId}/folders/${id}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        /** Qovluğu içindəki məzmunlarla birlikdə kütüphanəyə geri göndərir. */
        async handleFolderRemove(folderId, folderName = 'Qovluq') {
            const id = Number(folderId);
            const name = folderName || 'Qovluq';
            if (!window.confirm(`'${name}' qovluğu və içindəki məzmunlar workspace-dən çıxarılsın? (Kütüphanəyə qayıdacaq.)`)) return;
            try {
                await KelaApi('POST', `/teacher/workspaces/${this.workspaceId}/folders/${id}/remove`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        /** İçeriği workspace-dən kütüphanəyə geri göndərir. */
        async handleContentRemove(contentId, contentTitle = 'Məzmun') {
            const id = Number(contentId);
            const title = contentTitle || 'Məzmun';
            if (!window.confirm(`'${title}' workspace-dən çıxarılsın? (Kütüphanəyə qayıdacaq.)`)) return;
            try {
                await KelaApi('POST', '/teacher/workspace-folders/remove-content', { content_id: id });
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Məzmun əməliyyatları ───────────────────────────────────────────

        openContentMove(contentId) {
            this.moveContentId = Number(contentId);
            this.showContentMove = true;
        },

        async saveContentMove() {
            const selected = this.$refs.contentMoveSelect?.value;
            try {
                await KelaApi('POST', '/teacher/workspace-folders/move-content', {
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
            const root = { key: '', name: '', depth: -1, children: [], contents: [], folder_id: null, type: null };
            const nodes = { '': root };

            for (const c of this.availableContents) {
                const names = c.folder_path || [];
                const ids = c.folder_path_ids || [];
                let parent = root;
                let acc = [];

                for (let i = 0; i < names.length; i++) {
                    acc.push(names[i]);
                    // Açar tipə görə unikaldır — quiz/dərs qovluğu eyni adda ola bilər.
                    const key = c.type + '::' + acc.join(' › ');
                    if (!nodes[key]) {
                        nodes[key] = {
                            key,
                            name: names[i],
                            depth: i,
                            folder_id: ids[i] ?? null,
                            type: c.type,
                            // Kökdən bu qovluğa qədər bütün ata id-ləri (kaskad seçim üçün)
                            path_ids: ids.slice(0, i + 1),
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
                        folder_id: node.folder_id,
                        type: node.type,
                        path_ids: node.path_ids,
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
        //
        // Kaskad qaydası: bir qovluq işarələndikdə onun bütün alt qovluqları
        // və içindəki məzmunlar da seçilir; işarə silinəndə bütün alt ağac
        // seçimdən çıxarılır. Beləliklə "üst qovluq" ilə "alt ağac" heç vaxt
        // bir-birindən ayrılmır.

        isFolderSelected(f) {
            return this.selectedFolders.some(
                (x) => x.folder_id === f.folder_id && x.type === f.type,
            );
        },

        /** Qovluq seçilmiş hər hansı bir ATA qovluğun altındadırmı? */
        isFolderCovered(f) {
            const path = f.path_ids || [f.folder_id];
            return this.selectedFolders.some(
                (x) => x.type === f.type && x.folder_id !== f.folder_id && path.includes(x.folder_id),
            );
        },

        /** Məzmun seçilmiş bir bank qovluğunun altındadırmı? */
        isContentCoveredByFolder(c) {
            const ids = c.folder_path_ids || [];
            return this.selectedFolders.some(
                (f) => f.type === c.type && ids.includes(f.folder_id),
            );
        },

        /** Məzmun seçilib (ayrıca və ya qovluq vasitəsilə)? */
        isContentSelected(c) {
            return this.selectedContentIds.includes(c.content_id) || this.isContentCoveredByFolder(c);
        },

        /** Ağacda verilmiş bank qovluğunun düyününü tapır. */
        findTreeFolderNode(type, folderId) {
            const walk = (node) => {
                if (node.type === type && node.folder_id === folderId) return node;
                for (const child of node.children) {
                    const hit = walk(child);
                    if (hit) return hit;
                }
                return null;
            };
            return walk(this.contentTree);
        },

        /** Qovluğun bütün ALT ağacı: alt qovluqlar + içlərindəki məzmunlar. */
        subtreeOf(f) {
            const node = this.findTreeFolderNode(f.type, f.folder_id);
            const folders = [];
            const contents = [];
            if (!node) return { folders, contents };

            const walk = (n) => {
                for (const child of n.children) {
                    folders.push({
                        folder_id: child.folder_id,
                        type: child.type,
                        name: child.name,
                        path_ids: child.path_ids,
                    });
                    walk(child);
                }
                for (const c of n.contents) {
                    contents.push(c);
                }
            };
            walk(node);

            return { folders, contents };
        },

        /** Qovluq işarələndi: özü + bütün alt ağacı seçimə əlavə et. */
        addSubtreeSelection(f) {
            const { folders, contents } = this.subtreeOf(f);
            for (const x of [f, ...folders]) {
                if (!this.isFolderSelected(x)) {
                    this.selectedFolders.push({
                        folder_id: x.folder_id,
                        type: x.type,
                        name: x.name,
                        path_ids: x.path_ids,
                    });
                }
            }
            for (const c of contents) {
                if (!this.selectedContentIds.includes(c.content_id)) {
                    this.selectedContentIds.push(c.content_id);
                }
            }
        },

        /** Qovluğun işarəsi silindi: özü + bütün alt ağacı seçimdən çıxar. */
        removeSubtreeSelection(f) {
            const { folders, contents } = this.subtreeOf(f);
            const removeKeys = new Set(
                [f, ...folders].map((x) => x.folder_id + ':' + x.type),
            );
            this.selectedFolders = this.selectedFolders.filter(
                (x) => !removeKeys.has(x.folder_id + ':' + x.type),
            );
            const contentIds = new Set(contents.map((c) => c.content_id));
            this.selectedContentIds = this.selectedContentIds.filter(
                (id) => !contentIds.has(id),
            );
        },

        toggleFolderSelection(f) {
            if (this.isFolderSelected(f)) {
                this.removeSubtreeSelection(f);
            } else {
                this.addSubtreeSelection(f);
            }
        },

        updateContentSelection(e) {
            const id = Number(e.target.value);
            const c = this.availableContents.find((x) => x.content_id === id);
            if (c && this.isContentCoveredByFolder(c)) return; // qovluq vasitəsilə seçilib

            if (e.target.checked) {
                if (!this.selectedContentIds.includes(id)) {
                    this.selectedContentIds.push(id);
                }
            } else {
                this.selectedContentIds = this.selectedContentIds.filter((x) => x !== id);
            }
        },

        /** Başqa seçilmiş qovluq tərəfindən örtülməyən (ən üst) seçilmiş qovluqlar. */
        get topLevelSelectedFolders() {
            return this.selectedFolders.filter((f) => !this.isFolderCovered(f));
        },

        /** Qovluqla örtülməyən ayrıca seçilmiş məzmunlar (move-content-lə əlavə ediləcək). */
        get effectiveIndividualContents() {
            return this.selectedContentIds.filter((id) => {
                const c = this.availableContents.find((x) => x.content_id === id);
                return c && !this.isContentCoveredByFolder(c);
            });
        },

        get selectedFolderCount() {
            return this.topLevelSelectedFolders.length;
        },

        get selectedContentCount() {
            return this.selectedFolderCount + this.effectiveIndividualContents.length;
        },

        get selectionSummary() {
            const folders = this.selectedFolderCount;
            const contents = this.effectiveIndividualContents.length;
            if (folders > 0 && contents > 0) {
                return folders + ' qovluq, ' + contents + ' məzmun seçildi';
            }
            if (folders > 0) {
                return folders + ' qovluq seçildi';
            }
            if (contents > 0) {
                return contents + ' məzmun seçildi';
            }
            return 'Seçilməyib';
        },

        openContentAdd() {
            this.contentSearch = '';
            this.contentTypeFilter = '';
            this.selectedContentIds = [];
            this.selectedFolders = [];
            this.showContentAdd = true;
        },

        async saveContentAdd() {
            const targetFolder = this.$refs.contentAddSelect?.value
                ? Number(this.$refs.contentAddSelect.value)
                : null;

            if (this.selectedContentCount === 0) {
                window.alert('Ən azı bir qovluq və ya məzmun seçin.');
                return;
            }

            try {
                // 1) Bütöv qovluqlar — strukturu ilə birlikdə (ən üst seçilmişlər,
                //    alt ağac add-folder tərəfindən avtomatik gətirilir).
                for (const f of this.topLevelSelectedFolders) {
                    await KelaApi('POST', '/teacher/workspace-folders/add-folder', {
                        folder_type: f.type === 1 ? 'quiz' : 'lesson',
                        bank_folder_id: f.folder_id,
                        workspace_id: this.workspaceId,
                        folder_id: targetFolder,
                    });
                }
                // 2) Qovluqla örtülməyən ayrıca seçilmiş məzmunlar.
                for (const id of this.effectiveIndividualContents) {
                    await KelaApi('POST', '/teacher/workspace-folders/move-content', {
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
            const container = this.$refs.studentSelect;
            const checkboxes = container ? Array.from(container.querySelectorAll('input[type="checkbox"]:checked')) : [];
            const ids = checkboxes
                .map((cb) => Number(cb.value))
                .filter((n) => Number.isFinite(n));
            if (ids.length === 0) {
                window.alert('Ən azı bir tələbə seçin.');
                return;
            }
            
            const payload = { student_ids: ids };
            const price = this.$refs.studentPrice?.value;
            const date = this.$refs.studentStartDate?.value;
            if (price) payload.agreed_price = price;
            if (date) payload.start_date = date;
            
            try {
                await KelaApi('POST', `/teacher/workspaces/${this.workspaceId}/students`, payload);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        async detachStudent(studentId, studentName = 'Tələbə') {
            const id = Number(studentId);
            const name = studentName || 'Tələbə';
            if (!window.confirm(`'${name}' workspace-dən çıxarılsın?`)) return;
            try {
                await KelaApi('DELETE', `/teacher/workspaces/${this.workspaceId}/students/${id}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        // ── Sağ-tık kontekst menyusu ───────────────────────────────────────

        /**
         * openRowContextMenu(e, kind, el) — Kataloq sətirinə/kartına sağ-tık.
         * data-* atributlarından oxuyub müvafiq aksiya menyusunu qurur.
         */
        openRowContextMenu(e, kind, el) {
            const d = el.dataset;

            if (kind === 'folder') {
                this.openCtxMenu(e, d.folderName, [
                    { icon: 'pencil-square', iconClass: 'bg-base-200 text-base-content/70', label: 'Adını dəyiş', cls: 'text-base-content hover:bg-base-200', action: () => this.openFolderRename(d.folderId, d.folderName) },
                    { icon: 'arrows-right-left', iconClass: 'bg-info/10 text-info', label: 'Daşı', cls: 'text-base-content hover:bg-info/10 hover:text-info', action: () => this.openFolderMove(d.folderId) },
                    { icon: 'arrow-up-tray', iconClass: 'bg-warning/10 text-warning', label: 'Workspace-dən çıxar', cls: 'text-base-content hover:bg-warning/10 hover:text-warning', action: () => this.handleFolderRemove(d.folderId, d.folderName) },
                    { divider: true },
                    { icon: 'trash', iconClass: 'bg-error/10 text-error', label: 'Sil', danger: true, cls: 'text-error hover:bg-error/10 hover:text-error', action: () => this.handleFolderDelete(d.folderId, d.folderName) },
                ]);
                return;
            }

            if (kind === 'content') {
                const items = [
                    { icon: 'arrows-right-left', iconClass: 'bg-info/10 text-info', label: 'Qovluğa daşı', cls: 'text-base-content hover:bg-info/10 hover:text-info', action: () => this.openContentMove(d.contentId) },
                    { icon: 'arrow-up-tray', iconClass: 'bg-warning/10 text-warning', label: 'Workspace-dən çıxar', cls: 'text-base-content hover:bg-warning/10 hover:text-warning', action: () => this.handleContentRemove(d.contentId, d.contentTitle) },
                ];
                if (d.editUrl) {
                    items.push(
                        { divider: true },
                        { icon: d.contentType === '1' ? 'clipboard-document-list' : 'video-camera', iconClass: 'bg-primary/10 text-primary', label: 'Redaktə et', cls: 'text-base-content hover:bg-primary/10 hover:text-primary', href: d.editUrl },
                    );
                }
                this.openCtxMenu(e, d.contentTitle, items);
                return;
            }

            if (kind === 'student') {
                this.openCtxMenu(e, d.studentName, [
                    { icon: 'user-minus', iconClass: 'bg-error/10 text-error', label: 'Workspace-dən çıxar', danger: true, cls: 'text-error hover:bg-error/10 hover:text-error', action: () => this.detachStudent(d.studentId, d.studentName) },
                ]);
            }
        },

        closeAll() {
            this.closeCtxMenu();
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

/**
 * Workspace indeks səhifəsi üçün sadə görünüm idarəçisi (List / Grid).
 */
function workspaceList(config = {}) {
    return {
        ...createContextMenu(),

        // Kataloq görünümü: 'list' | 'grid' (localStorage-da saxlanılır).
        viewMode: localStorage.getItem('workspace-view') || 'list',

        // ── Yeni workspace modalı ─────────────────────────────────────────
        // autoCreate: dashboard "Yeni İş Sahəsi" → ?create=1 ilə gələndə açılır.
        showCreate: Boolean(config.autoCreate),

        setViewMode(mode) {
            this.viewMode = mode;
            try {
                localStorage.setItem('workspace-view', mode);
            } catch (e) { /* localStorage əlçatan deyilsə sadəcə seans üçün qalır */ }
        },

        openCreate() {
            this.showCreate = true;
        },

        /** Workspace kartı/sətirinə sağ-tık menyusu. */
        openWorkspaceContextMenu(e, el) {
            const d = el.dataset;
            this.openCtxMenu(e, d.workspaceName, [
                { icon: 'building-office-2', iconClass: 'bg-primary/10 text-primary', label: 'Aç', cls: 'text-base-content hover:bg-primary/10 hover:text-primary', href: d.openUrl },
                { icon: 'pencil-square', iconClass: 'bg-base-200 text-base-content/70', label: 'Redaktə et', cls: 'text-base-content hover:bg-base-200', href: d.editUrl },
                { divider: true },
                { icon: 'trash', iconClass: 'bg-error/10 text-error', label: 'Sil', danger: true, cls: 'text-error hover:bg-error/10 hover:text-error', action: () => this.deleteWorkspace(d.workspaceId) },
            ]);
        },

        async deleteWorkspace(workspaceId) {
            if (!window.confirm('Bu workspace silinsin?')) return;
            try {
                await KelaApi('DELETE', `/teacher/workspaces/${Number(workspaceId)}`);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
            }
        },

        closeAll() {
            this.closeCtxMenu();
            this.showCreate = false;
        },
    };
}
Alpine.data('workspaceList', workspaceList);

// Alpine-i işə salır. Bu entry yalnız workspace səhifələrində yüklənir.
Alpine.start();
