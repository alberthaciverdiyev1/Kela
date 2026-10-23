AdminUI.listPage({
    api: '/subject/list',
    bulkUrl: '/subject/bulk',
    deleteBase: '/subject',
    confirmKey: 'Js_ConfirmDeleteSubject',
    columns: 4,
    selectable: true,
    onLoaded: data => {
        const total = document.getElementById('stat_total');
        if (total) total.textContent = data.total;
    },
    row: (s, i, ctx) => [
        { html: AdminUI.rowNo(s, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        { html: `<a href="/lesson?subject_id=${s.id}" class="font-semibold text-primary hover:underline">${AdminUI.escapeHtml(s.name)}</a>` +
            (s.isShared ? ` <span class="ml-2 rounded-full bg-primary/10 px-2 py-0.5 text-[11px] font-semibold text-primary">${L["Subject_SharedBadge"]}</span>` : '') },
        { html: ctx.showDeleted ? AdminUI.restoreButton(s.id) : AdminUI.actions(s.id, { extra: AdminUI.actionLink(L["Nav_Lessons"], `/lesson?subject_id=${s.id}`) }), cls: 'text-right' }
    ]
});
