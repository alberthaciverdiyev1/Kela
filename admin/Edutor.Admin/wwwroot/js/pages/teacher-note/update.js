(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/teacher-note/${id}`);
        document.getElementById('modalTitle').textContent = L["Js_Edit"];
        document.getElementById('entityId').value = data.id;
        document.getElementById('title').value = data.title;
        document.getElementById('body').value = data.body || '';
        document.getElementById('date').value = data.date || '';
        document.getElementById('isDone').checked = data.isDone;
        errBox.classList.add('hidden');
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return;

        const fd = new FormData();
        fd.append('title', document.getElementById('title').value);
        fd.append('body', document.getElementById('body').value);
        fd.append('date', document.getElementById('date').value);
        fd.append('isDone', document.getElementById('isDone').checked ? 'true' : 'false');

        const res = await AdminUI.putForm(`/teacher-note/${id}`, fd);
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); AdminUI.closeModal('modal'); loadList(pager.page); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
