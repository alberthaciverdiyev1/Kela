(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');


    window.buildQuizForm = function () {
        const fd = new FormData();
        fd.append('lesson_id', document.getElementById('lesson_id').value);
        fd.append('title', document.getElementById('title').value);
        fd.append('duration_minutes', document.getElementById('duration_minutes').value || '0');
        const sortOrder = document.getElementById('sort_order').value;
        if (sortOrder !== '') fd.append('sort_order', sortOrder);
        fd.append('type', document.getElementById('type').value);
        fd.append('is_active', document.getElementById('is_active').checked ? 'true' : 'false');
        fd.append('is_repeat', document.getElementById('is_repeat').checked ? 'true' : 'false');
        return fd;
    };

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Quiz_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('sort_order').value = '';
        document.getElementById('lesson_id').value = '';
        document.getElementById('title').value = '';
        document.getElementById('duration_minutes').value = 0;
        document.getElementById('type').value = 'lesson';
        document.getElementById('is_active').checked = true;
        document.getElementById('is_repeat').checked = false;
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const res = await AdminUI.postForm('/quiz', window.buildQuizForm());
        if (res.ok) { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
