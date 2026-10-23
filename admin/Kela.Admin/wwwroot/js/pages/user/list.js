// Öz hesabını və (superadmin deyilsə) admin hesablarını silmək olmaz.
function canDeleteUser(u) {
    if (window.ADMIN && ADMIN.userId === u.id) return false;
    if (window.ADMIN && ADMIN.isSuperAdmin) return true;
    return u.type === 'teacher';
}

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
        {
            html: AdminUI.rowActions([
                AdminUI.actionButton(L["Js_Edit"], `openEdit(${u.id})`, { tone: 'primary' }),
                canDeleteUser(u)
                    ? AdminUI.actionButton(L["Common_Delete"], `removeItem(${u.id})`, { tone: 'danger' })
                    : null
            ]), cls: 'text-right'
        }
    ]
});
