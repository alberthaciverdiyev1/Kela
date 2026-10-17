(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/subject/${id}/edit`);
        document.getElementById('modalTitle').textContent = L["Js_EditSubject"];
        document.getElementById('entityId').value = data.id;
        document.getElementById('name').value = data.name;
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js idarə edir

        const fd = new FormData();
        fd.append('name', document.getElementById('name').value);

        const res = await AdminUI.putForm(`/subject/${id}`, fd);
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); AdminUI.closeModal('modal'); loadList(currentPage); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
