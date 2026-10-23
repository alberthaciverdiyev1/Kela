(function () {
    const form = document.getElementById('entityForm');

    function syncSelects() {
        ['group_ids', 'quiz_ids'].forEach(id => {
            const select = document.getElementById(id);
            if (select && AdminUI.syncSelect) AdminUI.syncSelect(select);
        });
    }

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Homework_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('title').value = '';
        document.getElementById('description').value = '';
        document.getElementById('due_date').value = '';
        document.getElementById('max_score').value = '100';
        document.getElementById('publishedHint').classList.add('hidden');

        ['group_ids', 'quiz_ids'].forEach(id => {
            const select = document.getElementById(id);
            select.disabled = false;
            [...select.options].forEach(o => { o.selected = false; });
        });
        document.getElementById('max_score').disabled = false;

        syncSelects();
        AdminUI.openModal('modal');
    };

    window.buildHomeworkForm = function () {
        const fd = new FormData();
        fd.append('title', document.getElementById('title').value.trim());
        fd.append('description', document.getElementById('description').value.trim());
        fd.append('max_score', document.getElementById('max_score').value || '0');

        const due = document.getElementById('due_date').value;
        if (due) fd.append('due_date', new Date(due).toISOString());

        ['group_ids', 'quiz_ids'].forEach(id => {
            [...document.getElementById(id).selectedOptions].forEach(o => fd.append(id, o.value));
        });

        return fd;
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const res = await AdminUI.postForm('/homework', window.buildHomeworkForm());
        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

        AdminUI.notify(L["Js_Created"]);
        AdminUI.closeModal('modal');
        loadList(1);
    });
})();
