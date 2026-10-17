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
    const data = await AdminUI.get(AdminUI.addSort(`/banner/list?page=${currentPage}&search=${encodeURIComponent(search)}&is_deleted=${showDeleted ? 1 : 0}`));
    lastPage = data.lastPage;

    const total = document.getElementById('stat_total');
    if (total) total.textContent = data.total;
    AdminUI.setInfo(data.total);

    AdminUI.table('rows', data.data.map((b, i) => [
        { html: AdminUI.rowNo(b, i, currentPage, data.perPage), cls: 'text-slate-400' },
        { html: b.image
            ? `<img src="${b.image}" class="h-10 w-16 rounded-lg border border-slate-200 object-cover" />`
            : '<span class="text-slate-400">—</span>' },
        { html: b.title, cls: 'font-semibold text-slate-800' },
        { html: showDeleted ? AdminUI.badge(L["Js_Deleted"], 'danger')
            : AdminUI.statusSwitch(b.isActive, `setStatus(${b.id}, ${!b.isActive})`) },
        { html: b.activeUntil
            ? `<span class="text-sm text-slate-500">${String(b.activeUntil).slice(0, 10)}</span>`
            : `<span class="text-sm text-slate-400">${L["Banner_Always"]}</span>` },
        { html: showDeleted ? AdminUI.restoreButton(b.id) : AdminUI.actions(b.id), cls: 'text-right' }
    ]), 5, { rowIds: data.data.map(b => b.id), selectable: !showDeleted });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

// Sətirdən aktiv/deaktiv etmə — adi redaktə endpoint-i ilə.
async function setStatus(id, isActive) {
    const d = await AdminUI.get(`/banner/${id}`);
    const fd = new FormData();
    fd.append('title', d.title);
    fd.append('is_active', String(isActive));
    if (d.activeUntil) fd.append('active_until', String(d.activeUntil).slice(0, 10));

    const res = await AdminUI.putForm(`/banner/${id}`, fd);
    if (res.ok) { AdminUI.notify(L["Js_StatusChanged"]); loadList(currentPage); }
    else AdminUI.error(AdminUI.errText(res));
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteBanner"])) return;
    const res = await AdminUI.del(`/banner/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
