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
        document.getElementById('modalTitle').textContent = 'Yeni dərs';
        document.getElementById('entityId').value = '';
        document.getElementById('subject_id').value = '';
        document.getElementById('title').value = '';
        document.getElementById('is_active').checked = true;
        document.getElementById('image').value = '';
        window.showModal();
    };

    window.buildLessonForm = function () {
        const fd = new FormData();
        fd.append('subject_id', document.getElementById('subject_id').value);
        fd.append('title', document.getElementById('title').value);
        fd.append('is_active', document.getElementById('is_active').checked ? 'true' : 'false');
        const file = document.getElementById('image').files[0];
        if (file) fd.append('Image', file);
        return fd;
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js idarə edir

        const res = await AdminUI.postForm('/lesson', window.buildLessonForm());
        if (res.ok) { AdminUI.notify('Əlavə olundu'); window.closeModal(); loadList(1); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
