var pager = AdminUI.createPager('pagination', loadList);
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/certificate/bulk', id, () => loadList(pager.page));
}

async function loadList(page) {
    var pageNo = page || pager.page;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/certificate/list?page=${pageNo}&search=${encodeURIComponent(search)}&is_deleted=${showDeleted ? 1 : 0}`));
    pager.apply(data);

    const total = document.getElementById('stat_total');
    if (total) total.textContent = data.total;

    AdminUI.table('rows', data.data.map((c, i) => [
        { html: AdminUI.rowNo(c, i, pager.page, pager.perPage), cls: 'text-slate-400' },
        { html: c.title, cls: 'font-semibold text-slate-800' },
        { html: c.templateName },
        { html: c.signature
            ? `<img src="${c.signature}" class="h-10 w-16 rounded-lg border border-slate-200 bg-white object-contain" />`
            : '<span class="text-slate-400">—</span>' },
        { html: new Date(c.createdAt).toLocaleDateString('az'), cls: 'text-slate-500' },
        { html: showDeleted ? AdminUI.restoreButton(c.id) : AdminUI.actions(c.id), cls: 'text-right' }
    ]), 6, { rowIds: data.data.map(c => c.id), selectable: !showDeleted });
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteCertificate"])) return;
    const res = await AdminUI.del(`/certificate/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(pager.page); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
