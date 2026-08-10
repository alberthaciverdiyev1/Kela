<script>
    import { api } from '$lib/api.js';
    import { t } from '$lib/i18n.js';
    import { notify } from '$lib/notify.js';
    import { user } from '$lib/auth.js';
    import AppBtn from '$lib/components/AppBtn.svelte';
    import AppIcon from '$lib/components/AppIcon.svelte';
    import AppModal from '$lib/components/AppModal.svelte';

    const LETTERS = ['A', 'B', 'C', 'D', 'E'];

    let items = $state([]);
    let query = $state('');
    let modalOpen = $state(false);
    let editingId = $state(null);
    let busy = $state(false);

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

    const filtered = $derived(() => {
        const q = query.trim().toLowerCase();
        if (!q) return items;
        return items.filter((it) => {
            const hay = it.text + ' ' + optionList(it).map((o) => o.label).join(' ');
            return hay.toLowerCase().includes(q);
        });
    });

    async function load() {
        try {
            items = (await api('/questions?teacherId=' + $user.id)) || [];
        } catch (e) {
            notify.error(e.message);
        }
    }

    function resetForm() {
        editingId = null;
        text = '';
        optionA = '';
        optionB = '';
        optionC = '';
        optionD = '';
        optionE = '';
        correctOption = 0;
    }

    function openCreate() {
        resetForm();
        modalOpen = true;
    }

    function openEdit(q) {
        editingId = q.id;
        text = q.text;
        optionA = q.optionA || '';
        optionB = q.optionB || '';
        optionC = q.optionC || '';
        optionD = q.optionD || '';
        optionE = q.optionE || '';
        correctOption = q.correctOption;
        modalOpen = true;
    }

    async function save(e) {
        e.preventDefault();
        busy = true;
        const payload = {
            text: text.trim(),
            optionA: optionA.trim(),
            optionB: optionB.trim(),
            optionC: optionC.trim(),
            optionD: optionD.trim() || null,
            optionE: optionE.trim() || null,
            correctOption: Number(correctOption)
        };
        try {
            if (editingId) {
                await api('/questions/' + editingId, { method: 'PUT', body: JSON.stringify(payload) });
            } else {
                await api('/questions', { method: 'POST', body: JSON.stringify({ ...payload, teacherId: $user.id }) });
            }
            modalOpen = false;
            await load();
            notify.success(editingId ? $t('questions.updated') : $t('questions.created'));
        } catch (err) {
            notify.error(err.errors && err.errors.length ? err.errors[0] : err.message);
        } finally {
            busy = false;
        }
    }

    async function remove(q) {
        if (!confirm($t('questions.deleteConfirm', { text: q.text }))) return;
        try {
            await api('/questions/' + q.id, { method: 'DELETE' });
            await load();
            notify.success($t('questions.deleted'));
        } catch (err) {
            notify.error(err.message);
        }
    }

    load();
</script>

<div class="flex flex-col gap-6">
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
            <h1 class="text-2xl font-semibold tracking-tight">{$t('questions.title')}</h1>
            <p class="text-sm text-base-content/50">{$t('questions.subtitle')}</p>
        </div>
        <AppBtn variant="primary" icon="plus" onclick={openCreate}>{$t('questions.new')}</AppBtn>
    </div>

    <div>
        <label class="input flex w-full max-w-sm items-center gap-2">
            <AppIcon name="search" class="h-4 w-4 opacity-40" />
            <input type="search" class="grow" placeholder={$t('nav.search')} autocomplete="off" bind:value={query} />
        </label>
    </div>

    {#if filtered.length}
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
                        {#each filtered as q, idx}
                            <tr>
                                <td class="text-base-content/40">{idx + 1}</td>
                                <td class="max-w-md"><p class="font-medium">{q.text}</p></td>
                                <td>
                                    <div class="flex flex-wrap gap-1">
                                        {#each optionList(q) as o}
                                            <span class:badge-success={o.index === q.correctOption} class:badge-ghost={o.index !== q.correctOption} class="badge badge-sm">{o.label}</span>
                                        {/each}
                                    </div>
                                </td>
                                <td><span class="badge badge-primary badge-sm">{LETTERS[q.correctOption] || ''}</span></td>
                                <td class="w-1 whitespace-nowrap text-right">
                                    <button type="button" class="btn btn-sm btn-square btn-ghost" title={$t('questions.edit')} onclick={() => openEdit(q)}>
                                        <AppIcon name="edit" class="h-4 w-4" />
                                    </button>
                                    <button type="button" class="btn btn-sm btn-square btn-ghost btn-error" title={$t('questions.delete')} onclick={() => remove(q)}>
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
            <p class="text-sm text-base-content/50">{$t('questions.empty')}</p>
        </div>
    {/if}

    <AppModal bind:open={modalOpen} title={editingId ? $t('questions.edit') : $t('questions.new')}>
        <form class="flex flex-col gap-4" onsubmit={save} novalidate>
            <div class="field">
                <label class="block text-sm font-semibold mb-1.5 text-base-content" for="q-text">{$t('questions.text')}</label>
                <textarea id="q-text" class="textarea w-full" rows="3" bind:value={text} required></textarea>
            </div>
            <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="q-a">A. {$t('questions.option')}</label>
                    <input id="q-a" class="input w-full" type="text" autocomplete="off" bind:value={optionA} required />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="q-b">B. {$t('questions.option')}</label>
                    <input id="q-b" class="input w-full" type="text" autocomplete="off" bind:value={optionB} required />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="q-c">C. {$t('questions.option')}</label>
                    <input id="q-c" class="input w-full" type="text" autocomplete="off" bind:value={optionC} required />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="q-d">D. {$t('questions.optionOptional')}</label>
                    <input id="q-d" class="input w-full" type="text" autocomplete="off" bind:value={optionD} />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="q-e">E. {$t('questions.optionOptional')}</label>
                    <input id="q-e" class="input w-full" type="text" autocomplete="off" bind:value={optionE} />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="q-correct">{$t('questions.correct')}</label>
                    <select id="q-correct" class="select w-full" bind:value={correctOption}>
                        <option value="0">A</option>
                        <option value="1">B</option>
                        <option value="2">C</option>
                        <option value="3">D</option>
                        <option value="4">E</option>
                    </select>
                </div>
            </div>
            <div class="flex justify-end gap-2">
                <AppBtn variant="secondary" onclick={() => (modalOpen = false)}>{$t('common.cancel')}</AppBtn>
                <AppBtn variant="primary" type="submit" loading={busy}>{$t('common.save')}</AppBtn>
            </div>
        </form>
    </AppModal>
</div>
