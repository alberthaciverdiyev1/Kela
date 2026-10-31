// Qeyd formasının material seçimi və həmkar paylaşımı (create.js / update.js ortaq istifadə edir).
(function () {
    let visibility = 'private';
    let shareTargets = [];
    let selectedUsers = new Set();

    const el = (id) => document.getElementById(id);

    function setVisibility(value) {
        visibility = value === 'shared' ? 'shared' : 'private';
        el('visibility').value = visibility;
        el('sharePanel').classList.toggle('hidden', visibility !== 'shared');

        const tones = [['btnPrivate', visibility === 'private'], ['btnShared', visibility === 'shared']];
        tones.forEach(([id, active]) => {
            el(id).classList.toggle('admin-button-primary', active);
            el(id).classList.toggle('admin-button-secondary', !active);
        });
    }

    function syncPositionRow() {
        el('positionRow').classList.toggle('hidden', !el('contentId').value);
    }

    function renderShareTargets() {
        const list = el('userList');
        if (!shareTargets.length) {
            list.innerHTML = `<p class="px-2 py-1 text-sm text-slate-400">${AdminUI.escapeHtml(L["Note_NoShareTargets"])}</p>`;
            return;
        }

        list.innerHTML = shareTargets.map(u => `
            <label class="flex cursor-pointer items-center gap-2 rounded-lg px-2 py-1 text-sm hover:bg-slate-50">
                <input type="checkbox" class="note-share-target accent-primary" value="${u.id}" ${selectedUsers.has(u.id) ? 'checked' : ''} />
                <span class="truncate">${AdminUI.escapeHtml(u.name)}</span>
            </label>`).join('');

        list.querySelectorAll('.note-share-target').forEach(box => {
            box.addEventListener('change', () => {
                const id = Number(box.value);
                if (box.checked) selectedUsers.add(id); else selectedUsers.delete(id);
            });
        });
    }

    async function loadShareTargets(search) {
        const qs = search ? `?search=${encodeURIComponent(search)}` : '';
        shareTargets = await AdminUI.get(`/teacher-note/users${qs}`);
        renderShareTargets();
    }

    async function loadContents(search) {
        const qs = search ? `?search=${encodeURIComponent(search)}` : '';
        const options = await AdminUI.get(`/teacher-note/contents${qs}`);
        const select = el('contentId');
        const current = select.value;

        select.innerHTML = `<option value="">${AdminUI.escapeHtml(L["Note_NoMaterial"])}</option>` +
            options.map(o => `<option value="${o.id}">${AdminUI.escapeHtml(o.lessonTitle)} — ${AdminUI.escapeHtml(o.title)}</option>`).join('');

        if (current) select.value = current;
        syncPositionRow();
    }

    // Redaktədə material ilk 300 seçim arasında olmaya bilər — seçimi itirməmək üçün əlavə edilir.
    function keepContentOption(note) {
        if (!note.contentId) return;
        const select = el('contentId');
        if (select.querySelector(`option[value="${note.contentId}"]`)) return;

        const option = document.createElement('option');
        option.value = note.contentId;
        option.textContent = [note.lessonTitle, note.contentTitle].filter(Boolean).join(' — ');
        select.appendChild(option);
    }

    window.noteSetVisibility = setVisibility;

    window.noteSearchUsers = (input) => {
        clearTimeout(window.noteSearchUsers._t);
        window.noteSearchUsers._t = setTimeout(() => loadShareTargets(input.value.trim()), 250);
    };

    window.noteSearchContents = (input) => {
        clearTimeout(window.noteSearchContents._t);
        window.noteSearchContents._t = setTimeout(() => loadContents(input.value.trim()), 250);
    };

    window.noteForm = {
        async init(note) {
            note = note || null;
            selectedUsers = new Set((note?.sharedWith || []).map(u => u.id));

            setVisibility(note?.visibility || 'private');
            el('contentSearch').value = '';
            el('userSearch').value = '';
            el('contentId').value = '';
            el('positionSeconds').value = note?.positionSeconds ?? '';

            await Promise.all([loadContents(''), loadShareTargets('')]);
            keepContentOption(note);
            if (note?.contentId) el('contentId').value = note.contentId;
            syncPositionRow();
            renderShareTargets();
        },

        payload() {
            return {
                contentId: el('contentId').value,
                positionSeconds: el('contentId').value ? el('positionSeconds').value : '',
                visibility,
                sharedWithUserIds: Array.from(selectedUsers)
            };
        }
    };

    el('contentId').addEventListener('change', syncPositionRow);
})();
