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
        { html: `${AdminUI.escapeHtml(g.name)}${g.pendingJoins > 0
            ? ` <span class="ml-2 inline-flex min-w-5 items-center justify-center rounded-full bg-danger px-1.5 py-0.5 text-[10px] font-bold leading-none text-white">${g.pendingJoins}</span>`
            : ''}`, cls: 'font-medium text-slate-800' },
        { html: AdminUI.escapeHtml(window.TEACHERS[g.userId] ?? '-'), cls: 'text-slate-500' },
        { html: g.studentCount, cls: 'text-slate-500' },
        { html: ctx.showDeleted ? AdminUI.restoreButton(g.id) : AdminUI.rowActions([
            AdminUI.actionLinkIcon(`/group/${g.id}/manage`,
                '<path stroke-linecap="round" stroke-linejoin="round" d="M10.3 4.3a2 2 0 0 1 3.4 0l.5.9 1-.3a2 2 0 0 1 2.5 2.5l-.3 1 .9.5a2 2 0 0 1 0 3.4l-.9.5.3 1a2 2 0 0 1-2.5 2.5l-1-.3-.5.9a2 2 0 0 1-3.4 0l-.5-.9-1 .3a2 2 0 0 1-2.5-2.5l.3-1-.9-.5a2 2 0 0 1 0-3.4l.9-.5-.3-1a2 2 0 0 1 2.5-2.5l1 .3.5-.9Z" /><circle cx="12" cy="12" r="2.6" />',
                L["Common_Manage"]),
            AdminUI.actionLinkIcon(`/homework?group_id=${g.id}`,
                '<path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2M9 5a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2M9 5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2m-6 9 2 2 4-4" />',
                L["Nav_Homework"]),
            AdminUI.actionIcon(
                '<path stroke-linecap="round" stroke-linejoin="round" d="M13.19 8.688a4.5 4.5 0 0 1 1.242 7.244l-4.5 4.5a4.5 4.5 0 0 1-6.364-6.364l1.757-1.757m13.35-.622 1.757-1.757a4.5 4.5 0 0 0-6.364-6.364l-4.5 4.5a4.5 4.5 0 0 0 1.242 7.244" />',
                L["Group_CopyJoinLink"], `copyJoinLink('${g.joinToken}')`),
            AdminUI.actionButton(L["Js_Edit"], `openEdit(${g.id})`, { tone: 'primary' }),
            AdminUI.actionButton(L["Common_Delete"], `removeItem(${g.id})`, { tone: 'danger' })
        ]), cls: 'text-right' }
    ]
});
