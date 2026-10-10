(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/group/${id}`);
        document.getElementById('modalTitle').textContent = 'Qrup redaktəsi';
        document.getElementById('entityId').value = data.id;
        document.getElementById('name').value = data.name;
        document.getElementById('description').value = data.description ?? '';
        document.getElementById('user_id').value = data.userId ?? '';
        document.getElementById('image').value = '';
        document.getElementById('remove_image').checked = false;
        document.getElementById('removeImageBlock').classList.remove('hidden');
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js

        const res = await AdminUI.putForm(`/group/${id}`, window.buildGroupForm());
        if (res.ok) { AdminUI.notify('Redaktə olundu'); window.closeModal(); loadList(currentPage); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
