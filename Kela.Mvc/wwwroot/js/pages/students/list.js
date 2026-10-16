let currentPage = 1;
let lastPage = 1;
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

function studentActions(s) {
    if (showDeleted) {
        return AdminUI.rowActions([AdminUI.actionButton(L["Common_Restore"], `restoreRow(${s.id})`, { tone: 'primary' })]);
    }
    return AdminUI.rowActions([
        AdminUI.actionLink(L["Js_Details"], `/students/${s.id}/detail`),
        AdminUI.actionButton(s.isActive ? L["Js_Deactivate"] : L["Js_Activate"], `toggleStatus(${s.id}, ${!s.isActive})`, { tone: 'primary' }),
        AdminUI.actionButton(L["Students_Devices"], `openDevices(${s.id})`),
        AdminUI.actionButton(L["Common_Delete"], `removeItem(${s.id})`, { tone: 'danger' })
    ]);
}

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    const active = document.getElementById('filter_active').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/students/list?page=${currentPage}&search=${encodeURIComponent(search)}&is_active=${active}&is_deleted=${showDeleted ? 1 : 0}`));
    lastPage = data.lastPage;
    AdminUI.setInfo(data.total);

    AdminUI.table('rows', data.data.map((s, i) => [
        { html: AdminUI.rowNo(s, i, currentPage, data.perPage), cls: 'text-slate-400' },
        { html: s.fullName, cls: 'font-medium text-slate-800' },
        { html: s.email, cls: 'text-slate-500' },
        { html: s.phone ?? '-', cls: 'text-slate-500' },
        { html: showDeleted ? AdminUI.badge(L["Js_Deleted"], 'danger') : (s.isActive ? AdminUI.badge(L["Common_Active"], 'success') : AdminUI.badge(L["Common_Archived"], 'neutral')) },
        { html: studentActions(s), cls: 'text-right' }
    ]), 6, { rowIds: data.data.map(s => s.id), selectable: !showDeleted });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function restoreRow(id) {
    if (!await AdminUI.confirmRestore()) return;
    const res = await AdminUI.postJson(`/students/${id}/restore`, {});
    if (res.ok) { AdminUI.notify(L["Js_Restored"]); loadList(currentPage); }
    else AdminUI.error(AdminUI.errText(res));
}

async function toggleStatus(id, isActive) {
    const res = await AdminUI.postJson(`/students/${id}/status`, { isActive });
    if (res.ok) { AdminUI.notify(L["Js_StatusChanged"]); loadList(currentPage); }
    else AdminUI.error();
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteStudent"])) return;
    const res = await AdminUI.del(`/students/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
