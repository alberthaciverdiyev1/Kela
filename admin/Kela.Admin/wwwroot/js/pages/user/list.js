AdminUI.listPage({
    api: '/user/list',
    deleteBase: '/user',
    confirmKey: 'Js_ConfirmDeleteUser',
    columns: 6,
    params: ({ page, search }) => ({
        page,
        search,
        type: document.getElementById('filter_type').value
    }),
    row: (u, i, ctx) => [
        { html: AdminUI.rowNo(u, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        { html: `${u.name ?? ''} ${u.surname ?? ''}`.trim() || '-', cls: 'font-medium text-slate-800' },
        { html: u.email, cls: 'text-slate-500' },
        { html: u.phoneNumber ?? '-', cls: 'text-slate-500' },
        { html: u.type === 'superadmin' ? AdminUI.badge('SuperAdmin', 'success') : (u.type === 'admin' ? AdminUI.badge('Admin', 'primary') : AdminUI.badge(L["Role_Teacher"], 'neutral')) },
        { html: AdminUI.actions(u.id), cls: 'text-right' }
    ]
});
