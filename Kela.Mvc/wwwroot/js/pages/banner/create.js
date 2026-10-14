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

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = 'Yeni banner';
        document.getElementById('entityId').value = '';
        document.getElementById('title').value = '';
        document.getElementById('image').value = '';
        document.getElementById('image_preview').classList.add('hidden');
        document.getElementById('image_x').classList.add('hidden');
        document.getElementById('image_remove').value = 'false';
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js idarə edir

        const fd = new FormData();
        fd.append('title', document.getElementById('title').value);
        const file = document.getElementById('image').files[0];
        if (file) fd.append('Image', file);

        const res = await AdminUI.postForm('/banner', fd);
        if (res.ok) { AdminUI.notify('Əlavə olundu'); window.closeModal(); loadList(1); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
