/**
 * Quiz redaktoru — giriş nöqtəsi (Alpine komponenti).
 *
 * CRUD funksiyaları ayrı modullarda saxlanılır:
 *   list.js   → sual siyahısını yenilə (LIST)
 *   add.js    → yeni sual / bankdan əlavə et (ADD)
 *   edit.js   → sualı düzləndir / sırala (EDIT)
 *   delete.js → sualı quizdən çıxar (DELETE)
 */
import Alpine from 'alpinejs';
import createQuestionList from './list';
import createQuestionAdder from './add';
import createQuestionUpdater from './edit';
import createQuestionRemover from './delete';

export default function quizEditor(config) {
    return {
        quizId: config.quizId,
        fragmentUrl: config.fragmentUrl,
        api: '/api/v1/quizzes/' + config.quizId,
        questionCount: config.questionCount,

        // dialoq / form vəziyyəti
        showQuestion: false,
        showBank: false,
        questionTitle: 'Sual Əlavə Et',
        editingId: null,

        qText: '',
        qOptionA: '',
        qOptionB: '',
        qOptionC: '',
        qOptionD: '',
        qOptionE: '',
        qCorrectOption: '0',
        bankQuestionId: '',

        // CRUD modulları (init-də qurulur)
        list: null,
        adder: null,
        updater: null,
        remover: null,

        init() {
            this.list = createQuestionList({
                fragmentUrl: config.fragmentUrl,
                getListEl: () => this.$refs.questionsList,
                setCount: (n) => { this.questionCount = n; },
            });
            this.adder = createQuestionAdder({
                api: this.api,
                getPayload: () => this.questionPayload(),
            });
            this.updater = createQuestionUpdater({
                api: this.api,
                getPayload: () => this.questionPayload(),
            });
            this.remover = createQuestionRemover({ api: this.api });
        },

        questionPayload() {
            return {
                text: this.qText,
                option_a: this.qOptionA,
                option_b: this.qOptionB,
                option_c: this.qOptionC,
                option_d: this.qOptionD,
                option_e: this.qOptionE,
                correct_option: Number(this.qCorrectOption),
            };
        },

        get questionsList() {
            return this.$refs.questionsList;
        },

        async refresh() {
            await this.list.refresh();
        },

        openAdd() {
            this.editingId = null;
            this.questionTitle = 'Sual Əlavə Et';
            this.resetForm();
            this.showQuestion = true;
        },

        openEdit(btn) {
            const q = JSON.parse(btn.dataset.question || '{}');
            this.editingId = Number(btn.dataset.questionId);
            this.questionTitle = 'Sualı Düzləndir';
            this.qText = q.text || '';
            this.qOptionA = q.option_a || '';
            this.qOptionB = q.option_b || '';
            this.qOptionC = q.option_c || '';
            this.qOptionD = q.option_d || '';
            this.qOptionE = q.option_e || '';
            this.qCorrectOption = String(q.correct_option ?? 0);
            this.showQuestion = true;
        },

        resetForm() {
            this.qText = '';
            this.qOptionA = '';
            this.qOptionB = '';
            this.qOptionC = '';
            this.qOptionD = '';
            this.qOptionE = '';
            this.qCorrectOption = '0';
        },

        async saveQuestion() {
            const ok = this.editingId
                ? await this.updater.update(this.editingId)
                : await this.adder.add();
            if (!ok) return;
            this.showQuestion = false;
            this.resetForm();
            await this.refresh();
        },

        async addFromBank() {
            const ok = await this.adder.addFromBank(this.bankQuestionId);
            if (!ok) return;
            this.showBank = false;
            await this.refresh();
        },

        openBank() {
            this.bankQuestionId = '';
            this.showBank = true;
        },

        async move(id, direction) {
            const ok = await this.updater.move(id, direction);
            if (ok) await this.refresh();
        },

        async remove(id) {
            const ok = await this.remover.remove(id);
            if (ok) await this.refresh();
        },

        // Fragment-dakı sual düymələri (delegasiya — fragment yenilənsə də işləyir).
        onListClick(e) {
            const btn = e.target.closest('[data-question-action]');
            if (!btn || !this.questionsList.contains(btn)) return;
            const action = btn.dataset.questionAction;
            const id = btn.dataset.questionId;
            if (action === 'move') this.move(id, btn.dataset.direction);
            else if (action === 'remove') this.remove(id);
            else if (action === 'edit') this.openEdit(btn);
        },
    };
}

Alpine.data('quizEditor', quizEditor);
Alpine.start();
