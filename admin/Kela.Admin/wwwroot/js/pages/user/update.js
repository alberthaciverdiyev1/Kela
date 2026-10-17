(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const u = await AdminUI.get(`/user/${id}`);
        document.getElementById('modalTitle').textContent = L["Js_EditUser"];
        document.getElementById('entityId').value = u.id;
        document.getElementById('name').value = u.name ?? '';
        document.getElementById('surname').value = u.surname ?? '';
        document.getElementById('email').value = u.email;
        document.getElementById('phoneNumber').value = u.phoneNumber ?? '';
        document.getElementById('type').value = u.type;
        document.getElementById('password').value = '';
        document.getElementById('passwordHint').classList.remove('hidden');
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js

        const res = await AdminUI.putForm(`/user/${id}`, window.buildUserForm());
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); AdminUI.closeModal('modal'); loadList(currentPage); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
