let currentPage = 1;
let lastPage = 1;
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/group/bulk', id, () => loadList(currentPage));
}

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/group/list?page=${currentPage}&search=${encodeURIComponent(search)}&is_deleted=${showDeleted ? 1 : 0}`));
    lastPage = data.lastPage;
    AdminUI.setInfo(data.total);

    AdminUI.table('rows', data.data.map((g, i) => [
        { html: AdminUI.rowNo(g, i, currentPage, data.perPage), cls: 'text-slate-400' },
        { html: g.name, cls: 'font-medium text-slate-800' },
        { html: window.TEACHERS[g.userId] ?? '-', cls: 'text-slate-500' },
        { html: g.studentCount, cls: 'text-slate-500' },
        { html: showDeleted ? AdminUI.restoreButton(g.id) : AdminUI.rowActions([
            AdminUI.actionButton(L["Common_Manage"], `openManage(${g.id})`),
            AdminUI.actionButton(L["Js_Edit"], `openEdit(${g.id})`, { tone: 'primary' }),
            AdminUI.actionButton(L["Common_Delete"], `removeItem(${g.id})`, { tone: 'danger' })
        ]), cls: 'text-right' }
    ]), 5, { rowIds: data.data.map(g => g.id), selectable: !showDeleted });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteGroup"])) return;
    const res = await AdminUI.del(`/group/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
