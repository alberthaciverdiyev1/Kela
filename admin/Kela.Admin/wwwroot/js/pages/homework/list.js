function homeworkStatusBadge(h) {
    return h.isPublished
        ? AdminUI.badge(L["Homework_Published"], 'success')
        : AdminUI.badge(L["Homework_Draft"], 'neutral');
}

function formatDue(value) {
    if (!value) return '—';
    return new Date(value).toLocaleString('az', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}

window.publishHomework = async function (id) {
    const result = await AdminUI.swal({
        title: L["Homework_Publish"],
        text: L["Js_ConfirmPublishHomework"],
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: L["Homework_Publish"],
        cancelButtonText: L["Common_Cancel"]
    });
    if (!result.isConfirmed) return;

    const res = await AdminUI.postForm(`/homework/${id}/publish`, new FormData());
    if (res.ok) { AdminUI.notify(L["Js_Published"]); loadList(pager.page); }
    else AdminUI.error(AdminUI.errText(res));
};

AdminUI.listPage({
    api: '/homework/list',
    deleteBase: '/homework',
    confirmKey: 'Js_ConfirmDeleteHomework',
    columns: 7,
    params: ({ page, search }) => ({
        page,
        search,
        is_published: document.getElementById('filter_status').value
    }),
    row: (h, i, ctx) => [
        { html: AdminUI.rowNo(h, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
        { html: `<span class="font-semibold text-slate-900">${AdminUI.escapeHtml(h.title)}</span>${h.quizzes.length ? `<div class="mt-0.5 text-xs text-slate-400">${AdminUI.escapeHtml(h.quizzes.map(q => q.title).join(', '))}</div>` : ''}` },
        { html: AdminUI.escapeHtml(h.groups.map(g => g.name).join(', ') || '—'), cls: 'text-slate-500' },
        { html: formatDue(h.dueDate), cls: 'text-slate-500 text-sm' },
        { html: h.isPublished ? `${h.submittedCount + h.gradedCount + h.returnedCount} / ${h.assignedCount}` : '—', cls: 'text-slate-500' },
        { html: homeworkStatusBadge(h) },
        {
            html: AdminUI.rowActions([
                h.isPublished ? AdminUI.actionLink(L["Homework_Manage"], `/homework/${h.id}/manage`) : null,
                !h.isPublished ? AdminUI.actionButton(L["Homework_Publish"], `publishHomework(${h.id})`, { tone: 'primary' }) : null,
                AdminUI.actionButton(L["Js_Edit"], `openEdit(${h.id})`, { tone: 'primary' }),
                AdminUI.actionButton(L["Common_Delete"], `removeItem(${h.id})`, { tone: 'danger' })
            ]), cls: 'text-right'
        }
    ]
});
