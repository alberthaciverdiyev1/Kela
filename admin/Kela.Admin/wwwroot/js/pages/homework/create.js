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

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Homework_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('title').value = '';
        document.getElementById('description').value = '';
        document.getElementById('due_date').value = '';
        document.getElementById('max_score').value = '100';
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
        document.getElementById('max_score').disabled = false;

        syncSelects();
        AdminUI.openModal('modal');
    };

    window.buildHomeworkForm = function () {
        const fd = new FormData();
        fd.append('title', document.getElementById('title').value.trim());
        fd.append('description', window.homeworkDescription());
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
