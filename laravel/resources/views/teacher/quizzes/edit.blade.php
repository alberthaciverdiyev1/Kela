@extends('common.layouts.teacher')
@section('title', 'Quiz Redaktoru - Kela')
@section('content')
<div class="mx-auto max-w-4xl space-y-6" id="quiz-editor" data-content-id="{{ $contentId }}">
    <x-teacher.heading subtitle="Quiz redaktoru — sual əlavə et, düzləndir, sırala">
        {{ $quiz['title'] ?? 'Quiz Redaktoru' }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.quizzes.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Quiz form --}}
    <x-teacher.card>
        <form method="POST" action="{{ route('teacher.quizzes.update', $contentId) }}" class="grid gap-5">
            @csrf

            <x-teacher.field label="Başlıq" name="title" :required="true">
                <x-teacher.input name="title" value="{{ old('title', $quiz['title'] ?? '') }}" />
            </x-teacher.field>

            <x-teacher.field label="Təsvir" name="description">
                <x-teacher.textarea name="description" rows="3">{{ old('description', $quiz['description'] ?? '') }}</x-teacher.textarea>
            </x-teacher.field>

            <x-teacher.field label="Yayımlandı" name="is_published">
                <label class="flex items-center gap-2 pt-2">
                    <input
                        type="checkbox"
                        name="is_published"
                        value="1"
                        @checked(old('is_published', $quiz['is_published'] ?? false))
                        class="toggle toggle-primary"
                    />
                    <span class="text-sm text-base-content/70">Yayımlanmış quiz kimi işarələ</span>
                </label>
            </x-teacher.field>

            <div class="flex items-center justify-end gap-2 border-t border-base-300 pt-5">
                <x-teacher.button type="submit" icon="check">Yadda Saxla</x-teacher.button>
            </div>
        </form>
    </x-teacher.card>

    {{-- Question list --}}
    <div class="space-y-3">
        <div class="flex flex-wrap items-center justify-between gap-2">
            <h3 class="flex items-center gap-2 text-lg font-semibold text-base-content">
                Quiz Sualları
                <x-teacher.badge color="blue" id="question-count">{{ count($questions) }}</x-teacher.badge>
            </h3>
            <div class="flex flex-wrap items-center gap-2">
                <button id="add-question-btn" type="button" class="btn btn-sm btn-primary">
                    <x-icon name="heroicon-o-plus-circle" class="size-4" /> Sual Əlavə Et
                </button>
                <button id="bank-btn" type="button" class="btn btn-sm btn-ghost border border-base-300">
                    <x-icon name="heroicon-o-banknotes" class="size-4" /> Bankdan Seç
                </button>
            </div>
        </div>

        <div id="questions-list">
            @include('teacher.quizzes._questions', ['contentId' => $contentId, 'questions' => $questions])
        </div>
    </div>

    {{-- Question dialog (add / edit) --}}
    <div id="question-dialog" class="fixed inset-0 z-50 hidden items-center justify-center bg-black/40 p-4">
        <div class="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 id="question-dialog-title" class="mb-4 text-lg font-semibold text-base-content">Sual Əlavə Et</h3>
            <div class="grid gap-4">
                <x-teacher.field label="Sual" name="q_text" :required="true">
                    <x-teacher.input name="q_text" />
                </x-teacher.field>

                <div class="grid gap-4 sm:grid-cols-2">
                    <x-teacher.field label="A" name="q_option_a" :required="true">
                        <x-teacher.input name="q_option_a" />
                    </x-teacher.field>
                    <x-teacher.field label="B" name="q_option_b" :required="true">
                        <x-teacher.input name="q_option_b" />
                    </x-teacher.field>
                    <x-teacher.field label="C" name="q_option_c">
                        <x-teacher.input name="q_option_c" />
                    </x-teacher.field>
                    <x-teacher.field label="D" name="q_option_d">
                        <x-teacher.input name="q_option_d" />
                    </x-teacher.field>
                    <x-teacher.field label="E" name="q_option_e">
                        <x-teacher.input name="q_option_e" />
                    </x-teacher.field>
                    <x-teacher.field label="Doğru cavab" name="q_correct_option">
                        <select name="q_correct_option" class="select select-bordered w-full text-sm">
                            @foreach ([0 => 'A', 1 => 'B', 2 => 'C', 3 => 'D', 4 => 'E'] as $value => $letter)
                                <option value="{{ $value }}">{{ $letter }}</option>
                            @endforeach
                        </select>
                    </x-teacher.field>
                </div>

                <div class="flex items-center justify-end gap-2 border-t border-base-300 pt-4">
                    <button type="button" id="cancel-question" class="btn btn-sm btn-ghost">Ləğv et</button>
                    <button type="button" id="save-question-btn" class="btn btn-sm btn-primary">Saxla</button>
                </div>
            </div>
        </div>
    </div>

    {{-- Bank dialog --}}
    <div id="bank-dialog" class="fixed inset-0 z-50 hidden items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Bankdan Seç</h3>
            <p class="mb-3 text-sm text-base-content/60">Sual bankından mövcud sualı quizə əlavə edin.</p>
            @if (count($bankOptions) === 0)
                <p class="text-sm text-base-content/50">Bankda əlavə edilə bilən sual yoxdur.</p>
            @else
                <select name="bank_question_id" class="select select-bordered w-full text-sm">
                    <option value="">Sual seçin...</option>
                    @foreach ($bankOptions as $value => $label)
                        <option value="{{ $value }}">{{ $label }}</option>
                    @endforeach
                </select>
            @endif
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" id="cancel-bank" class="btn btn-sm btn-ghost">Ləğv et</button>
                <button type="button" id="add-from-bank-btn" class="btn btn-sm btn-primary">Əlavə Et</button>
            </div>
        </div>
    </div>
