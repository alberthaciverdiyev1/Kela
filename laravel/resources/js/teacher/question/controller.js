/**
 * Controller — Sual Bankı səhifəsinin giriş nöqtəsi və "orkestr"i.
 *
 * Add/edit/list/delete funksiya kodu burada deyil — hər əməliyyat öz
 * modulundadır:
 *
 *   list.js   → kataloqu yenilə        (refresh)
 *   add.js    → qovluq / sual əlavə et (addFolder/addQuestion)
 *   edit.js   → qovluq adı/silah, sual düzləndir/daşı (renameFolder/moveFolder/updateQuestion/moveQuestion)
 *   delete.js → qovluq / sual sil      (deleteFolder/deleteQuestion)
 *
 * Controller yalnız: dialoq vəziyyətini saxlayır, $refs ilə DOM-dan form
 * dəyərlərini toplayıb modullara ötürür və { ok } gəlincə kataloqu təzələyir.
 */
import Alpine from 'alpinejs';
import createContextMenu from '../context-menu';
import createBankList from './list';
import createBankAdder from './add';
import createBankEditor from './edit';
import createBankRemover from './delete';

export default function questionBank(config) {
    const list = createBankList(config.fragmentUrl);
    const adder = createBankAdder({ parentId: config.folderId ?? null });
    const editor = createBankEditor();
    const remover = createBankRemover();

    return {
        // ── Ortaq sağ-tık kontekst menyusu ────────────────────────────────
        ...createContextMenu(),

        // ── Konfiqurasiya ──────────────────────────────────────────────────
        folderId: config.folderId ?? null,
        folderTree: config.folderTree || [],

        // ── UI vəziyyəti (Alpine reaktiv dəyişənləri) ──────────────────────
        showFolderAdd: false,
        showFolderRename: false,
        showFolderMove: false,
        showQuestion: false,
        showQuestionMove: false,

        editingFolderId: null,
        editingQuestionId: null,
        questionTitle: 'Yeni Sual',
        moveTargetId: null,   // daşınacaq qovluq (folder move) üçün
        moveQuestionId: null, // daşınacaq sual (question move) üçün

        /**
         * Sual formu — reaktiv x-model obyekti. Modullar bu obyekti alır;
         * həm saxla zamanı payload qurulur, həm də canlı önizləmə buradan oxunur.
         */
        qForm: {
            text: '',
            option_a: '',
            option_b: '',
            option_c: '',
            option_d: '',
            option_e: '',
            correct_option: 0,
            explanation: '',
        },

        // ── Form sahələri ──────────────────────────────────────────────────
        // Qovluq dialoqları $refs ilə DOM-dan oxunur.
        getFolderFields() {
            return {
                folderName: this.$refs.folderNameInput,
                folderRename: this.$refs.folderRenameInput,
            };
        },

        // Sual formu reaktiv qForm-dur (x-model) — modullar onu alır.
        getQuestionFields() {
            return this.qForm;
        },

        // ── Rich text editörü (contenteditable + toolbar) ──────────────────
        init() {
            this.$nextTick(() => this.bindRichEditor());
        },

        /**
         * bindRichEditor() — contenteditable sual sahəsinə input dinləyicisini bağlar.
         * Dəyişikliklər qForm.text-ə yazılır → canlı önizləmə avtomatik yenilənir.
         */
        bindRichEditor() {
            const el = this.$refs?.qFormText;
            if (!el || el.__kelaRichBound) return;
            el.__kelaRichBound = true;
            el.addEventListener('input', () => {
                this.qForm.text = el.innerHTML;
            });
        },

        /**
         * setEditorContent() — qForm.text dəyərini editör DOM-a yazır.
         * Dialog açıldıqda form təmizlənir/doldurulur; DOM da sinxron edilməlidir.
         */
        setEditorContent() {
            const el = this.$refs?.qFormText;
            if (el && el.innerHTML !== this.qForm.text) {
                el.innerHTML = this.qForm.text;
            }
        },

        /**
         * execRich(cmd, value) — toolbar düymələri üçün dokument komandası.
         * Sual sahəsində formatlama tətbiq edir, sonra qForm.text-i yeniləyir.
         */
        execRich(cmd, value = null) {
            const el = this.$refs?.qFormText;
            if (!el) return;
            el.focus();
            document.execCommand(cmd, false, value);
            el.dispatchEvent(new Event('input'));
        },

        // ── Canlı önizləmə ─────────────────────────────────────────────────
        previewOptions() {
            const f = this.qForm;
            const keys = ['option_a', 'option_b', 'option_c', 'option_d', 'option_e'];
            const correct = Number(f.correct_option);
            const options = [];
            for (let i = 0; i < 5; i++) {
                const value = String(f[keys[i]] ?? '').trim();
                if (value) {
                    options.push({
                        letter: String.fromCharCode(65 + i),
                        text: value,
                        isCorrect: i === correct,
                    });
                }
            }
            return options;
        },

        /**
         * refresh() — Kataloqu yenidən çəkir (list modulu).
         */
        async refresh() {
            await list.refresh(this.$refs.directory);
        },

        // ── Qovluq əməliyyatları ───────────────────────────────────────────

        openFolderAdd() {
            this.showFolderAdd = true;
            adder.openFolder(this.getFolderFields());
        },

        async saveFolder() {
            const ok = await adder.addFolder(this.getFolderFields());
            if (!ok) return;
            this.showFolderAdd = false;
            await this.refresh();
        },

        openFolderRename(folderId, folderName = '') {
            this.editingFolderId = Number(folderId);
            this.showFolderRename = true;
            this.$nextTick(() => {
                editor.openFolderRename(this.getFolderFields(), { name: folderName || '' });
                this.$refs.folderRenameInput?.focus();
            });
        },

        async saveFolderRename() {
            const ok = await editor.renameFolder(this.getFolderFields(), this.editingFolderId);
            if (!ok) return;
            this.showFolderRename = false;
            await this.refresh();
        },

        openFolderMove(folderId) {
            this.moveTargetId = Number(folderId);
            this.showFolderMove = true;
        },

        async saveFolderMove() {
            const selected = this.$refs.folderMoveSelect.value;
            const ok = await editor.moveFolder(this.moveTargetId, selected ? Number(selected) : null);
            if (!ok) return;
            this.showFolderMove = false;
            await this.refresh();
        },

        async handleFolderDelete(folderId, folderName = 'Qovluq') {
            const ok = await remover.deleteFolder(folderId, folderName || 'Qovluq');
            if (ok) await this.refresh();
        },

        // ── Sual əməliyyatları ─────────────────────────────────────────────

        openQuestionAdd() {
            this.editingQuestionId = null;
            this.questionTitle = 'Yeni Sual';
            this.showQuestion = true;
            adder.openQuestion(this.getQuestionFields());
            this.$nextTick(() => {
                this.setEditorContent();
                this.$refs?.qFormText?.focus();
            });
        },

        openQuestionEdit(questionId, questionData) {
            const q = (typeof questionData === 'string' ? JSON.parse(questionData || '{}') : (questionData || {}));
            this.editingQuestionId = Number(questionId);
            this.questionTitle = 'Sualı Düzləndir';
            this.showQuestion = true;
            editor.openQuestionEdit(this.getQuestionFields(), q);
            this.$nextTick(() => {
                this.setEditorContent();
                this.$refs?.qFormText?.focus();
            });
        },

        async saveQuestion() {
            const form = this.getQuestionFields();
            const ok = this.editingQuestionId
                ? await editor.updateQuestion(form, this.editingQuestionId)
                : await adder.addQuestion(form, this.folderId);
            if (!ok) return;
            this.showQuestion = false;
            await this.refresh();
        },

        openQuestionMove(questionId) {
            this.moveQuestionId = Number(questionId);
            this.showQuestionMove = true;
        },

        async saveQuestionMove() {
            const selected = this.$refs.questionMoveSelect.value;
            const ok = await editor.moveQuestion(this.moveQuestionId, selected ? Number(selected) : null);
            if (!ok) return;
            this.showQuestionMove = false;
            await this.refresh();
        },

        async handleQuestionDelete(questionId, questionText) {
            const ok = await remover.deleteQuestion(questionId, questionText);
            if (ok) await this.refresh();
        },

        // ── Sağ-tık kontekst menyusu ───────────────────────────────────────

        /**
         * onTableContextMenu(e) — Kataloq üzərində sağ-tık (delegasiya).
         * Kataloq hər refresh-də yenidən render olunduğundan, hadisə kökə
         * bağlanır və [data-kind] sətrinə görə menyu açılır.
         */
        onTableContextMenu(e) {
            const row = e.target.closest('[data-kind]');
            if (row) {
                e.preventDefault();
                this.openRowContextMenu(e, row.dataset.kind, row);
            }
        },

        /** Sətirə sağ-tık — data-* atributlarından menyu qurur. */
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

            if (kind === 'question') {
                this.openCtxMenu(e, this.questionPreviewTitle(d.questionText), [
                    { icon: 'pencil-square', iconClass: 'bg-amber-500/10 text-amber-600', label: 'Sualı düzləndir', cls: 'text-base-content hover:bg-amber-50 hover:text-amber-600', action: () => this.openQuestionEdit(d.questionId, d.question) },
                    { icon: 'arrows-right-left', iconClass: 'bg-info/10 text-info', label: 'Qovluğa daşı', cls: 'text-base-content hover:bg-info/10 hover:text-info', action: () => this.openQuestionMove(d.questionId) },
                    { divider: true },
                    { icon: 'trash', iconClass: 'bg-error/10 text-error', label: 'Sil', danger: true, cls: 'text-error hover:bg-error/10 hover:text-error', action: () => this.handleQuestionDelete(d.questionId, d.questionText) },
                ]);
            }
        },

        /** Menyu başlığı üçün sual mətnindən qısa önizləmə. */
        questionPreviewTitle(text) {
            const clean = String(text || '').replace(/<[^>]*>/g, '').replace(/\s+/g, ' ').trim();
            return clean.length > 40 ? clean.slice(0, 40) + '…' : (clean || 'Sual');
        },

        /**
         * closeAll() — Escape düyməsi basılanda bütün dialoqları bağlayır.
         */
        closeAll() {
            this.closeCtxMenu();
            this.showFolderAdd = false;
            this.showFolderRename = false;
            this.showFolderMove = false;
            this.showQuestion = false;
            this.showQuestionMove = false;
        },
    };
}

// Alpine-də qeydiyyat: blade-də x-data="questionBank(...)" işləyə bilsin.
Alpine.data('questionBank', questionBank);
// Alpine-i işə salır. Bu entry yalnız sual bankı səhifəsində yüklənir.
Alpine.start();
