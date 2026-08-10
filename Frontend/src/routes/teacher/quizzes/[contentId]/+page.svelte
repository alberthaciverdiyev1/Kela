<script>
    import { page } from '$app/stores';
    import { api } from '$lib/api.js';
    import { t } from '$lib/i18n.js';
    import { notify } from '$lib/notify.js';
    import { user } from '$lib/auth.js';
    import AppBtn from '$lib/components/AppBtn.svelte';
    import AppIcon from '$lib/components/AppIcon.svelte';
    import AppModal from '$lib/components/AppModal.svelte';

    const LETTERS = ['A', 'B', 'C', 'D', 'E'];
    const contentId = Number($page.params.contentId);

    let quiz = $state(null);
    let bank = $state([]);

    let addOpen = $state(false);
    let addQuery = $state('');
    let addChecked = $state(new Set());
    let addBusy = $state(false);

    let newOpen = $state(false);
    let newBusy = $state(false);
    let text = $state('');
    let optionA = $state('');
    let optionB = $state('');
    let optionC = $state('');
    let optionD = $state('');
    let optionE = $state('');
    let correctOption = $state(0);

    function optionList(q) {
        const opts = [];
        [q.optionA, q.optionB, q.optionC, q.optionD, q.optionE].forEach((v, i) => {
            if (v) opts.push({ label: LETTERS[i] + '. ' + v, index: i });
        });
        return opts;
    }

    async function load() {
        try {
            quiz = await api('/quizzes/' + contentId);
        } catch (e) {
            notify.error(e.message);
        }
    }

    function existingIds() {
        return quiz ? quiz.questions.map((x) => x.question.id) : [];
    }

    const addRows = $derived(() => {
        const q = addQuery.trim().toLowerCase();
        const existing = existingIds();
        return bank.filter((b) => {
            if (existing.includes(b.id)) return false;
            return !q || b.text.toLowerCase().includes(q);
        });
    });

    function toggleCheck(id) {
        const next = new Set(addChecked);
        if (next.has(id)) next.delete(id);
        else next.add(id);
        addChecked = next;
    }

    async function openAdd() {
        addQuery = '';
        addChecked = new Set();
        try {
            bank = (await api('/questions?teacherId=' + $user.id)) || [];
            addOpen = true;
        } catch (e) {
            notify.error(e.message);
        }
    }

    async function confirmAdd() {
        const ids = Array.from(addChecked);
        if (!ids.length) return;
        addBusy = true;
        try {
            await api('/quizzes/' + contentId + '/questions', {
                method: 'POST',
                body: JSON.stringify({ questionIds: ids })
            });
            addOpen = false;
            await load();
            notify.success($t('quiz.questionAdded'));
        } catch (e) {
            notify.error(e.errors && e.errors.length ? e.errors[0] : e.message);
        } finally {
            addBusy = false;
        }
    }

    function resetNewForm() {
        text = '';
        optionA = '';
        optionB = '';
        optionC = '';
        optionD = '';
        optionE = '';
        correctOption = 0;
    }

    async function createNewQuestion(e) {
        e.preventDefault();
        newBusy = true;
        const payload = {
            teacherId: $user.id,
            text: text.trim(),
            optionA: optionA.trim(),
            optionB: optionB.trim(),
            optionC: optionC.trim(),
            optionD: optionD.trim() || null,
            optionE: optionE.trim() || null,
            correctOption: Number(correctOption)
        };
        try {
            const created = await api('/questions', { method: 'POST', body: JSON.stringify(payload) });
            await api('/quizzes/' + contentId + '/questions', {
                method: 'POST',
                body: JSON.stringify({ questionIds: [created] })
            });
            newOpen = false;
            await load();
            notify.success($t('quiz.created'));
        } catch (err) {
            notify.error(err.errors && err.errors.length ? err.errors[0] : err.message);
        } finally {
            newBusy = false;
        }
    }

    async function removeQuestion(q) {
        if (!confirm($t('quiz.removeConfirm', { text: q.text }))) return;
        try {
            await api('/quizzes/' + contentId + '/questions/' + q.id, { method: 'DELETE' });
            await load();
            notify.success($t('quiz.questionRemoved'));
        } catch (e) {
            notify.error(e.message);
        }
    }

    load();
</script>