</div>
@endsection

@push('scripts')
<script>
(function () {
    const editor = document.getElementById('quiz-editor');
    if (!editor) return;

    const quizId = editor.dataset.contentId;
    const api = '/api/v1/quizzes/' + quizId;
    const fragmentUrl = '{{ route('teacher.quizzes.questions', $contentId) }}';

    const listEl = document.getElementById('questions-list');
    const countEl = document.getElementById('question-count');
    const questionDialog = document.getElementById('question-dialog');
    const bankDialog = document.getElementById('bank-dialog');

    let editingId = null;

    const val = (name) => (document.querySelector(`[name="${name}"]`) || { value: '' }).value;
    const show = (el) => el.classList.remove('hidden');
    const hide = (el) => el.classList.add('hidden');

    function resetQuestionForm() {
        ['q_text', 'q_option_a', 'q_option_b', 'q_option_c', 'q_option_d', 'q_option_e'].forEach(n => {
            const el = document.querySelector(`[name="${n}"]`);
            if (el) el.value = '';
        });
        const sel = document.querySelector('[name="q_correct_option"]');
        if (sel) sel.value = '0';
        editingId = null;
    }

    function setQuestionForm(q) {
        document.querySelector('[name="q_text"]').value = q.text || '';
        document.querySelector('[name="q_option_a"]').value = q.option_a || '';
        document.querySelector('[name="q_option_b"]').value = q.option_b || '';
        document.querySelector('[name="q_option_c"]').value = q.option_c || '';
        document.querySelector('[name="q_option_d"]').value = q.option_d || '';
        document.querySelector('[name="q_option_e"]').value = q.option_e || '';
        document.querySelector('[name="q_correct_option"]').value = String(q.correct_option ?? 0);
    }

    async function refreshQuestions() {
        try {
            const html = await KelaFragment(fragmentUrl);
            listEl.innerHTML = html;
            countEl.textContent = listEl.querySelectorAll('tr').length;
        } catch (err) {
            window.alert(err.message);
        }
    }

    async function saveQuestion() {
        const payload = {
            text: val('q_text'),
            option_a: val('q_option_a'),
            option_b: val('q_option_b'),
            option_c: val('q_option_c'),
            option_d: val('q_option_d'),
            option_e: val('q_option_e'),
            correct_option: Number(val('q_correct_option')),
        };
        if (!payload.text.trim() || !payload.option_a || !payload.option_b) {
            window.alert('Sual mətni və ən azı A, B seçimləri tələb olunur.');
            return;
        }
        try {
            if (editingId) {
                await KelaApi('PUT', '/api/v1/questions/' + editingId, payload);
            } else {
                const created = await KelaApi('POST', '/api/v1/questions', payload);
                await KelaApi('POST', api + '/questions', { question_id: created.data.id });
            }
            hide(questionDialog);
            resetQuestionForm();
            await refreshQuestions();
        } catch (err) {
            window.alert(err.message);
        }
    }

    async function addFromBank() {
        const id = val('bank_question_id');
        if (!id) { window.alert('Sual seçin.'); return; }
        try {
            await KelaApi('POST', api + '/questions', { question_id: Number(id) });
            hide(bankDialog);
            await refreshQuestions();
        } catch (err) {
            window.alert(err.message);
        }
    }

    // Sual siyahısı düymələri (delegasiya — fragment yenilənsə də işləyir).
    document.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-question-action]');
        if (!btn || !listEl.contains(btn)) return;
        e.preventDefault();

        const action = btn.dataset.questionAction;
        const id = btn.dataset.questionId;

        try {
            if (action === 'move') {
                await KelaApi('POST', `${api}/questions/${id}/move`, { direction: btn.dataset.direction });
                await refreshQuestions();
            } else if (action === 'remove') {
                if (!window.confirm(btn.dataset.confirm || 'Sual quizdən çıxarılsın?')) return;
                await KelaApi('DELETE', `${api}/questions/${id}`);
                await refreshQuestions();
            } else if (action === 'edit') {
                const q = JSON.parse(btn.dataset.question || '{}');
                editingId = Number(id);
                setQuestionForm(q);
                document.getElementById('question-dialog-title').textContent = 'Sualı Düzləndir';
                show(questionDialog);
            }
        } catch (err) {
            window.alert(err.message);
        }
    });

    document.getElementById('add-question-btn').addEventListener('click', () => {
        editingId = null;
        resetQuestionForm();
        document.getElementById('question-dialog-title').textContent = 'Sual Əlavə Et';
        show(questionDialog);
    });
    document.getElementById('bank-btn').addEventListener('click', () => {
        const sel = document.querySelector('[name="bank_question_id"]');
        if (sel) sel.value = '';
        show(bankDialog);
    });
    document.getElementById('save-question-btn').addEventListener('click', saveQuestion);
    document.getElementById('add-from-bank-btn').addEventListener('click', addFromBank);
    document.getElementById('cancel-question').addEventListener('click', () => hide(questionDialog));
    document.getElementById('cancel-bank').addEventListener('click', () => hide(bankDialog));

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') { hide(questionDialog); hide(bankDialog); }
    });
})();
</script>
@endpush
