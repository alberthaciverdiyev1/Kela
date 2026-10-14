(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/lesson-content/${id}`);
        document.getElementById('modalTitle').textContent = 'Material redaktəsi';
        document.getElementById('entityId').value = data.id;
        document.getElementById('sort_order').value = data.sortOrder ?? 0;
        document.getElementById('description').value = data.description ?? '';
        document.getElementById('lesson_id').value = data.lessonId;
        document.getElementById('title').value = data.title;
        document.getElementById('type').value = data.type ?? 'video';
        document.getElementById('is_active').checked = data.isActive;
        document.getElementById('fileBlock').classList.add('hidden');
        document.getElementById('lesson_id').disabled = true;
        document.getElementById('type').disabled = true;
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js idarə edir

        const fd = new FormData();
        fd.append('title', document.getElementById('title').value);
        fd.append('sort_order', document.getElementById('sort_order').value || '0');
        fd.append('description', document.getElementById('description').value || '');
        fd.append('is_active', document.getElementById('is_active').checked ? 'true' : 'false');

        const res = await AdminUI.putForm(`/lesson-content/${id}`, fd);
        if (res.ok) { AdminUI.notify('Redaktə olundu'); window.closeModal(); loadList(currentPage); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
