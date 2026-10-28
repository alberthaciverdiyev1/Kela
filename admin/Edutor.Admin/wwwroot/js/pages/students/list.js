function studentActions(s, showDeleted) {
    if (showDeleted) {
        return AdminUI.rowActions([AdminUI.actionButton(L["Common_Restore"], `restoreRow(${s.id})`, { tone: 'primary' })]);
    }
    return AdminUI.rowActions([
        AdminUI.actionLink(L["Js_Details"], `/students/${s.id}/detail`),
        AdminUI.actionButton(s.isActive ? L["Js_Deactivate"] : L["Js_Activate"], `toggleStatus(${s.id}, ${!s.isActive})`, { tone: 'primary' }),
        AdminUI.actionButton(L["Students_Devices"], `openDevices(${s.id})`),
        AdminUI.actionIcon(
            '<rect x="5" y="11" width="14" height="10" rx="2" /><path d="M8 11V7a4 4 0 0 1 8 0v4" />',
            L["Students_ResetPassword"], `resetStudentPassword(${s.id})`),
        AdminUI.actionButton(L["Common_Delete"], `removeItem(${s.id})`, { tone: 'danger' })
    ]);
}

async function toggleStatus(id, isActive) {
    const res = await AdminUI.postJson(`/students/${id}/status`, { isActive });
    if (res.ok) { AdminUI.notify(L["Js_StatusChanged"]); loadList(pager.page); }
    else AdminUI.error();
}

AdminUI.listPage({
    api: '/students/list',
    bulkUrl: '/students/bulk',
    deleteBase: '/students',
    confirmKey: 'Js_ConfirmDeleteStudent',
    columns: 6,
    selectable: true,
    params: ({ page, search, showDeleted }) => ({
        page,
        search,
        is_active: document.getElementById('filter_active').value,
        is_deleted: showDeleted ? 1 : 0
    }),
    restore: async (id, reload) => {
        if (!await AdminUI.confirmRestore()) return;
        const res = await AdminUI.postJson(`/students/${id}/restore`, {});
        if (res.ok) { AdminUI.notify(L["Js_Restored"]); reload(); }
        else AdminUI.error(AdminUI.errText(res));
    },
    row: (s, i, ctx) => [
        { html: AdminUI.rowNo(s, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        { html: s.fullName, cls: 'font-medium text-slate-800' },
        { html: s.email, cls: 'text-slate-500' },
        { html: s.phone ?? '-', cls: 'text-slate-500' },
        { html: ctx.showDeleted
            ? AdminUI.badge(L["Js_Deleted"], 'danger')
            : AdminUI.statusSwitch(s.isActive, `toggleStatus(${s.id}, ${!s.isActive})`) },
        { html: studentActions(s, ctx.showDeleted), cls: 'text-right' }
    ]
});
