let currentPage = 1;
let lastPage = 1;

async function loadList(page) {
    currentPage = page || 1;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/notification/list?page=${currentPage}`);
    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map(n => [
        { html: n.id, cls: 'text-slate-400' },
        { html: n.title, cls: 'font-medium text-slate-800' },
        { html: n.body ?? '-', cls: 'text-slate-500' },
        { html: n.all ? '<span class="rounded-full bg-primary/10 px-2 py-0.5 text-xs text-primary">Hamıya</span>' : '<span class="rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-500">Seçilmiş</span>' },
        { html: n.image ? `<img src="${n.image}" class="h-8 w-12 rounded border border-slate-200 object-cover" />` : '-', cls: 'text-slate-500' },
        { html: new Date(n.createdAt).toLocaleString('az'), cls: 'text-slate-400 text-xs' },
        { html: `<div class="flex justify-end"><button type="button" class="px-2 py-1 text-red-600 hover:bg-red-50 rounded-lg" onclick="removeItem(${n.id})">Sil</button></div>`, cls: 'text-right' }
    ]), 7);

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu bildiriş silinəcək.')) return;
    const res = await AdminUI.del(`/notification/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
