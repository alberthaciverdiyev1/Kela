(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    // Tək müəllim varsa paylaşım açarı göstərilmir — mövcud dəyər itməsin.
    let currentShared = false;

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/subject/${id}/edit`);
        document.getElementById('modalTitle').textContent = L["Js_EditSubject"];
        document.getElementById('entityId').value = data.id;
        document.getElementById('name').value = data.name;
        currentShared = data.isShared === true;
        const sharedToggle = document.getElementById('is_shared');
        if (sharedToggle) sharedToggle.checked = currentShared;
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js idarə edir

        const fd = new FormData();
        fd.append('name', document.getElementById('name').value);
        const sharedField = document.getElementById('is_shared');
        fd.append('is_shared', (sharedField ? sharedField.checked : currentShared) ? 'true' : 'false');

        const res = await AdminUI.putForm(`/subject/${id}`, fd);
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); AdminUI.closeModal('modal'); loadList(pager.page); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
