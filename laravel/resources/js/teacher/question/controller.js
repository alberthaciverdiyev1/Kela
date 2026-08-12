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

        // ── Form sahələri ($refs ilə DOM-dan) ──────────────────────────────
        getFolderFields() {
            return {
                folderName: this.$refs.folderNameInput,
                folderRename: this.$refs.folderRenameInput,
            };
        },

        getQuestionFields() {
            return {
                qText: this.$refs.qText,
                qOptionA: this.$refs.qOptionA,
                qOptionB: this.$refs.qOptionB,
                qOptionC: this.$refs.qOptionC,
                qOptionD: this.$refs.qOptionD,
                qOptionE: this.$refs.qOptionE,
                qCorrectOption: this.$refs.qCorrectOption,
            };
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

        openFolderRename(btn) {
            this.editingFolderId = Number(btn.dataset.folderId);
            this.showFolderRename = true;
            this.$nextTick(() => {
                editor.openFolderRename(this.getFolderFields(), {
                    name: btn.dataset.folderName || '',
                });
                this.$refs.folderRenameInput?.focus();
            });
        },

        async saveFolderRename() {
            const ok = await editor.renameFolder(this.getFolderFields(), this.editingFolderId);
            if (!ok) return;
            this.showFolderRename = false;
            await this.refresh();
        },

        openFolderMove(btn) {
            this.moveTargetId = Number(btn.dataset.folderId);
            this.showFolderMove = true;
        },

        async saveFolderMove() {
            const selected = this.$refs.folderMoveSelect.value;
            const ok = await editor.moveFolder(this.moveTargetId, selected ? Number(selected) : null);
            if (!ok) return;
            this.showFolderMove = false;
            await this.refresh();
        },

        async handleFolderDelete(btn) {
            const ok = await remover.deleteFolder(btn.dataset.folderId, btn.dataset.folderName);
            if (ok) await this.refresh();
        },

        // ── Sual əməliyyatları ─────────────────────────────────────────────

        openQuestionAdd() {
            this.editingQuestionId = null;
            this.questionTitle = 'Yeni Sual';
            this.showQuestion = true;
            this.$nextTick(() => adder.openQuestion(this.getQuestionFields()));
        },

        openQuestionEdit(btn) {
            const q = JSON.parse(btn.dataset.question || '{}');
            this.editingQuestionId = Number(btn.dataset.questionId);
            this.questionTitle = 'Sualı Düzləndir';
            this.showQuestion = true;
            this.$nextTick(() => editor.openQuestionEdit(this.getQuestionFields(), q));
        },

        async saveQuestion() {
            const fields = this.getQuestionFields();
            const ok = this.editingQuestionId
                ? await editor.updateQuestion(fields, this.editingQuestionId)
                : await adder.addQuestion(fields, this.folderId);
            if (!ok) return;
            this.showQuestion = false;
            await this.refresh();
        },

        openQuestionMove(btn) {
            this.moveQuestionId = Number(btn.dataset.questionId);
            this.showQuestionMove = true;
        },

        async saveQuestionMove() {
            const selected = this.$refs.questionMoveSelect.value;
            const ok = await editor.moveQuestion(this.moveQuestionId, selected ? Number(selected) : null);
            if (!ok) return;
            this.showQuestionMove = false;
            await this.refresh();
        },

        async handleQuestionDelete(btn) {
            const ok = await remover.deleteQuestion(btn.dataset.questionId, btn.dataset.questionText);
            if (ok) await this.refresh();
        },

        // ── Delegasiya ─────────────────────────────────────────────────────

        /**
         * onTableClick(e) — Kataloq üzərində click hadisəsi.
         *
         * Niyə delegasiya: kataloq hər refresh-də yenidən render olunur —
         * bir dəfə kökə @click bağlanır və data-folder-action /
         * data-question-action düymələri müvafiq əməliyyata ötürülür.
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
            const qBtn = e.target.closest('[data-question-action]');
            if (qBtn) {
                const action = qBtn.dataset.questionAction;
                if (action === 'edit') this.openQuestionEdit(qBtn);
                else if (action === 'move') this.openQuestionMove(qBtn);
                else if (action === 'delete') this.handleQuestionDelete(qBtn);
            }
        },

        /**
         * closeAll() — Escape düyməsi basılanda bütün dialoqları bağlayır.
         */
        closeAll() {
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
