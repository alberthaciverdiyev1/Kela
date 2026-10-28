// Admin hər banneri idarə edir; teacher yalnız öz yaratdığını.
function canEdit(b) {
    const me = window.ADMIN || {};
    return me.isAdmin || (b.userId != null && b.userId === me.userId);
}

function ownerCell(b) {
    if (b.userId == null) return AdminUI.badge(L["Banner_OwnerAdmin"], 'neutral');
    if (b.userId === (window.ADMIN || {}).userId) return AdminUI.badge(L["Banner_OwnerYou"], 'success');
    return `<span class="text-sm text-slate-600">${AdminUI.escapeHtml(b.ownerName || ('#' + b.userId))}</span>`;
}

function setStatus(id, isActive) {
    AdminUI.setActive('/banner', id, isActive, () => loadList(pager.page));
}

AdminUI.listPage({
    api: '/banner/list',
    bulkUrl: '/banner/bulk',
    deleteBase: '/banner',
    confirmKey: 'Js_ConfirmDeleteBanner',
    columns: 6,
    selectable: true,
    selectableIds: canEdit,
    onLoaded: data => {
        const total = document.getElementById('stat_total');
        if (total) total.textContent = data.total;
    },
    row: (b, i, ctx) => [
        { html: AdminUI.rowNo(b, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        {
            html: b.image
                ? `<img src="${b.image}" class="h-10 w-16 rounded-lg border border-slate-200 object-cover" />`
                : '<span class="text-slate-400">—</span>'
        },
        { html: b.title, cls: 'font-semibold text-slate-800' },
        { html: ownerCell(b) },
        {
            html: ctx.showDeleted ? AdminUI.badge(L["Js_Deleted"], 'danger')
                : canEdit(b)
                    ? AdminUI.statusSwitch(b.isActive, `setStatus(${b.id}, ${!b.isActive})`)
                    : AdminUI.badge(b.isActive ? L["Common_Active"] : L["Common_Inactive"], b.isActive ? 'success' : 'neutral')
        },
        {
            html: b.activeUntil
                ? `<span class="text-sm text-slate-500">${String(b.activeUntil).slice(0, 10)}</span>`
                : `<span class="text-sm text-slate-400">${L["Banner_Always"]}</span>`
        },
        { html: ctx.showDeleted ? AdminUI.restoreButton(b.id) : (canEdit(b) ? AdminUI.actions(b.id) : ''), cls: 'text-right' }
    ]
});
