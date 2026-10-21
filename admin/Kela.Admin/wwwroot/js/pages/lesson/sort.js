window.sortClick = function () {
    AdminUI.openListSort({
        api: '/lesson/list',
        filterId: 'filter_subject',
        filterParam: 'subject_id',
        title: value => value ? L["Js_SortLessonsSubject"] : L["Js_SortLessons"],
        label: l => l.title,
        saveUrl: '/lesson/sort'
    });
};
