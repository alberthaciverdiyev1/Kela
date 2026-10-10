(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.showModal = function () {
        document.getElementById('modal').classList.remove('hidden');
        document.getElementById('modal').classList.add('flex');
    };
    window.closeModal = function () {
        document.getElementById('modal').classList.add('hidden');
        document.getElementById('modal').classList.remove('flex');
        errBox.classList.add('hidden');
    };

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
        document.getElementById('modalTitle').textContent = 'Yeni istifadəçi';
        document.getElementById('entityId').value = '';
        ['name', 'surname', 'email', 'phoneNumber', 'password'].forEach(id => document.getElementById(id).value = '');
        document.getElementById('type').value = 'teacher';
        document.getElementById('passwordHint').classList.add('hidden');
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const res = await AdminUI.postForm('/user', window.buildUserForm());
        if (res.ok) { AdminUI.notify('Əlavə olundu'); window.closeModal(); loadList(1); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
