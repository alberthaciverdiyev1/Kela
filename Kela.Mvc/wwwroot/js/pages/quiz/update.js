(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const d = await AdminUI.get(`/quiz/${id}`);
        document.getElementById('modalTitle').textContent = 'İmtahan redaktəsi';
        document.getElementById('entityId').value = d.id;
        document.getElementById('sort_order').value = d.sortOrder ?? 0;
        document.getElementById('lesson_id').value = d.lessonId ?? '';
        document.getElementById('title').value = d.title;
        document.getElementById('duration_minutes').value = d.durationMinutes;
        document.getElementById('type').value = d.type;
        document.getElementById('is_active').checked = d.isActive;
        document.getElementById('is_repeat').checked = d.isRepeat;
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js

        const res = await AdminUI.putForm(`/quiz/${id}`, window.buildQuizForm());
        if (res.ok) { AdminUI.notify('Redaktə olundu'); window.closeModal(); loadList(currentPage); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
