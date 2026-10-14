let currentPage = 1;
let lastPage = 1;

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/group/list?page=${currentPage}&search=${encodeURIComponent(search)}`);
    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map(g => [
        { html: g.id, cls: 'text-slate-400' },
        { html: g.name, cls: 'font-medium text-slate-800' },
        { html: window.TEACHERS[g.userId] ?? '-', cls: 'text-slate-500' },
        { html: g.studentCount, cls: 'text-slate-500' },
        { html: `<div class="flex items-center justify-end gap-2">
            <button type="button" class="px-2 py-1 text-navy hover:bg-slate-100 rounded-lg" onclick="openManage(${g.id})">İdarə</button>
            <button type="button" class="px-2 py-1 text-primary hover:bg-primary/10 rounded-lg" onclick="openEdit(${g.id})">Redaktə</button>
            <button type="button" class="px-2 py-1 text-red-600 hover:bg-red-50 rounded-lg" onclick="removeItem(${g.id})">Sil</button>
        </div>`, cls: 'text-right' }
    ]), 5, { rowIds: data.data.map(g => g.id), selectable: true });

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
