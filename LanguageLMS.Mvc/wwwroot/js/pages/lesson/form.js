(function () {
    const modal = document.getElementById('modal');
    const form = document.getElementById('lessonForm');
    const errBox = document.getElementById('formError');

    function showModal() { modal.classList.remove('hidden'); modal.classList.add('flex'); }
    function closeModal() { modal.classList.add('hidden'); modal.classList.remove('flex'); errBox.classList.add('hidden'); }

    function fill(data) {
        document.getElementById('lessonId').value = data?.id ?? '';
        document.getElementById('subject_id').value = data?.subjectId ?? '';
        document.getElementById('title').value = data?.title ?? '';
        document.getElementById('sort_order').value = data?.sortOrder ?? 0;
        document.getElementById('is_active').checked = data ? data.isActive : true;
        document.getElementById('image').value = '';
    }

    function openCreate() { document.getElementById('modalTitle').textContent = 'Yeni dərs'; fill(null); showModal(); }

    async function openEdit(id) {
        const data = await AdminUI.get(`/Lesson/Get?id=${id}`);
        document.getElementById('modalTitle').textContent = 'Dərs redaktəsi';
        fill(data);
        showModal();
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('lessonId').value;
        const fd = new FormData();
        fd.append('subject_id', document.getElementById('subject_id').value);
        fd.append('title', document.getElementById('title').value);
        fd.append('sort_order', document.getElementById('sort_order').value || '0');
        fd.append('is_active', document.getElementById('is_active').checked ? 'true' : 'false');
        const file = document.getElementById('image').files[0];
        if (file) fd.append('Image', file);

        const res = await AdminUI.postForm(id ? `/Lesson/Update?id=${id}` : '/Lesson/Create', fd);

        if (res.ok) { AdminUI.notify(id ? 'Redaktə olundu' : 'Əlavə olundu'); AdminUI.reloadAfter(); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });

    window.openCreate = openCreate;
    window.openEdit = openEdit;
    window.closeModal = closeModal;
})();
