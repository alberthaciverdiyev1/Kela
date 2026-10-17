window.sortClick = async function () {
    const lessonId = document.getElementById('filter_lesson').value;
    const params = new URLSearchParams({ page: '1', perPage: '500' });
    if (lessonId) params.set('lesson_id', lessonId);

    const data = await AdminUI.get(`/quiz/list?${params.toString()}`);
    AdminUI.openSort({
        title: L["Js_SortQuizzes"],
        items: (data.data || []).map(q => ({ id: q.id, label: q.title })),
        saveUrl: '/quiz/sort',
        onSaved: () => loadList(currentPage)
    });
};
