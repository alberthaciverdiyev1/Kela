AdminUI.listPage({
    api: '/notification/list',
    deleteBase: '/notification',
    confirmKey: 'Js_ConfirmDeleteNotification',
    columns: 7,
    row: (n, i, ctx) => [
        { html: AdminUI.rowNo(n, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        { html: n.title, cls: 'font-medium text-slate-800' },
        { html: n.body ?? '-', cls: 'text-slate-500' },
        { html: n.all ? AdminUI.badge(L["Js_All"], 'primary') : AdminUI.badge(L["Js_Selected"], 'neutral') },
        { html: n.image ? `<img src="${n.image}" class="h-8 w-12 rounded border border-slate-200 object-cover" />` : '-', cls: 'text-slate-500' },
        { html: new Date(n.createdAt).toLocaleString('az'), cls: 'text-slate-400 text-xs' },
        { html: `<div class="flex justify-end"><button type="button" class="px-2 py-1 text-red-600 hover:bg-red-50 rounded-lg" onclick="removeItem(${n.id})">Sil</button></div>`, cls: 'text-right' }
    ]
});
