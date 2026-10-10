(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/lesson/${id}`);
        document.getElementById('modalTitle').textContent = 'Dərs redaktəsi';
        document.getElementById('entityId').value = data.id;
        document.getElementById('subject_id').value = data.subjectId;
        document.getElementById('title').value = data.title;
        document.getElementById('sort_order').value = data.sortOrder;
        document.getElementById('is_active').checked = data.isActive;
        document.getElementById('image').value = '';
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js idarə edir

        const res = await AdminUI.putForm(`/lesson/${id}`, window.buildLessonForm());
        if (res.ok) { AdminUI.notify('Redaktə olundu'); window.closeModal(); loadList(currentPage); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
