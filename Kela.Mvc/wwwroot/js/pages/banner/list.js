let currentPage = 1;
let lastPage = 1;
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/banner/bulk', id, () => loadList(currentPage));
}

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/banner/list?page=${currentPage}&search=${encodeURIComponent(search)}&is_deleted=${showDeleted ? 1 : 0}`);
    lastPage = data.lastPage;

    const total = document.getElementById('stat_total');
    if (total) total.textContent = data.total;
    document.getElementById('tableInfo').textContent = `Cəmi ${data.total} nəticə`;

    AdminUI.table('rows', data.data.map((b, i) => [
        { html: AdminUI.rowNo(b, i, currentPage, data.perPage), cls: 'text-slate-400' },
        { html: b.image
            ? `<img src="${b.image}" class="h-10 w-16 rounded-lg border border-slate-200 object-cover" />`
            : '<span class="text-slate-400">—</span>' },
        { html: b.title, cls: 'font-semibold text-slate-800' },
        { html: b.isNew ? AdminUI.badge('Yeni', 'primary') : AdminUI.badge('Köhnə', 'neutral') },
        { html: showDeleted ? AdminUI.restoreButton(b.id) : AdminUI.actions(b.id), cls: 'text-right' }
    ]), 5, { rowIds: data.data.map(b => b.id), selectable: !showDeleted });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu banner silinəcək.')) return;
    const res = await AdminUI.del(`/banner/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
