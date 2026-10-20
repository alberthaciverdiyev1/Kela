var pager = AdminUI.createPager('pagination', loadList);
let showDeleted = false;

// Admin hər banneri idarə edir; teacher yalnız öz yaratdığını.
function canEdit(b) {
    const me = window.ADMIN || {};
    return me.isAdmin || (b.userId != null && b.userId === me.userId);
}

function esc(v) {
    return String(v ?? '').replace(/[&<>"']/g, c => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[c]));
}

function ownerCell(b) {
    if (b.userId == null) return AdminUI.badge(L["Banner_OwnerAdmin"], 'neutral');
    if (b.userId === (window.ADMIN || {}).userId) return AdminUI.badge(L["Banner_OwnerYou"], 'success');
    return `<span class="text-sm text-slate-600">${esc(b.ownerName || ('#' + b.userId))}</span>`;
}

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/banner/bulk', id, () => loadList(pager.page));
}

async function loadList(page) {
    var pageNo = page || pager.page;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/banner/list?page=${pageNo}&search=${encodeURIComponent(search)}&is_deleted=${showDeleted ? 1 : 0}`));
    pager.apply(data);

    const total = document.getElementById('stat_total');
    if (total) total.textContent = data.total;

    AdminUI.table('rows', data.data.map((b, i) => [
        {html: AdminUI.rowNo(b, i, pager.page, pager.perPage), cls: 'text-slate-400'},
        {
            html: b.image
                ? `<img src="${b.image}" class="h-10 w-16 rounded-lg border border-slate-200 object-cover" />`
                : '<span class="text-slate-400">—</span>'
        },
        {html: b.title, cls: 'font-semibold text-slate-800'},
        {html: ownerCell(b)},
        {
            html: showDeleted ? AdminUI.badge(L["Js_Deleted"], 'danger')
                : canEdit(b)
                    ? AdminUI.statusSwitch(b.isActive, `setStatus(${b.id}, ${!b.isActive})`)
                    : AdminUI.badge(b.isActive ? L["Common_Active"] : L["Common_Inactive"], b.isActive ? 'success' : 'neutral')
        },
        {
            html: b.activeUntil
                ? `<span class="text-sm text-slate-500">${String(b.activeUntil).slice(0, 10)}</span>`
                : `<span class="text-sm text-slate-400">${L["Banner_Always"]}</span>`
        },
        {html: showDeleted ? AdminUI.restoreButton(b.id) : (canEdit(b) ? AdminUI.actions(b.id) : ''), cls: 'text-right'}
    ]), 6, {
        rowIds: data.data.map(b => b.id),
        selectable: !showDeleted,
        selectableIds: data.data.filter(canEdit).map(b => b.id)
    });
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
    if (res.ok) {
        AdminUI.notify(L["Js_StatusChanged"]);
        loadList(pager.page);
    } else AdminUI.error(AdminUI.errText(res));
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteBanner"])) return;
    const res = await AdminUI.del(`/banner/${id}`);
    if (res.ok) {
        AdminUI.notify(L["Js_Deleted"]);
        loadList(pager.page);
    } else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
