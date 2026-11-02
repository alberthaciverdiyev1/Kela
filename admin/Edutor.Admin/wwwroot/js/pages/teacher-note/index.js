// Şəxsi qeydlər: admin dizaynına uyğun kartlar + zəngin mətn redaktoru.
(function () {
    // Qeyd rəngi yalnız kartın yuxarı zolağında istifadə olunur.
    const ACCENTS = {
        default: '#94a3b8', red: '#ef4444', orange: '#f59e0b', yellow: '#eab308',
        green: '#10b981', teal: '#14b8a6', blue: '#3b82f6', purple: '#8b5cf6',
        pink: '#ec4899', gray: '#64748b', brown: '#a16207'
    };

    let notes = [];
    let search = '';
    let editingId = null;
    let draftColor = 'default';
    let draftPinned = false;

    const el = (id) => document.getElementById(id);

    function colorName(value) {
        return ACCENTS[value] ? value : 'default';
    }

    function accent(value) {
        return ACCENTS[colorName(value)];
    }

    function paletteButtons(active) {
        return Object.keys(ACCENTS).map(color => {
            const ring = color === active
                ? ' ring-2 ring-primary ring-offset-2'
                : ' hover:scale-110';
            return `<button type="button" class="h-6 w-6 rounded-full border border-black/10 transition${ring}"
                        style="background:${ACCENTS[color]}" title="${color}"
                        data-action="TeacherNotes.setDraftColor" data-args='["${color}"]'
                        aria-label="${color}" aria-pressed="${color === active}"></button>`;
        }).join('');
    }

    function plainText(html) {
        const box = document.createElement('div');
        box.innerHTML = html || '';
        return (box.textContent || '').replace(/\s+/g, ' ').trim().toLowerCase();
    }

    function bodyHtml(value) {
        if (!value) return '';
        if (!/[<&]/.test(value)) {
            return AdminUI.escapeHtml(value).replace(/\n/g, '<br>');
        }
        return value;
    }

    const iconButton = 'grid h-8 w-8 place-items-center rounded-lg text-slate-400 transition hover:bg-surface-secondary hover:text-slate-700';

    function card(note) {
        const pinnedIcon = note.isPinned
            ? `<svg class="h-4 w-4" viewBox="0 0 24 24" fill="currentColor"><path d="M14.5 2 22 9.5l-2 2-2.5-.5-4.4 4.4.9 3-1.5 1.5-4-4L3.9 21H3v-.9l5.1-5.1-4-4L5.5 9.5l3 .9L12.9 6l-.5-2.5 2-2Z"/></svg>`
            : `<svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M14.5 3 21 9.5l-2 2-2.5-.5-4.4 4.4.9 3-1.5 1.5-4-4L3.9 21H3v-.9l5.1-5.1-4-4L5.5 9.5l3 .9L12.9 6l-.5-2.5 2-2Z"/></svg>`;

        return `
            <article class="admin-card flex flex-col overflow-hidden">
                <span class="h-1.5 w-full shrink-0" style="background:${accent(note.color)}"></span>
                <div class="flex flex-1 flex-col p-4">
                    <div class="flex items-start gap-2">
                        <h3 class="min-w-0 flex-1 break-words text-sm font-bold text-slate-900">${AdminUI.escapeHtml(note.title)}</h3>
                        <button type="button"
                                class="${iconButton} ${note.isPinned ? '!text-primary' : ''}"
                                title="${AdminUI.escapeHtml(note.isPinned ? L["Note_Unpin"] : L["Note_Pin"])}"
                                data-action="TeacherNotes.togglePin" data-args='[${note.id}, ${note.isPinned ? 'false' : 'true'}]'>${pinnedIcon}</button>
                    </div>
                    <div class="note-card-body mt-2 max-h-56 overflow-hidden break-words">${bodyHtml(note.body)}</div>
                    <div class="mt-4 flex items-center justify-between border-t border-border pt-3">
                        <div class="flex items-center gap-2">
                            <span class="h-2 w-2 rounded-full" style="background:${accent(note.color)}"></span>
                            <span class="text-[11px] font-medium text-slate-400">${AdminUI.escapeHtml(new Date(note.createdAt).toLocaleDateString())}</span>
                        </div>
                        <div class="flex items-center gap-1">
                            <button type="button" class="${iconButton}" title="${AdminUI.escapeHtml(L["Js_Edit"])}"
                                    data-action="TeacherNotes.edit" data-args='[${note.id}]'>
                                <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="m16.5 3.5 4 4L8 20H4v-4L16.5 3.5Z"/></svg>
                            </button>
                            <button type="button" class="${iconButton} hover:!text-danger" title="${AdminUI.escapeHtml(L["Common_Delete"])}"
                                    data-action="TeacherNotes.remove" data-args='[${note.id}]'>
                                <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M4 7h16M9 7V5h6v2m-8 0 1 13h8l1-13"/></svg>
                            </button>
                        </div>
                    </div>
                </div>
            </article>`;
    }

    function section(title, items) {
        if (!items.length) return '';
        return `
            <section>
                <p class="mb-2 text-[11px] font-bold uppercase tracking-wider text-slate-400">${AdminUI.escapeHtml(title)}</p>
                <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">${items.map(card).join('')}</div>
            </section>`;
    }

    function render() {
        const board = el('noteBoard');
        const term = search.trim().toLowerCase();
        const visible = term
            ? notes.filter(n => (n.title || '').toLowerCase().includes(term) || plainText(n.body).includes(term))
            : notes;

        if (!visible.length) {
            board.innerHTML = `<div class="admin-card py-14 text-center text-sm text-slate-400">
                ${AdminUI.escapeHtml(term ? L["Note_NoMatch"] : L["Note_Empty"])}</div>`;
            return;
        }

        board.innerHTML =
            section(L["Note_Pinned"], visible.filter(n => n.isPinned)) +
            section(L["Note_Others"], visible.filter(n => !n.isPinned));
    }

    async function load() {
        const data = await AdminUI.get('/teacher-note/list?per_page=300');
        notes = data?.data || [];
        render();
    }

    function renderDraftPalette() {
        el('notePalette').innerHTML = paletteButtons(draftColor);
    }

    function paintPin() {
        el('notePinLabel').textContent = draftPinned ? L["Note_Unpin"] : L["Note_Pin"];
        el('notePinToggle').setAttribute('aria-pressed', String(draftPinned));
        el('notePinIcon').classList.toggle('text-primary', draftPinned);
    }

    function resetDraft() {
        editingId = null;
        draftColor = 'default';
        draftPinned = false;
        AdminUI.resetForm('noteComposer');
        el('noteTitle').value = '';
        AdminUI.syncEditors(el('noteComposer'));
        renderDraftPalette();
        paintPin();
    }

    function openComposer() {
        resetDraft();
        el('noteComposerWrap').classList.remove('hidden');
        el('noteTitle').focus();
        el('noteComposerWrap').scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    function cancelComposer() {
        el('noteComposerWrap').classList.add('hidden');
        resetDraft();
    }

    function setDraftColor(color) {
        draftColor = colorName(color);
        renderDraftPalette();
    }

    function toggleDraftPin() {
        draftPinned = !draftPinned;
        paintPin();
    }

    function edit(id) {
        const note = notes.find(n => n.id === id);
        if (!note) return;

        resetDraft();
        editingId = id;
        draftColor = colorName(note.color);
        draftPinned = note.isPinned;

        el('noteTitle').value = note.title;
        el('noteBody').value = note.body || '';
        renderDraftPalette();
        paintPin();
        el('noteComposerWrap').classList.remove('hidden');
        AdminUI.syncEditors(el('noteComposer'));
        el('noteComposerWrap').scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    async function togglePin(id, pinned) {
        const res = await AdminUI.putJson(`/teacher-note/${id}/pin`, { pinned });
        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }
        await load();
    }

    async function remove(id) {
        if (!await AdminUI.confirmDelete(L["Js_AreYouSure"])) return;
        const res = await AdminUI.del(`/teacher-note/${id}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); await load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    window.TeacherNotes = { openComposer, cancelComposer, setDraftColor, toggleDraftPin, edit, togglePin, remove };

    document.addEventListener('DOMContentLoaded', () => {
        renderDraftPalette();
        paintPin();
        el('noteSearch').addEventListener('input', (e) => { search = e.target.value; render(); });
        el('noteNew').addEventListener('click', openComposer);

        el('noteComposer').addEventListener('submit', async (e) => {
            e.preventDefault();

            const title = el('noteTitle').value.trim();
            const body = el('noteBody').value.trim();
            if (!title) { AdminUI.error(L["Common_RequiredFields"]); el('noteTitle').focus(); return; }
            if (!body) { AdminUI.error(L["Note_BodyRequired"]); return; }

            const fd = new FormData();
            fd.append('title', title);
            fd.append('body', body);
            fd.append('color', draftColor);
            fd.append('isPinned', draftPinned ? 'true' : 'false');
            fd.append('visibility', 'private');

            const res = editingId
                ? await AdminUI.putForm(`/teacher-note/${editingId}`, fd)
                : await AdminUI.postForm('/teacher-note', fd);

            if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

            AdminUI.notify(editingId ? L["Js_Updated"] : L["Js_Added"]);
            cancelComposer();
            await load();
        });

        load();
    });
})();
