AdminUI.listPage({
    api: '/certificate/list',
    bulkUrl: '/certificate/bulk',
    deleteBase: '/certificate',
    confirmKey: 'Js_ConfirmDeleteCertificate',
    columns: 6,
    selectable: true,
    onLoaded: data => {
        const total = document.getElementById('stat_total');
        if (total) total.textContent = data.total;
    },
    row: (c, i, ctx) => [
        { html: AdminUI.rowNo(c, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        { html: c.title, cls: 'font-semibold text-slate-800' },
        { html: c.templateName },
        { html: c.signature
            ? `<img src="${c.signature}" class="h-10 w-16 rounded-lg border border-slate-200 bg-white object-contain" />`
            : '<span class="text-slate-400">—</span>' },
        { html: new Date(c.createdAt).toLocaleDateString('az'), cls: 'text-slate-500' },
        { html: ctx.showDeleted ? AdminUI.restoreButton(c.id) : AdminUI.actions(c.id), cls: 'text-right' }
    ]
});
