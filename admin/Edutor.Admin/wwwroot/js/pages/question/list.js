AdminUI.listPage({
    api: '/question/list',
    bulkUrl: '/question/bulk',
    deleteBase: '/question',
    confirmKey: 'Js_ConfirmDeleteQuestion',
    columns: 5,
    selectable: true,
    params: ({ page, search, showDeleted }) => ({
        page,
        search,
        subject_id: document.getElementById('filter_subject').value,
        is_deleted: showDeleted ? 1 : 0
    }),
    row: (q, i, ctx) => [
        { html: AdminUI.rowNo(q, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        { html: AdminUI.escapeHtml(window.SUBJECTS[q.subjectId] ?? '-'), cls: 'text-slate-500' },
        { html: q.titleType === 'image' && q.titleUrl
                ? `<img src="${AdminUI.escapeHtml(q.titleUrl)}" class="h-8 w-12 rounded border border-slate-200 object-cover" />`
                : AdminUI.escapeHtml(q.title ?? '-'), cls: 'font-medium text-slate-800' },
        { html: `<span class="rounded bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">${AdminUI.escapeHtml(q.correctAnswer)}</span>` },
        { html: ctx.showDeleted ? AdminUI.restoreButton(q.id) : AdminUI.actions(q.id), cls: 'text-right' }
    ]
});
