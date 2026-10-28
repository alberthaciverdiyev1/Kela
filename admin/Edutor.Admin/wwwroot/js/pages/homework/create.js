(function () {
    const form = document.getElementById('entityForm');

    // Boş editor HTML qalığı (<br>, <p></p>) saxlamasın; şəkil varsa mətn boş olsa da qalsın.
    window.homeworkDescription = function () {
        const raw = (document.getElementById('description')?.value || '').trim();
        if (!raw) return '';

        const tmp = document.createElement('div');
        tmp.innerHTML = raw;
        return !tmp.querySelector('img') && !tmp.textContent.trim() ? '' : raw;
    };

    function syncSelects() {
        ['group_ids', 'quiz_ids'].forEach(id => {
            const select = document.getElementById(id);
            if (select && AdminUI.syncSelect) AdminUI.syncSelect(select);
        });
    }

    function toLocalInput(value) {
        if (!value) return '';
        const d = new Date(value);
        const pad = n => String(n).padStart(2, '0');
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    }

    function settingRow(id) {
        return document.querySelector(`#groupSettings [data-group-id="${id}"]`);
    }

    // Hər seçilmiş qrup üçün ayrıca son tarix və maksimum bal göstərilir.
    window.renderGroupSettings = function (initial) {
        const container = document.getElementById('groupSettings');
        const select = document.getElementById('group_ids');
        const selected = [...select.selectedOptions];

        const current = new Map([...container.querySelectorAll('[data-group-id]')].map(row => [
            row.dataset.groupId,
            {
                dueDate: row.querySelector('[data-setting="due"]').value,
                maxScore: row.querySelector('[data-setting="score"]').value
            }
        ]));

        const preset = new Map((initial || []).map(g => [String(g.id), g]));
        container.innerHTML = '';

        selected.forEach(option => {
            const saved = preset.get(option.value);
            const dueDate = current.get(option.value)?.dueDate
                || (saved ? toLocalInput(saved.dueDate) : '');
            const maxScore = current.get(option.value)?.maxScore
                || (saved ? saved.maxScore : '100');

            const row = document.createElement('div');
            row.dataset.groupId = option.value;
            row.className = 'rounded-xl border border-border bg-surface-secondary p-3';
            row.innerHTML = `
                <p class="text-sm font-semibold text-slate-700">${AdminUI.escapeHtml(option.text)}</p>
                <div class="mt-2 grid gap-2 sm:grid-cols-2">
                    <label class="text-xs font-semibold text-slate-500">${L["Homework_DueDate"]}
                        <input type="datetime-local" data-setting="due" class="admin-field mt-1" value="${dueDate}" required />
                    </label>
                    <label class="text-xs font-semibold text-slate-500">${L["Homework_MaxScore"]}
                        <input type="number" data-setting="score" class="admin-field mt-1" min="0" step="0.01" value="${maxScore}" required />
                    </label>
                </div>`;
            container.appendChild(row);
        });
    }

    // Yaradılışda iki seçim: qaralama kimi saxla və ya dərhal yayımla.
    function setSaveMode(create) {
        document.getElementById('saveBtn').classList.toggle('hidden', create);
        document.getElementById('saveDraftBtn').classList.toggle('hidden', !create);
        document.getElementById('publishBtn').classList.toggle('hidden', !create);
    }

    window.openCreate = function () {
        setSaveMode(true);
        document.getElementById('modalTitle').textContent = L["Homework_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('title').value = '';
        document.getElementById('description').value = '';
        document.getElementById('publishedHint').classList.add('hidden');

        // Qrup səhifəsindən gəlmişiksə (/homework?group_id=X) həmin qrup seçili olsun.
        const presetGroup = window.HOMEWORK_GROUP_ID || '';

        ['group_ids', 'quiz_ids'].forEach(id => {
            const select = document.getElementById(id);
            select.disabled = false;
            [...select.options].forEach(o => { o.selected = false; });
        });

        if (presetGroup) {
            [...document.getElementById('group_ids').options]
                .forEach(o => { o.selected = o.value === presetGroup; });
        }

        syncSelects();
        window.renderGroupSettings();
        AdminUI.openModal('modal');
    };

    window.buildHomeworkForm = function () {
        const fd = new FormData();
        fd.append('title', document.getElementById('title').value.trim());
        fd.append('description', window.homeworkDescription());

        [...document.getElementById('group_ids').selectedOptions].forEach(o => {
            fd.append('group_ids', o.value);

            const row = settingRow(o.value);
            const due = row?.querySelector('[data-setting="due"]').value;
            const score = row?.querySelector('[data-setting="score"]').value;
            fd.append('group_due_dates', due ? new Date(due).toISOString() : '');
            fd.append('group_scores', score || '0');
        });

        [...document.getElementById('quiz_ids').selectedOptions].forEach(o => fd.append('quiz_ids', o.value));

        return fd;
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const publish = e.submitter?.id === 'publishBtn';
        const groupSelect = document.getElementById('group_ids');

        // Yayımlamaq üçün ən azı bir qrup lazımdır — əvvəlcədən yoxlanılır ki,
        // qaralama yaranıb yarımçıq qalmasın.
        if (publish && !groupSelect.selectedOptions.length) {
            AdminUI.error(L["Homework_GroupRequired"]);
            return;
        }

        const missing = [...groupSelect.selectedOptions].find(o => {
            const row = settingRow(o.value);
            const due = row?.querySelector('[data-setting="due"]')?.value;
            const score = Number(row?.querySelector('[data-setting="score"]')?.value || 0);
            return !due || score <= 0;
        });
        if (missing) {
            AdminUI.error(L["Homework_GroupDeadlineRequired"]);
            return;
        }

        const res = await AdminUI.postForm('/homework', window.buildHomeworkForm());
        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

        if (publish) {
            const pub = await AdminUI.postForm(`/homework/${res.data.id}/publish`, new FormData());
            if (!pub.ok) {
                AdminUI.error(AdminUI.errText(pub));
                AdminUI.closeModal('modal');
                loadList(1);
                return;
            }
        }

        AdminUI.notify(publish ? L["Js_Published"] : L["Js_Created"]);
        AdminUI.closeModal('modal');
        loadList(1);
    });
})();

document.getElementById('group_ids')?.addEventListener('change', () => window.renderGroupSettings());
