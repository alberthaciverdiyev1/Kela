let currentPage = 1;
let lastPage = 1;

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    const type = document.getElementById('filter_type').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/user/list?page=${currentPage}&search=${encodeURIComponent(search)}&type=${type}`);
    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map((u, i) => [
        { html: AdminUI.rowNo(u, i, currentPage, data.perPage), cls: 'text-slate-400' },
        { html: `${u.name ?? ''} ${u.surname ?? ''}`.trim() || '-', cls: 'font-medium text-slate-800' },
        { html: u.email, cls: 'text-slate-500' },
        { html: u.phoneNumber ?? '-', cls: 'text-slate-500' },
        { html: u.type === 'superadmin' ? AdminUI.badge('SuperAdmin', 'success') : (u.type === 'admin' ? AdminUI.badge('Admin', 'primary') : AdminUI.badge(L["Role_Teacher"], 'neutral')) },
        { html: AdminUI.actions(u.id), cls: 'text-right' }
    ]), 6);

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteUser"])) return;
    const res = await AdminUI.del(`/user/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
