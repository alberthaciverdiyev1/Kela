window.sortClick = function () {
    AdminUI.openListSort({
        api: '/quiz/list',
        filterId: 'filter_lesson',
        filterParam: 'lesson_id',
        title: L["Js_SortQuizzes"],
        label: q => q.title,
        saveUrl: '/quiz/sort'
    });
};
