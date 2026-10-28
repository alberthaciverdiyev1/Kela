(function () {
    const form = document.getElementById('entityForm');

    window.openEdit = async function (id) {
        const h = await AdminUI.get(`/homework/${id}`);

        document.getElementById('saveBtn').classList.remove('hidden');
        ['saveDraftBtn', 'publishBtn'].forEach(id => document.getElementById(id).classList.add('hidden'));

        document.getElementById('modalTitle').textContent = L["Homework_Edit"];
        document.getElementById('entityId').value = h.id;
        document.getElementById('title').value = h.title;
        document.getElementById('description').value = h.description || '';

        ['group_ids', 'quiz_ids'].forEach(selectId => {
            const select = document.getElementById(selectId);
            const wanted = new Set((selectId === 'group_ids' ? h.groups : h.quizzes).map(x => String(x.id)));
            [...select.options].forEach(o => { o.selected = wanted.has(o.value); });
        });

        // Yayımlanmış tapşırıqda hədəflər, son tarix və bal kilidlənir.
        document.getElementById('group_ids').disabled = h.isPublished;
        document.getElementById('quiz_ids').disabled = h.isPublished;
        document.getElementById('publishedHint').classList.toggle('hidden', !h.isPublished);

        ['group_ids', 'quiz_ids'].forEach(id => {
            const select = document.getElementById(id);
            if (AdminUI.syncSelect) AdminUI.syncSelect(select);
        });

        window.renderGroupSettings(h.groups);
        document.querySelectorAll('#groupSettings [data-setting]').forEach(input => { input.disabled = h.isPublished; });

        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js

        // Disabled sahələr FormData-ya düşmür — müvəqqəti açılır.
        const disabled = ['group_ids', 'quiz_ids'].filter(x => document.getElementById(x).disabled);
        disabled.forEach(x => { document.getElementById(x).disabled = false; });

        const settings = [...document.querySelectorAll('#groupSettings [data-setting]:disabled')];
        settings.forEach(x => { x.disabled = false; });

        const fd = window.buildHomeworkForm();
        const res = await AdminUI.putForm(`/homework/${id}`, fd);

        disabled.forEach(x => { document.getElementById(x).disabled = true; });
        settings.forEach(x => { x.disabled = true; });

        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

        AdminUI.notify(L["Js_Updated"]);
        AdminUI.closeModal('modal');
        loadList(pager.page);
    });
})();