<div class="flex flex-col gap-6">
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
            <h1 class="text-2xl font-semibold tracking-tight">{quiz ? quiz.title : ''}</h1>
            <p class="text-sm text-base-content/50">{$t('quiz.editorSubtitle')}</p>
        </div>
        <div class="flex gap-2">
            <AppBtn variant="secondary" icon="arrow-left" href="/teacher/library">{$t('common.back')}</AppBtn>
            <AppBtn variant="secondary" icon="plus" onclick={() => (resetNewForm(), (newOpen = true))}>{$t('quiz.newQuestion')}</AppBtn>
            <AppBtn variant="primary" icon="copy" onclick={openAdd}>{$t('quiz.addQuestion')}</AppBtn>
        </div>
    </div>

    {#if quiz && quiz.questions.length}
        <div class="card overflow-hidden border border-base-200 bg-base-100">
            <div class="overflow-x-auto">
                <table class="table">
                    <thead>
                        <tr>
                            <th class="w-10 bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">#</th>
                            <th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">{$t('questions.text')}</th>
                            <th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">{$t('questions.options')}</th>
                            <th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">{$t('questions.correct')}</th>
                            <th class="w-1 whitespace-nowrap bg-base-200/50 text-right"></th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each quiz.questions as entry, idx}
                            <tr>
                                <td class="text-base-content/40">{idx + 1}</td>
                                <td class="max-w-md"><p class="font-medium">{entry.question.text}</p></td>
                                <td>
                                    <div class="flex flex-wrap gap-1">
                                        {#each optionList(entry.question) as o}
                                            <span class:badge-success={o.index === entry.question.correctOption} class:badge-ghost={o.index !== entry.question.correctOption} class="badge badge-sm">{o.label}</span>
                                        {/each}
                                    </div>
                                </td>
                                <td><span class="badge badge-primary badge-sm">{LETTERS[entry.question.correctOption] || ''}</span></td>
                                <td class="w-1 whitespace-nowrap text-right">
                                    <button type="button" class="btn btn-sm btn-square btn-ghost btn-error" title={$t('quiz.remove')} onclick={() => removeQuestion(entry.question)}>
                                        <AppIcon name="trash" class="h-4 w-4" />
                                    </button>
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
        </div>
    {:else}
        <div class="card flex items-center justify-center gap-2 border border-base-200 bg-base-100 py-10 text-center">
            <span class="text-base-300"><AppIcon name="quiz" class="h-10 w-10" /></span>
            <p class="text-sm text-base-content/50">{$t('quiz.noQuestions')}</p>
        </div>
    {/if}

    <AppModal bind:open={addOpen} title={$t('quiz.addQuestion')}>
        <div class="flex flex-col gap-4">
            <label class="input flex items-center gap-2">
                <AppIcon name="search" class="h-4 w-4 opacity-40" />
                <input type="search" class="grow" placeholder={$t('nav.search')} autocomplete="off" bind:value={addQuery} />
            </label>
            {#if addRows.length}
                <ul class="menu menu-sm max-h-80 overflow-auto rounded-box border border-base-200 bg-base-100 p-0">
                    {#each addRows as b}
                        <li>
                            <label class="flex items-center gap-3 rounded-lg px-2 py-2">
                                <input type="checkbox" class="checkbox checkbox-sm" checked={addChecked.has(b.id)} onchange={() => toggleCheck(b.id)} />
                                <span class="flex-1 truncate text-sm">{b.text}</span>
                            </label>
                        </li>
                    {/each}
                </ul>
            {:else}
                <p class="p-3 text-sm text-base-content/50">{$t('quiz.noMoreQuestions')}</p>
            {/if}
            <div class="flex justify-end gap-2">
                <AppBtn variant="secondary" onclick={() => (addOpen = false)}>{$t('common.cancel')}</AppBtn>
                <AppBtn variant="primary" onclick={confirmAdd} loading={addBusy}>{$t('quiz.addSelected')}</AppBtn>
            </div>
        </div>
    </AppModal>

    <AppModal bind:open={newOpen} title={$t('quiz.newQuestion')}>
        <form class="flex flex-col gap-4" onsubmit={createNewQuestion} novalidate>
            <div class="field">
                <label class="block text-sm font-semibold mb-1.5 text-base-content" for="qn-text">{$t('questions.text')}</label>
                <textarea id="qn-text" class="textarea w-full" rows="3" bind:value={text} required></textarea>
            </div>
            <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="qn-a">A. {$t('questions.option')}</label>
                    <input id="qn-a" class="input w-full" type="text" autocomplete="off" bind:value={optionA} required />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="qn-b">B. {$t('questions.option')}</label>
                    <input id="qn-b" class="input w-full" type="text" autocomplete="off" bind:value={optionB} required />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="qn-c">C. {$t('questions.option')}</label>
                    <input id="qn-c" class="input w-full" type="text" autocomplete="off" bind:value={optionC} required />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="qn-d">D. {$t('questions.optionOptional')}</label>
                    <input id="qn-d" class="input w-full" type="text" autocomplete="off" bind:value={optionD} />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="qn-e">E. {$t('questions.optionOptional')}</label>
                    <input id="qn-e" class="input w-full" type="text" autocomplete="off" bind:value={optionE} />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="qn-correct">{$t('questions.correct')}</label>
                    <select id="qn-correct" class="select w-full" bind:value={correctOption}>
                        <option value="0">A</option>
                        <option value="1">B</option>
                        <option value="2">C</option>
                        <option value="3">D</option>
                        <option value="4">E</option>
                    </select>
                </div>
            </div>
            <div class="flex justify-end gap-2">
                <AppBtn variant="secondary" onclick={() => (newOpen = false)}>{$t('common.cancel')}</AppBtn>
                <AppBtn variant="primary" type="submit" loading={newBusy}>{$t('common.save')}</AppBtn>
            </div>
        </form>
    </AppModal>
</div>
