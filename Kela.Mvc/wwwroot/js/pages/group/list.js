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
    const data = await AdminUI.get(`/group/list?page=${currentPage}&search=${encodeURIComponent(search)}&is_deleted=${showDeleted ? 1 : 0}`);
    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map((g, i) => [
        { html: AdminUI.rowNo(g, i, currentPage, data.perPage), cls: 'text-slate-400' },
        { html: g.name, cls: 'font-medium text-slate-800' },
        { html: window.TEACHERS[g.userId] ?? '-', cls: 'text-slate-500' },
        { html: g.studentCount, cls: 'text-slate-500' },
        { html: showDeleted ? AdminUI.restoreButton(g.id) : `<div class="flex items-center justify-end gap-2">
            <button type="button" class="px-2 py-1 text-navy hover:bg-slate-100 rounded-lg" onclick="openManage(${g.id})">İdarə</button>
            <button type="button" class="px-2 py-1 text-primary hover:bg-primary/10 rounded-lg" onclick="openEdit(${g.id})">Redaktə</button>
            <button type="button" class="px-2 py-1 text-red-600 hover:bg-red-50 rounded-lg" onclick="removeItem(${g.id})">Sil</button>
        </div>`, cls: 'text-right' }
    ]), 5, { rowIds: data.data.map(g => g.id), selectable: !showDeleted });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu qrup silinəcək.')) return;
    const res = await AdminUI.del(`/group/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
