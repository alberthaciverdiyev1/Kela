(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');


    window.buildUserForm = function () {
        const fd = new FormData();
        fd.append('name', document.getElementById('name').value || '');
        fd.append('surname', document.getElementById('surname').value || '');
        fd.append('email', document.getElementById('email').value);
        fd.append('phoneNumber', document.getElementById('phoneNumber').value || '');
        fd.append('type', document.getElementById('type').value);
        fd.append('password', document.getElementById('password').value || '');
        return fd;
    };

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["User_Create"];
        document.getElementById('entityId').value = '';
        ['name', 'surname', 'email', 'phoneNumber', 'password'].forEach(id => document.getElementById(id).value = '');
        document.getElementById('type').value = 'teacher';
        document.getElementById('passwordHint').classList.add('hidden');
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const res = await AdminUI.postForm('/user', window.buildUserForm());
        if (res.ok) { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
