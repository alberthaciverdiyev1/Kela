AdminUI.listPage({
    api: '/teacher-note/list',
    deleteBase: '/teacher-note',
    confirmKey: 'Js_AreYouSure',
    columns: 5,
    row: (c, i, ctx) => [
        { html: AdminUI.rowNo(c, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        { html: AdminUI.escapeHtml(c.title), cls: 'font-semibold text-slate-800' },
        { html: c.date ? AdminUI.escapeHtml(c.date) : '<span class="text-slate-400">—</span>', cls: 'text-slate-500' },
        {
            html: c.isDone
                ? AdminUI.badge(AdminUI.escapeHtml(L["Note_Done"]), 'success')
                : AdminUI.badge(AdminUI.escapeHtml(L["Note_Pending"]), 'warning')
        },
        { html: AdminUI.actions(c.id), cls: 'text-right' }
    ]
});
