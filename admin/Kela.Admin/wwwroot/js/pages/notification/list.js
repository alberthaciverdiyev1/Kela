var pager = AdminUI.createPager('pagination', loadList);

async function loadList(page) {
    var pageNo = page || pager.page;
    const search = document.getElementById('search')?.value ?? '';
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/notification/list?page=${pageNo}&search=${encodeURIComponent(search)}`));
    pager.apply(data);

    AdminUI.table('rows', data.data.map((n, i) => [
        { html: AdminUI.rowNo(n, i, pager.page, pager.perPage), cls: 'text-slate-400' },
        { html: n.title, cls: 'font-medium text-slate-800' },
        { html: n.body ?? '-', cls: 'text-slate-500' },
        { html: n.all ? AdminUI.badge(L["Js_All"], 'primary') : AdminUI.badge(L["Js_Selected"], 'neutral') },
        { html: n.image ? `<img src="${n.image}" class="h-8 w-12 rounded border border-slate-200 object-cover" />` : '-', cls: 'text-slate-500' },
        { html: new Date(n.createdAt).toLocaleString('az'), cls: 'text-slate-400 text-xs' },
        { html: `<div class="flex justify-end"><button type="button" class="px-2 py-1 text-red-600 hover:bg-red-50 rounded-lg" onclick="removeItem(${n.id})">Sil</button></div>`, cls: 'text-right' }
    ]), 7);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteNotification"])) return;
    const res = await AdminUI.del(`/notification/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(pager.page); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
