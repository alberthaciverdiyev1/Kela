var pager = AdminUI.createPager('pagination', loadList);

async function loadList(page) {
    var pageNo = page || pager.page;
    const search = document.getElementById('search').value;
    const type = document.getElementById('filter_type').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/user/list?page=${pageNo}&search=${encodeURIComponent(search)}&type=${type}`));
    pager.apply(data);

    AdminUI.table('rows', data.data.map((u, i) => [
        { html: AdminUI.rowNo(u, i, pager.page, pager.perPage), cls: 'text-slate-400' },
        { html: `${u.name ?? ''} ${u.surname ?? ''}`.trim() || '-', cls: 'font-medium text-slate-800' },
        { html: u.email, cls: 'text-slate-500' },
        { html: u.phoneNumber ?? '-', cls: 'text-slate-500' },
        { html: u.type === 'superadmin' ? AdminUI.badge('SuperAdmin', 'success') : (u.type === 'admin' ? AdminUI.badge('Admin', 'primary') : AdminUI.badge(L["Role_Teacher"], 'neutral')) },
        { html: AdminUI.actions(u.id), cls: 'text-right' }
    ]), 6);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteUser"])) return;
    const res = await AdminUI.del(`/user/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(pager.page); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
