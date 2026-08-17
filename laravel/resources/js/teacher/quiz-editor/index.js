/**
 * Quiz redaktoru — giriş nöqtəsi (Alpine komponenti).
 *
 * Quiz redaktoru YALNIZ kompozisiya alətidir: bankdan sual seç, sırala, çıxar.
 * Sual yaratma/düzləndirmə Sual Bankı modulundadır (teacher/question) — burada
 * inline sual formu YOXDUR.
 *
 *   list.js   → sual siyahısını yenilə   (LIST)
 *   add.js    → bankdan sual əlavə et     (ADD — yalnız addFromBank)
 *   edit.js   → sıralama                  (EDIT — yalnız move)
 *   delete.js → sualı quizdən çıxar       (DELETE)
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
        api: '/teacher/quizzes/' + config.quizId,
        questionCount: config.questionCount,

        // dialoq vəziyyəti (yalnız bank seçimi)
        showBank: false,
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
            this.adder = createQuestionAdder({ api: this.api });
            this.updater = createQuestionUpdater({ api: this.api });
            this.remover = createQuestionRemover({ api: this.api });
        },

        get questionsList() {
            return this.$refs.questionsList;
        },

        async refresh() {
            await this.list.refresh();
        },

        openBank() {
            this.bankQuestionId = '';
            this.showBank = true;
        },

        async addFromBank() {
            const ok = await this.adder.addFromBank(this.bankQuestionId);
            if (!ok) return;
            this.showBank = false;
            await this.refresh();
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
        },
    };
}

Alpine.data('quizEditor', quizEditor);
Alpine.start();
