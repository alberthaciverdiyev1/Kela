(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/group/${id}`);
        document.getElementById('modalTitle').textContent = L["Js_EditGroup"];
        document.getElementById('entityId').value = data.id;
        document.getElementById('name').value = data.name;
        document.getElementById('description').value = data.description ?? '';
        const teacher = document.getElementById('user_id');
        if (teacher) teacher.value = data.userId ?? '';
        AdminUI.fileSet('image', data.image);
        errBox.classList.add('hidden');

        window.setWizardMode(false);
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js

        const res = await AdminUI.putForm(`/group/${id}`, window.buildGroupForm());
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); AdminUI.closeModal('modal'); loadList(pager.page); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
