let currentPage = 1;
let lastPage = 1;

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/banner/list?page=${currentPage}&search=${encodeURIComponent(search)}`);
    lastPage = data.lastPage;

    const total = document.getElementById('stat_total');
    if (total) total.textContent = data.total;
    document.getElementById('tableInfo').textContent = `Cəmi ${data.total} nəticə`;

    AdminUI.table('rows', data.data.map(b => [
        { html: b.id, cls: 'text-slate-400' },
        { html: b.image
            ? `<img src="${b.image}" class="h-10 w-16 rounded-lg border border-slate-200 object-cover" />`
            : '<span class="text-slate-400">—</span>' },
        { html: b.title, cls: 'font-semibold text-slate-800' },
        { html: b.isNew ? AdminUI.badge('Yeni', 'primary') : AdminUI.badge('Köhnə', 'neutral') },
        { html: AdminUI.actions(b.id), cls: 'text-right' }
    ]), 5, { rowIds: data.data.map(b => b.id), selectable: true });

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
