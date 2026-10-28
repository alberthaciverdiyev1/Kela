(function () {
    const form = document.getElementById('entityForm');

    function toLocalInput(value) {
        if (!value) return '';
        const d = new Date(value);
        const pad = n => String(n).padStart(2, '0');
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    }

    window.openEdit = async function (id) {
        const h = await AdminUI.get(`/homework/${id}`);

        document.getElementById('saveBtn').classList.remove('hidden');
        ['saveDraftBtn', 'publishBtn'].forEach(id => document.getElementById(id).classList.add('hidden'));

        document.getElementById('modalTitle').textContent = L["Homework_Edit"];
        document.getElementById('entityId').value = h.id;
        document.getElementById('title').value = h.title;
        document.getElementById('description').value = h.description || '';
        document.getElementById('due_date').value = toLocalInput(h.dueDate);
        document.getElementById('max_score').value = h.maxScore;

        ['group_ids', 'quiz_ids'].forEach(selectId => {
            const select = document.getElementById(selectId);
            const wanted = new Set((selectId === 'group_ids' ? h.groups : h.quizzes).map(x => String(x.id)));
            [...select.options].forEach(o => { o.selected = wanted.has(o.value); });
        });

        // Yayımlanmış tapşırıqda hədəflər və bal kilidlənir.
        document.getElementById('group_ids').disabled = h.isPublished;
        document.getElementById('quiz_ids').disabled = h.isPublished;
        document.getElementById('max_score').disabled = h.isPublished;
        document.getElementById('publishedHint').classList.toggle('hidden', !h.isPublished);

        ['group_ids', 'quiz_ids'].forEach(id => {
            const select = document.getElementById(id);
            if (AdminUI.syncSelect) AdminUI.syncSelect(select);
        });

        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js

        const disabled = ['group_ids', 'quiz_ids', 'max_score'].filter(x => document.getElementById(x).disabled);
        disabled.forEach(x => { document.getElementById(x).disabled = false; });

        const fd = window.buildHomeworkForm();
        const res = await AdminUI.putForm(`/homework/${id}`, fd);

        disabled.forEach(x => { document.getElementById(x).disabled = true; });

        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

        AdminUI.notify(L["Js_Updated"]);
        AdminUI.closeModal('modal');
        loadList(pager.page);
    });
})();
