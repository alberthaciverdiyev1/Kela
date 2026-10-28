window.sortClick = function () {
    AdminUI.openListSort({
        api: '/lesson-content/list',
        filterId: 'filter_lesson',
        filterParam: 'lesson_id',
        requireFilter: 'Js_SelectLessonFirst',
        title: L["Js_SortMaterials"],
        label: c => c.title,
        saveUrl: '/lesson-content/sort'
    });
};
