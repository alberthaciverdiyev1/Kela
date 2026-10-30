(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Note_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('title').value = '';
        document.getElementById('body').value = '';
        document.getElementById('date').value = '';
        document.getElementById('isDone').checked = false;
        errBox.classList.add('hidden');
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return;

        const fd = new FormData();
        fd.append('title', document.getElementById('title').value);
        fd.append('body', document.getElementById('body').value);
        fd.append('date', document.getElementById('date').value);
        fd.append('isDone', document.getElementById('isDone').checked ? 'true' : 'false');

        const res = await AdminUI.postForm('/teacher-note', fd);
        if (res.ok) { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
