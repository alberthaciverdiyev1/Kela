(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const u = await AdminUI.get(`/user/${id}`);
        document.getElementById('modalTitle').textContent = 'İstifadəçi redaktəsi';
        document.getElementById('entityId').value = u.id;
        document.getElementById('name').value = u.name ?? '';
        document.getElementById('surname').value = u.surname ?? '';
        document.getElementById('email').value = u.email;
        document.getElementById('phoneNumber').value = u.phoneNumber ?? '';
        document.getElementById('type').value = u.type;
        document.getElementById('password').value = '';
        document.getElementById('passwordHint').classList.remove('hidden');
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js

        const res = await AdminUI.putForm(`/user/${id}`, window.buildUserForm());
        if (res.ok) { AdminUI.notify('Redaktə olundu'); window.closeModal(); loadList(currentPage); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
