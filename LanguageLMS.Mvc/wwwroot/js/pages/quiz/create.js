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

    window.buildQuizForm = function () {
        const fd = new FormData();
        fd.append('lesson_id', document.getElementById('lesson_id').value);
        fd.append('title', document.getElementById('title').value);
        fd.append('duration_minutes', document.getElementById('duration_minutes').value || '0');
        fd.append('type', document.getElementById('type').value);
        fd.append('is_active', document.getElementById('is_active').checked ? 'true' : 'false');
        fd.append('is_repeat', document.getElementById('is_repeat').checked ? 'true' : 'false');
        return fd;
    };

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = 'Yeni imtahan';
        document.getElementById('entityId').value = '';
        document.getElementById('lesson_id').value = '';
        document.getElementById('title').value = '';
        document.getElementById('duration_minutes').value = 0;
        document.getElementById('type').value = 'lesson';
        document.getElementById('is_active').checked = true;
        document.getElementById('is_repeat').checked = false;
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const res = await AdminUI.postForm('/quiz', window.buildQuizForm());
        if (res.ok) { AdminUI.notify('Əlavə olundu'); window.closeModal(); loadList(1); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
