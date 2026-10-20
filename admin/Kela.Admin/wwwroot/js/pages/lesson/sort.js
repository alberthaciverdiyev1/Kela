window.sortClick = async function () {
    const subjectId = document.getElementById('filter_subject').value;
    const params = new URLSearchParams({ page: '1', perPage: '500' });
    if (subjectId) params.set('subject_id', subjectId);

    const data = await AdminUI.get(`/lesson/list?${params.toString()}`);
    AdminUI.openSort({
        title: subjectId ? L["Js_SortLessonsSubject"] : L["Js_SortLessons"],
        items: (data.data || []).map(l => ({ id: l.id, label: l.title })),
        saveUrl: '/lesson/sort',
        onSaved: () => loadList(pager.page)
    });
};
