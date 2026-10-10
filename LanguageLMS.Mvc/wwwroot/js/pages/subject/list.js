let currentPage = 1;
let lastPage = 1;

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/subject/list?page=${currentPage}&search=${encodeURIComponent(search)}`);

    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map(s => [
        { html: s.id, cls: 'text-slate-400' },
        { html: s.name, cls: 'font-medium text-slate-800' },
        { html: AdminUI.actions(s.id), cls: 'text-right' }
    ]), 3);

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu fənn silinəcək.')) return;
    const res = await AdminUI.del(`/subject/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
