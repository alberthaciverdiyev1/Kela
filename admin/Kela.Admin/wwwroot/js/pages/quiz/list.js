function quizActions(q) {
    return AdminUI.rowActions([
        AdminUI.actionButton(L["Common_Questions"], `openQuestions(${q.id})`),
        AdminUI.actionButton(L["Js_Edit"], `openEdit(${q.id})`, { tone: 'primary' }),
        AdminUI.actionButton(L["Common_Delete"], `removeItem(${q.id})`, { tone: 'danger' })
    ]);
}

async function setStatus(id, isActive) {
    const d = await AdminUI.get(`/quiz/${id}`);
    const fd = new FormData();
    fd.append('lesson_id', d.lessonId ?? '');
    fd.append('title', d.title);
    fd.append('type', d.type);
    fd.append('duration_minutes', d.durationMinutes ?? 0);
    fd.append('sort_order', d.sortOrder ?? 0);
    fd.append('is_repeat', String(!!d.isRepeat));
    fd.append('is_active', String(isActive));

    const res = await AdminUI.putForm(`/quiz/${id}`, fd);
    if (res.ok) { AdminUI.notify(L["Js_StatusChanged"]); loadList(pager.page); }
    else AdminUI.error(AdminUI.errText(res));
}

AdminUI.listPage({
    api: '/quiz/list',
    bulkUrl: '/quiz/bulk',
    deleteBase: '/quiz',
    confirmKey: 'Js_ConfirmDeleteQuiz',
    columns: 8,
    selectable: true,
    params: ({ page, search, showDeleted }) => ({
        page,
        search,
        lesson_id: document.getElementById('filter_lesson').value,
        is_deleted: showDeleted ? 1 : 0
    }),
    row: (q, i, ctx) => [
        { html: AdminUI.rowNo(q, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        { html: q.lessonId ? (window.LESSONS[q.lessonId] ?? '-') : '-', cls: 'text-slate-500' },
        { html: q.title, cls: 'font-medium text-slate-800' },
        { html: AdminUI.badge(q.type === 'general' ? L["Common_General"] : L["Common_Lesson"], 'neutral') },
        { html: q.durationMinutes, cls: 'text-slate-500' },
        { html: q.questionCount, cls: 'text-slate-500' },
        { html: AdminUI.statusSwitch(q.isActive, `setStatus(${q.id}, ${!q.isActive})`) },
        { html: ctx.showDeleted ? AdminUI.restoreButton(q.id) : quizActions(q), cls: 'text-right' }
    ]
});
