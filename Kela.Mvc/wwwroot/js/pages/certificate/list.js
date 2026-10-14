let currentPage = 1;
let lastPage = 1;
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/certificate/bulk', id, () => loadList(currentPage));
}

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/certificate/list?page=${currentPage}&search=${encodeURIComponent(search)}&is_deleted=${showDeleted ? 1 : 0}`);
    lastPage = data.lastPage;

    const total = document.getElementById('stat_total');
    if (total) total.textContent = data.total;
    document.getElementById('tableInfo').textContent = `Cəmi ${data.total} nəticə`;

    AdminUI.table('rows', data.data.map(c => [
        { html: c.id, cls: 'text-slate-400' },
        { html: c.title, cls: 'font-semibold text-slate-800' },
        { html: c.templateName },
        { html: c.signature
            ? `<img src="${c.signature}" class="h-10 w-16 rounded-lg border border-slate-200 bg-white object-contain" />`
            : '<span class="text-slate-400">—</span>' },
        { html: new Date(c.createdAt).toLocaleDateString('az'), cls: 'text-slate-500' },
        { html: showDeleted ? AdminUI.restoreButton(c.id) : AdminUI.actions(c.id), cls: 'text-right' }
    ]), 6, { rowIds: data.data.map(c => c.id), selectable: !showDeleted });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu sertifikat silinəcək.')) return;
    const res = await AdminUI.del(`/certificate/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
