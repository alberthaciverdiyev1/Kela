// Google Keep üslubunda şəxsi qeydlər: lövhə + inline redaktor.
(function () {
    const COLORS = {
        default: '#ffffff', red: '#f8b4b4', orange: '#fdd9a0', yellow: '#fff3a3',
        green: '#c9f2b2', teal: '#a9efe4', blue: '#bfe3fb', purple: '#dcc9f7',
        pink: '#f9c9e0', gray: '#e2e5e9', brown: '#e3cbb0'
    };

    let notes = [];
    let search = '';
    let editingId = null;
    let draftColor = 'default';
    let draftPinned = false;

    const el = (id) => document.getElementById(id);

    function colorName(value) {
        return COLORS[value] ? value : 'default';
    }

    function paletteButtons(active, action, id) {
        return Object.keys(COLORS).map(color => {
            const ring = color === active ? ' ring-2 ring-slate-500 ring-offset-1' : '';
            const args = id === undefined ? `"${color}"` : `[${id},"${color}"]`;
            return `<button type="button" class="h-5 w-5 rounded-full border border-black/10${ring}"
                        style="background:${COLORS[color]}" title="${color}"
                        data-action="${action}" data-args='${args}' aria-label="${color}"></button>`;
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

    function card(note) {
        const pinned = note.isPinned
            ? `<svg class="h-4 w-4" viewBox="0 0 24 24" fill="currentColor"><path d="M14.5 2 22 9.5l-2 2-2.5-.5-4.4 4.4.9 3-1.5 1.5-4-4L3.9 21H3v-.9l5.1-5.1-4-4L5.5 9.5l3 .9L12.9 6l-.5-2.5 2-2Z"/></svg>`
            : `<svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M14.5 3 21 9.5l-2 2-2.5-.5-4.4 4.4.9 3-1.5 1.5-4-4L3.9 21H3v-.9l5.1-5.1-4-4L5.5 9.5l3 .9L12.9 6l-.5-2.5 2-2Z"/></svg>`;

        return `
            <article class="group mb-3 break-inside-avoid overflow-hidden rounded-xl border border-black/5 shadow-sm transition hover:shadow-md"
                     style="background:${COLORS[colorName(note.color)]}">
                <div class="p-3">
                    <div class="flex items-start gap-2">
                        <h3 class="min-w-0 flex-1 break-words text-sm font-bold text-slate-800">${AdminUI.escapeHtml(note.title)}</h3>
                        <button type="button" class="shrink-0 rounded-md p-1 text-slate-500 hover:bg-black/5"
                                title="${note.isPinned ? AdminUI.escapeHtml(L["Note_Unpin"]) : AdminUI.escapeHtml(L["Note_Pin"])}"
                                data-action="TeacherNotes.togglePin" data-args='[${note.id}, ${note.isPinned ? 'false' : 'true'}]'>${pinned}</button>
                    </div>
                    <div class="note-card-body mt-1 break-words text-[13px] leading-5 text-slate-700">${bodyHtml(note.body)}</div>
                    <div class="mt-2 flex flex-wrap items-center gap-1.5 opacity-60 transition group-hover:opacity-100">
                        ${paletteButtons(colorName(note.color), 'TeacherNotes.setColor', note.id)}
                    </div>
                </div>
                <div class="flex items-center justify-end gap-1 px-2 pb-2 text-slate-500">
                    <span class="mr-auto pl-1 text-[10px] font-medium text-slate-500">${AdminUI.escapeHtml(new Date(note.createdAt).toLocaleDateString())}</span>
                    <button type="button" class="rounded-md p-1.5 hover:bg-black/5" title="${AdminUI.escapeHtml(L["Js_Edit"])}"
                            data-action="TeacherNotes.edit" data-args='[${note.id}]'>
                        <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="m16.5 3.5 4 4L8 20H4v-4L16.5 3.5Z"/></svg>
                    </button>
                    <button type="button" class="rounded-md p-1.5 hover:bg-black/5" title="${AdminUI.escapeHtml(L["Common_Delete"])}"
                            data-action="TeacherNotes.remove" data-args='[${note.id}]'>
                        <svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M4 7h16M9 7V5h6v2m-8 0 1 13h8l1-13"/></svg>
                    </button>
                </div>
            </article>`;
    }

    function section(title, items) {
        if (!items.length) return '';
        return `
            <p class="mb-2 mt-4 text-[11px] font-bold uppercase tracking-wide text-slate-400">${AdminUI.escapeHtml(title)}</p>
            <div class="columns-1 gap-3 sm:columns-2 lg:columns-3">${items.map(card).join('')}</div>`;
    }

    function render() {
        const board = el('noteBoard');
        const term = search.trim().toLowerCase();
        const visible = term
            ? notes.filter(n => (n.title || '').toLowerCase().includes(term) || plainText(n.body).includes(term))
            : notes;

        if (!visible.length) {
            board.innerHTML = `<div class="admin-empty py-12 text-center text-sm text-slate-400">
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

    function setExpanded(on) {
        el('noteComposerBody').classList.toggle('hidden', !on);
    }

    function resetDraft() {
        editingId = null;
        draftColor = 'default';
        draftPinned = false;
        AdminUI.resetForm('noteComposer');
        el('noteTitle').value = '';
        el('notePinLabel').textContent = L["Note_Pin"];
        renderDraftPalette();
    }

    function renderDraftPalette() {
        el('notePalette').innerHTML = paletteButtons(draftColor, 'TeacherNotes.setDraftColor');
    }

    function expandComposer() {
        setExpanded(true);
        el('noteTitle').focus();
    }

    function cancelComposer() {
        resetDraft();
        setExpanded(false);
    }

    function setDraftColor(color) {
        draftColor = colorName(color);
        renderDraftPalette();
    }

    function toggleDraftPin() {
        draftPinned = !draftPinned;
        el('notePinLabel').textContent = draftPinned ? L["Note_Unpin"] : L["Note_Pin"];
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
        el('notePinLabel').textContent = draftPinned ? L["Note_Unpin"] : L["Note_Pin"];
        renderDraftPalette();
        setExpanded(true);
        AdminUI.syncEditors(el('noteComposer'));
        el('noteComposer').scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    async function togglePin(id, pinned) {
        const res = await AdminUI.putJson(`/teacher-note/${id}/pin`, { pinned });
        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }
        await load();
    }

    async function setColor(id, color) {
        const res = await AdminUI.putJson(`/teacher-note/${id}/color`, { color });
        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }
        await load();
    }

    async function remove(id) {
        if (!await AdminUI.confirmDelete(L["Js_AreYouSure"])) return;
        const res = await AdminUI.del(`/teacher-note/${id}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); await load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    window.TeacherNotes = { expandComposer, cancelComposer, setDraftColor, toggleDraftPin, edit, togglePin, setColor, remove };

    document.addEventListener('DOMContentLoaded', () => {
        renderDraftPalette();
        el('noteSearch').addEventListener('input', (e) => { search = e.target.value; render(); });
        el('noteTitle').addEventListener('focus', () => setExpanded(true));

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
