function copyJoinLink(token) {
    if (!token) return;
    const base = (window.GROUP_APP_BASE_URL || location.origin).replace(/\/$/, '');
    const url = base + '/join/' + token;

    AdminUI.copyText(url).then(ok => {
        if (ok) AdminUI.notify(L["Js_LinkCopied"]);
    });
}

AdminUI.listPage({
    api: '/group/list',
    bulkUrl: '/group/bulk',
    deleteBase: '/group',
    confirmKey: 'Js_ConfirmDeleteGroup',
    columns: 5,
    selectable: true,
    row: (g, i, ctx) => [
        { html: AdminUI.rowNo(g, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        { html: `${g.name}${g.pendingJoins > 0
            ? ` <span class="ml-2 inline-flex min-w-5 items-center justify-center rounded-full bg-danger px-1.5 py-0.5 text-[10px] font-bold leading-none text-white">${g.pendingJoins}</span>`
            : ''}`, cls: 'font-medium text-slate-800' },
        { html: window.TEACHERS[g.userId] ?? '-', cls: 'text-slate-500' },
        { html: g.studentCount, cls: 'text-slate-500' },
        { html: ctx.showDeleted ? AdminUI.restoreButton(g.id) : AdminUI.rowActions([
            AdminUI.actionLink(L["Common_Manage"], `/group/${g.id}/manage`),
            AdminUI.actionButton(L["Group_CopyJoinLink"], `copyJoinLink('${g.joinToken}')`, { tone: 'neutral' }),
            AdminUI.actionButton(L["Js_Edit"], `openEdit(${g.id})`, { tone: 'primary' }),
            AdminUI.actionButton(L["Common_Delete"], `removeItem(${g.id})`, { tone: 'danger' })
        ]), cls: 'text-right' }
    ]
});
