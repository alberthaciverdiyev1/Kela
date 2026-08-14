/**
 * Ev tapşırığı redaktoru — sual kompozisiyası (Alpine).
 *
 * Sual mənbələri:
 *   QUIZ tipi  — "Quizdən əlavə et" pəncərəsi ilə müəllimin öz quizlərindən
 *                seçilən variantlı suallar (anlıq görüntü kimi saxlanılır).
 *   TASK tipi  — "Əl ilə tapşırıq yaz" pəncərəsi ilə variantsız tapşırıq sualı.
 *
 * Suallar x-model ilə reaktiv questions massivində saxlanılır; form POST-da
 * questions_json gizli sahəsi ilə servisə ötürülür.
 */
import Alpine from 'alpinejs';

export default function homeworkEditor(config) {
    const normalize = (q) => ({
        type: Number(q.type ?? 0),
        text: q.text ?? '',
        option_a: q.option_a ?? q.options?.A ?? '',
        option_b: q.option_b ?? q.options?.B ?? '',
        option_c: q.option_c ?? q.options?.C ?? '',
        option_d: q.option_d ?? q.options?.D ?? '',
        option_e: q.option_e ?? q.options?.E ?? '',
        correct_option: q.correct_option ?? null,
        source_question_id: q.source_question_id ?? null,
        source_quiz_id: q.source_quiz_id ?? null,
    });

    return {
        questions: (config.questions ?? []).map(normalize),

        // Pəncərə vəziyyətləri
        showQuizPicker: false,
        showTaskModal: false,

        // Quiz seçimi pəncərəsi
        quizzes: [],
        quizzesLoading: false,
        quizSearch: '',
        selectedQuizId: '',
        quizQuestions: [],
        quizQuestionsLoading: false,
        checked: {},

        // Əl ilə tapşırıq pəncərəsi
        taskText: '',

        // ── Hesablanmış dəyərlər ─────────────────────────────────────────
        get questionsJson() {
            return JSON.stringify(this.questions);
        },

        get taskCount() {
            return this.questions.filter((q) => q.type === 0).length;
        },

        get quizCount() {
            return this.questions.filter((q) => q.type === 1).length;
        },

        // ── İlkin yükləmə ────────────────────────────────────────────────
        init() {
            this.loadQuizzes();
        },

        async loadQuizzes() {
            this.quizzesLoading = true;
            try {
                const res = await KelaApi('GET', '/api/v1/quiz-folders/picker');
                this.quizzes = (res?.quizzes ?? []).map((q) => ({
                    id: Number(q.content_id),
                    title: q.title ?? 'Adsız quiz',
                    questions_count: Number(q.questions_count ?? 0),
                    folder_id: q.folder_id ? Number(q.folder_id) : null,
                    folder_path: q.folder_path ?? [],
                    folder_path_ids: q.folder_path_ids ?? [],
                }));
            } catch (err) {
                window.alert(err.message);
            } finally {
                this.quizzesLoading = false;
            }
        },

        // ── Quiz seçimi ──────────────────────────────────────────────────
        openQuizPicker() {
            this.selectedQuizId = '';
            this.quizQuestions = [];
            this.checked = {};
            this.showQuizPicker = true;
        },

        async selectQuiz() {
            if (!this.selectedQuizId) {
                this.quizQuestions = [];
                return;
            }
            this.quizQuestionsLoading = true;
            this.quizQuestions = [];
            this.checked = {};
            try {
                const res = await KelaApi('GET', `/teacher/homeworks/quiz-questions/${this.selectedQuizId}`);
                this.quizQuestions = res?.questions ?? [];
            } catch (err) {
                window.alert(err.message);
            } finally {
                this.quizQuestionsLoading = false;
            }
        },

        // ── Quiz qovluq ağacı (folder_path üzrə) ─────────────────────────
        /** quizzes → qovluq ağacı (ic-ice qovluqlar altında qruplaşmış). */
        get quizTree() {
            const root = { key: '', name: '', depth: -1, children: [], contents: [] };
            const nodes = { '': root };

            for (const quiz of this.quizzes) {
                const names = quiz.folder_path || [];
                let parent = root;
                let acc = [];
                for (let i = 0; i < names.length; i++) {
                    acc.push(names[i]);
                    const key = acc.join(' › ');
                    if (!nodes[key]) {
                        nodes[key] = { key, name: names[i], depth: i, children: [], contents: [] };
                        parent.children.push(nodes[key]);
                    }
                    parent = nodes[key];
                }
                parent.contents.push(quiz);
            }

            return root;
        },

        quizMatches(quiz) {
            const s = this.quizSearch.trim().toLowerCase();
            if (!s) return true;
            const haystack = [quiz.title || '', ...(quiz.folder_path || [])].join(' / ').toLowerCase();
            return haystack.includes(s);
        },

        /** Ağacı düz satırlara çevirir: qovluq başlıqları + içindəki quizlər. */
        get quizRows() {
            const filtering = this.quizSearch.trim() !== '';
            const rows = [];

            const visibleCounts = {};
            const countVisible = (node) => {
                let n = node.contents.filter((q) => this.quizMatches(q)).length;
                for (const child of node.children) n += countVisible(child);
                visibleCounts[node.key] = n;
                return n;
            };
            countVisible(this.quizTree);

            const walk = (node) => {
                if (node.depth >= 0) {
                    const vis = visibleCounts[node.key];
                    if (vis === 0) return; // məzmunu olmayan qovluğu göstərmə
                    rows.push({ kind: 'folder', key: 'f:' + node.key, name: node.name, depth: node.depth, count: vis });
                }
                for (const q of node.contents) {
                    if (filtering && !this.quizMatches(q)) continue;
                    rows.push({ kind: 'quiz', key: 'q:' + q.id, q, depth: node.depth + 1 });
                }
                for (const child of node.children) walk(child);
            };
            walk(this.quizTree);

            return rows;
        },

        get quizRowsCount() {
            return this.quizRows.filter((r) => r.kind === 'quiz').length;
        },

        /** Ağacdan quiz seçildi → sualları yüklə. */
        selectQuizRow(quiz) {
            this.selectedQuizId = String(quiz.id);
            this.selectQuiz();
        },

        toggleCheck(questionId) {
            this.checked[questionId] = !this.checked[questionId];
        },

        get checkedCount() {
            return Object.values(this.checked).filter(Boolean).length;
        },

        addSelectedFromQuiz() {
            const selected = this.quizQuestions.filter((q) => this.checked[q.question_id]);
            if (selected.length === 0) {
                window.alert('Heç bir sual seçilməyib.');
                return;
            }
            for (const q of selected) {
                this.questions.push({
                    type: 1,
                    text: q.text ?? '',
                    option_a: q.options?.A ?? '',
                    option_b: q.options?.B ?? '',
                    option_c: q.options?.C ?? '',
                    option_d: q.options?.D ?? '',
                    option_e: q.options?.E ?? '',
                    correct_option: q.correct_option ?? null,
                    source_question_id: q.question_id,
                    source_quiz_id: Number(this.selectedQuizId),
                });
            }
            this.showQuizPicker = false;
            this.quizQuestions = [];
            this.checked = {};
            this.selectedQuizId = '';
        },

        // ── Əl ilə tapşırıq ──────────────────────────────────────────────
        addTask() {
            const text = String(this.taskText || '').trim();
            if (!text) {
                window.alert('Tapşırıq mətni boş ola bilməz.');
                return;
            }
            this.questions.push({ type: 0, text });
            this.taskText = '';
            this.showTaskModal = false;
        },

        // ── Siyahı əməliyyatları ─────────────────────────────────────────
        move(idx, direction) {
            const to = idx + direction;
            if (to < 0 || to >= this.questions.length) return;
            const arr = [...this.questions];
            [arr[idx], arr[to]] = [arr[to], arr[idx]];
            this.questions = arr;
        },

        remove(idx) {
            this.questions = this.questions.filter((_, i) => i !== idx);
        },

        /** Bir sualın variantlarını (A–E) göstərir — yalnız quiz sualları üçün. */
        itemOptions(q) {
            const letters = ['A', 'B', 'C', 'D', 'E'];
            const out = [];
            for (const letter of letters) {
                const value = q['option_' + letter.toLowerCase()];
                if (value) out.push({ letter, text: value });
            }
            return out;
        },

        /** Doğru cavab hərfi (0-indeksli) — yalnız quiz sualları üçün. */
        correctLetter(q) {
            const idx = Number(q.correct_option);
            return Number.isInteger(idx) && idx >= 0 && idx <= 4 ? String.fromCharCode(65 + idx) : null;
        },
    };
}

// Alpine-də qeydiyyat: blade-də x-data="homeworkEditor(...)" işləyə bilsin.
Alpine.data('homeworkEditor', homeworkEditor);
Alpine.start();
