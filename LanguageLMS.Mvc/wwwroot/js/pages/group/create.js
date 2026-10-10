(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    function showError(m) { errBox.textContent = m; errBox.classList.remove('hidden'); }
    function done() { AdminUI.notify('Əlavə olundu'); window.closeModal(); loadList(1); }

    window.showModal = function () {
        document.getElementById('modal').classList.remove('hidden');
        document.getElementById('modal').classList.add('flex');
    };
    window.closeModal = function () {
        document.getElementById('modal').classList.add('hidden');
        document.getElementById('modal').classList.remove('flex');
        errBox.classList.add('hidden');
    };

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = 'Yeni qrup';
        document.getElementById('entityId').value = '';
        document.getElementById('name').value = '';
        document.getElementById('description').value = '';
        document.getElementById('user_id').value = '';
        document.getElementById('image').value = '';
        document.getElementById('removeImageBlock').classList.add('hidden');
        window.showModal();
    };

    window.buildGroupForm = function () {
        const fd = new FormData();
        fd.append('name', document.getElementById('name').value);
        fd.append('description', document.getElementById('description').value || '');
        fd.append('user_id', document.getElementById('user_id').value || '');
        fd.append('RemoveImage', document.getElementById('remove_image').checked ? 'true' : 'false');
        const file = document.getElementById('image').files[0];
        if (file) fd.append('Image', file);
        return fd;
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const res = await AdminUI.postForm('/group', window.buildGroupForm());
        if (res.ok) { done(); return; }
        showError(res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi');
    });
})();
