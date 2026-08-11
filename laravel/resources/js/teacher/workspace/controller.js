/**
 * Controller — Workspace file-manager səhifəsinin giriş nöqtəsi və "orkestr"i.
 *
 * Niyə tək fayl (index.js YOXDUR): bu fayl həm Alpine komponentini
 * (workspaceManager) qeydiyyatdan keçirir, həm də səhifə məntiqini idarə edir.
 * Add/edit/list/delete funksiya kodu burada deyil — hər əməliyyat öz
 * modulundadır:
 *
 *   list.js      → kataloqu yenilə     (refresh)
 *   add.js       → qovluq/məzmun/tələbə əlavə et (addFolder/addContent/addStudents)
 *   edit.js      → node adını dəyiş / daşı (renameNode/moveNode)
 *   delete.js    → node sil / tələbə çıxar (deleteNode/detachStudent)
 *
 * Əsl HTTP çağırışı add/edit/delete/list modullarındadır. Controller yalnız:
 * dialoqların açıq/bağlı vəziyyətini (showRename/showMove/showFolder/...)
 * saxlayır, moduldan { ok } gəlincə kataloqu təzələyir.
 */
import Alpine from 'alpinejs';
import createDirectoryList from './list';
import createWorkspaceAdder from './add';
import createWorkspaceEditor from './edit';
import createWorkspaceRemover from './delete';

