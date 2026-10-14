window.sortClick = async function () {
    const lessonId = document.getElementById('filter_lesson').value;
    if (!lessonId) { AdminUI.notify('Əvvəlcə dərs seçin', 'warning'); return; }

    const data = await AdminUI.get(`/lesson-content/list?lesson_id=${lessonId}&page=1&perPage=500`);
    AdminUI.openSort({
        title: 'Materialları sırala',
        items: (data.data || []).map(c => ({ id: c.id, label: c.title })),
        saveUrl: '/lesson-content/sort',
        onSaved: () => loadList(currentPage)
    });
};
