let currentPage = 1;
let lastPage = 1;

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    const type = document.getElementById('filter_type').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/user/list?page=${currentPage}&search=${encodeURIComponent(search)}&type=${type}`);
    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map(u => [
        { html: u.id, cls: 'text-slate-400' },
        { html: `${u.name ?? ''} ${u.surname ?? ''}`.trim() || '-', cls: 'font-medium text-slate-800' },
        { html: u.email, cls: 'text-slate-500' },
        { html: u.phoneNumber ?? '-', cls: 'text-slate-500' },
        { html: u.type === 'admin' ? '<span class="rounded-full bg-primary/10 px-2 py-0.5 text-xs text-primary">Admin</span>' : '<span class="rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-600">Müəllim</span>' },
        { html: AdminUI.actions(u.id), cls: 'text-right' }
    ]), 6);

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu istifadəçi silinəcək.')) return;
    const res = await AdminUI.del(`/user/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
