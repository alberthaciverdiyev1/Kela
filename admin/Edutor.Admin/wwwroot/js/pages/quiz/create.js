(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    const lessonSelect = document.getElementById('lesson_id');

    // Fənn filtri üçün ilkin siyahı; variant mətni artıq "Fənn — Dərs" formatındadır.
    const allLessons = [...lessonSelect.options].map(o => ({
        id: o.value,
        text: o.text,
        subjectId: (window.LESSON_SUBJECTS || {})[o.value] ?? ''
    }));

    // Fənn seçiminə uyğun dərs siyahısını yenidən qurur. selectedLessonId verilməsə
    // seçim sıfırlanır (filtr dəyişəndə köhnə dərs qalmasın).
    window.filterQuizLessons = function (selectedLessonId) {
        const subjectId = document.getElementById('subject_filter').value;
        const keep = selectedLessonId ?? '';
        const items = subjectId ? allLessons.filter(l => l.subjectId === subjectId) : allLessons;

        lessonSelect.innerHTML =
            `<option value="" disabled${keep ? '' : ' selected'}>${AdminUI.escapeHtml(L["Common_ChooseOne"] || '—')}</option>` +
            items.map(l => `<option value="${l.id}"${l.id === keep ? ' selected' : ''}>${AdminUI.escapeHtml(l.text)}</option>`).join('');

        if (keep && items.some(l => l.id === keep)) lessonSelect.value = keep;
        AdminUI.syncSelect(lessonSelect);
    };

    document.getElementById('subject_filter').addEventListener('change', () => window.filterQuizLessons(''));

    window.buildQuizForm = function () {
        const fd = new FormData();
        fd.append('lesson_id', document.getElementById('lesson_id').value);
        fd.append('title', document.getElementById('title').value);
        fd.append('duration_minutes', document.getElementById('duration_minutes').value || '1');
        const sortOrder = document.getElementById('sort_order').value;
        if (sortOrder !== '') fd.append('sort_order', sortOrder);
        fd.append('type', document.getElementById('type').value);
        fd.append('is_active', document.getElementById('is_active').checked ? 'true' : 'false');
        fd.append('is_repeat', document.getElementById('is_repeat').checked ? 'true' : 'false');
        return fd;
    };

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Quiz_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('sort_order').value = '';
        document.getElementById('subject_filter').value = '';
        window.filterQuizLessons('');
        document.getElementById('title').value = '';
        document.getElementById('duration_minutes').value = 1;
        document.getElementById('type').value = 'lesson';
        document.getElementById('is_active').checked = true;
        document.getElementById('is_repeat').checked = false;
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const res = await AdminUI.postForm('/quiz', window.buildQuizForm());
        if (res.ok) { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
