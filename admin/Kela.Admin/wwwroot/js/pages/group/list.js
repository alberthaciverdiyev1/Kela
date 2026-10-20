var pager = AdminUI.createPager('pagination', loadList);
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/group/bulk', id, () => loadList(pager.page));
}

async function loadList(page) {
    var pageNo = page || pager.page;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/group/list?page=${pageNo}&search=${encodeURIComponent(search)}&is_deleted=${showDeleted ? 1 : 0}`));
    pager.apply(data);

    AdminUI.table('rows', data.data.map((g, i) => [
        { html: AdminUI.rowNo(g, i, pager.page, pager.perPage), cls: 'text-slate-400' },
        { html: `${g.name}${g.pendingJoins > 0
            ? ` <span class="ml-2 inline-flex min-w-5 items-center justify-center rounded-full bg-danger px-1.5 py-0.5 text-[10px] font-bold leading-none text-white">${g.pendingJoins}</span>`
            : ''}`, cls: 'font-medium text-slate-800' },
        { html: window.TEACHERS[g.userId] ?? '-', cls: 'text-slate-500' },
        { html: g.studentCount, cls: 'text-slate-500' },
        { html: showDeleted ? AdminUI.restoreButton(g.id) : AdminUI.rowActions([
            AdminUI.actionLink(L["Common_Manage"], `/group/${g.id}/manage`),
            AdminUI.actionButton(L["Group_CopyJoinLink"], `copyJoinLink('${g.joinToken}')`, { tone: 'neutral' }),
            AdminUI.actionButton(L["Js_Edit"], `openEdit(${g.id})`, { tone: 'primary' }),
            AdminUI.actionButton(L["Common_Delete"], `removeItem(${g.id})`, { tone: 'danger' })
        ]), cls: 'text-right' }
    ]), 5, { rowIds: data.data.map(g => g.id), selectable: !showDeleted });
    AdminUI.loader(false);
}

function copyJoinLink(token) {
    if (!token) return;
    const base = (window.GROUP_APP_BASE_URL || location.origin).replace(/\/$/, '');
    const url = base + '/join/' + token;

    AdminUI.copyText(url).then(ok => {
        if (ok) AdminUI.notify(L["Js_LinkCopied"]);
    });
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteGroup"])) return;
    const res = await AdminUI.del(`/group/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(pager.page); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