export default function workspaceManager(config) {
    const api = '/api/v1/workspaces/' + config.workspaceId;

    // CRUD modulları — hər biri öz əməliyyatını bilir, bir-birindən asılı deyil.
    // parentId statikdir, bu səbəbdən adder konstruktorda birbaşa alır.
    const list = createDirectoryList(config.fragmentUrl);
    const adder = createWorkspaceAdder({ api, parentId: config.parentId });
    const editor = createWorkspaceEditor({ api });
    const remover = createWorkspaceRemover({ api });

    return {
        // ── Konfiqurasiya ──────────────────────────────────────────────────
        workspaceId: config.workspaceId,
        parentId: config.parentId,
        fragmentUrl: config.fragmentUrl,
        api,

        // ── UI vəziyyəti (Alpine reaktiv dəyişənləri) ──────────────────────
        // Dialoqların açıq/bağlı olması (x-show ilə bağlı).
        showRename: false,
        showMove: false,
        showFolder: false,
        showContent: false,
        showStudent: false,

        // Dialoq daxilindəki mətn dəyərləri (x-model ilə bağlı) — bunlar
        // reaktiv UI vəziyyətidir, buna görə komponent data obyektindədirlər.
        renameName: '',       // Adını dəyiş dialoqundakı mətn
        folderName: '',       // Yeni qovluq dialoqundakı mətn
        moveTargetId: null,   // Daşınacaq node-un id-si (openMove-də doldurulur)
        renameTargetId: null, // Adı dəyişiləcək node-un id-si (openRename-də doldurulur)

        /**
         * refresh() — Kataloqu yenidən çəkir.
         *
         * Niyə fragment: əməliyyatdan sonra tam səhifə reload etmək əvəzinə
         * yalnız kataloq hissəsi serverdən təzə fragment olaraq gətirilib
         * DOM-da əvəz olunur (list modulu vasitəsilə).
         */
        async refresh() {
            await list.refresh(this.$refs.directory);
        },

        /**
         * onDirectoryClick(e) — Kataloq üzərində click hadisəsi (delegasiya).
         *
         * Niyə delegasiya: kataloq hər refresh-də (list.refresh) yenidən
         * render olunur — hər düyməyə ayrıca listener bağlasaq, refresh-dən
         * sonra köhnə listener-lər ölür. Bunun əvəzinə bir dəfə kataloqa
         * @click bağlanır; bu metod data-node-action düyməsini tapıb müvafiq
         * əməliyyata ötürür. Kataloqdan kənar kliklər (məs. tələbə sətri)
         * yoxlanılıb buraxılır.
         */
        onDirectoryClick(e) {
            const btn = e.target.closest('[data-node-action]');
            if (!btn || !this.$refs.directory.contains(btn)) return;
            const action = btn.dataset.nodeAction;
            if (action === 'rename') this.openRename(btn);
            else if (action === 'move') this.openMove(btn);
            else if (action === 'delete') this.handleDelete(btn);
        },

        /**
         * openRename(btn) — "Adını dəyiş" düyməsi basılanda.
         *
         * Node id-sini və cari adını data attribute-dan oxuyub dialoqa yazır.
         * showRename=true dialoqu açır; $nextTick fokusu input-a verir ki,
         * istifadəçi dərhal yazmağa başlasın.
         */
        openRename(btn) {
            this.renameTargetId = Number(btn.dataset.nodeId);
            this.renameName = btn.dataset.nodeName || '';
            this.showRename = true;
            this.$nextTick(() => this.$refs.renameInput?.focus());
        },

        /**
         * openMove(btn) — "Daşı" düyməsi basılanda.
         *
         * Hədəf seçimini doldurur (populateMoveOptions) və dialoqu açır.
         */
        openMove(btn) {
            this.moveTargetId = Number(btn.dataset.nodeId);
            this.populateMoveOptions();
            this.showMove = true;
        },

        /**
         * populateMoveOptions() — Daşıma dialoqundakı select-i qovluq
         * ağacı ilə doldurur.
         *
         * Niyə template-dən klonlama: server-rendered kataloq fragmentində
         * gizli #folder-tree-template (template) elementi var. Server özü
         * bütün qovluq ağacını HTML olaraq hazırlayır (breadcrumbs/yol
         * vəziyyəti qorunur); bu metod yalnız onu select-ə köçürür. Kök
         * seçimi həmişə mövcuddur — boş seçim kökə daşımaq deməkdir.
         */
        populateMoveOptions() {
            const tpl = this.$refs.directory.querySelector('#folder-tree-template');
            const select = this.$refs.moveSelect;
            if (!tpl || !select) return;
            select.innerHTML = '<option value="">Kök qovluğa</option>';
            tpl.content.querySelectorAll('option').forEach((o) =>
                select.appendChild(o.cloneNode(true)),
            );
        },

        /**
         * handleDelete(btn) — "Sil" düyməsi basılanda.
         *
         * Silməni delete moduluna həvalə edir; uğurlu olarsa kataloqu təzələyir.
         */
        async handleDelete(btn) {
            const ok = await remover.deleteNode(btn.dataset.nodeId, btn.dataset.nodeName);
            if (ok) await this.refresh();
        },

        /**
         * saveRename() — Adını dəyiş dialoqundakı "Saxla" düyməsi basılanda.
         *
         * Dəyəri rename moduluna ötürür; uğursuz → dialoq açıq qalır,
         * uğurlu → dialoq bağlanır və kataloq təzələnir (yeni ad görünür).
         */
        async saveRename() {
            const ok = await editor.renameNode(this.renameTargetId, this.renameName.trim());
            if (!ok) return;
            this.showRename = false;
            await this.refresh();
        },

        /**
         * saveMove() — Daşı dialoqundakı "Daşı" düyməsi basılanda.
         *
         * Seçilmiş hədəf qovluğu edit moduluna ötürür; boş seçim kökə
         * daşımaq deməkdir (null).
         */
        async saveMove() {
            const selected = this.$refs.moveSelect.value;
            const ok = await editor.moveNode(this.moveTargetId, selected ? Number(selected) : null);
            if (!ok) return;
            this.showMove = false;
            await this.refresh();
        },

        /**
         * saveFolder() — Yeni qovluq dialoqundakı "Yarat" düyməsi basılanda.
         *
         * Adı add moduluna ötürür; uğurlu olarsa dialoqu bağlayır, sahəni
         * təmizləyir və kataloqu təzələyir (yeni qovluq görünür).
         */
        async saveFolder() {
            const ok = await adder.addFolder(this.folderName.trim());
            if (!ok) return;
            this.showFolder = false;
            this.folderName = '';
            await this.refresh();
        },

        /**
         * saveContent() — Məzmun dialoqundakı "Əlavə Et" düyməsi basılanda.
         *
         * Seçilmiş content_id DOM-dan ($refs) oxunub add moduluna verilir;
         * uğurlu olarsa dialoq bağlanır, seçim sıfırlanır, kataloq təzələnir.
         */
        async saveContent() {
            const select = this.$refs.contentSelect;
            const ok = await adder.addContent(select.value);
            if (!ok) return;
            this.showContent = false;
            select.value = '';
            await this.refresh();
        },

        /**
         * saveStudents() — Tələbə dialoqundakı "Əlavə Et" düyməsi basılanda.
         *
         * Çoxlu seçim (multi-select) dəyərlərini DOM-dan toplayıb add moduluna
         * verir. Niyə reload: tələbə siyahısı kataloqda deyil, ayrıca "Tələbələr"
         * bölməsində göstərilir — təzə server renderi ən sadə yoldur.
         */
        async saveStudents() {
            const select = this.$refs.studentSelect;
            const ids = Array.from(select.selectedOptions).map((o) => Number(o.value));
            const ok = await adder.addStudents(ids);
            if (!ok) return;
            window.location.reload();
        },

        /**
         * detachStudent(btn) — Tələbə sətirindəki "Çıxar" düyməsi basılanda.
         *
         * Silmə/çıxarma delete moduluna həvalə edilir; uğurlu olarsa səhifə
         * reload olunur (tələbə siyahısı kataloqdan ayrıdır).
         */
        async detachStudent(btn) {
            const ok = await remover.detachStudent(btn.dataset.studentId, btn.dataset.studentName);
            if (ok) window.location.reload();
        },

        /**
         * closeAll() — Escape düyməsi basılanda bütün dialoqları bağlayır.
         *
         * Niyə tək metod: blade-də @keydown.escape.window="closeAll()" var —
         * istifadəçi hansı dialoqu açıbsa, tək çağırışla hamısı bağlanır.
         */
        closeAll() {
            this.showRename = false;
            this.showMove = false;
            this.showFolder = false;
            this.showContent = false;
            this.showStudent = false;
        },
    };
}

// Alpine-də qeydiyyat: blade-də x-data="workspaceManager(...)" işləyə bilsin.
Alpine.data('workspaceManager', workspaceManager);
// Alpine-i işə salır. Bu entry yalnız workspace səhifəsində yüklənir və
// komponent qeydiyyatdan keçdikdən sonra başladılmalıdır.
// (start() idempotentdir — layout-dakı index.js də çağırsa zərər yoxdur.)
Alpine.start();
