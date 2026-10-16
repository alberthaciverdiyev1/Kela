let currentPage = 1;
let lastPage = 1;
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/subject/bulk', id, () => loadList(currentPage));
}

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/subject/list?page=${currentPage}&search=${encodeURIComponent(search)}&is_deleted=${showDeleted ? 1 : 0}`));
    lastPage = data.lastPage;

    const total = document.getElementById('stat_total');
    if (total) total.textContent = data.total;
    AdminUI.setInfo(data.total);

    AdminUI.table('rows', data.data.map((s, i) => [
        { html: AdminUI.rowNo(s, i, currentPage, data.perPage), cls: 'text-slate-400' },
        { html: `<a href="/lesson?subject_id=${s.id}" class="font-semibold text-primary hover:underline">${s.name}</a>` },
        { html: showDeleted ? AdminUI.restoreButton(s.id) : AdminUI.actions(s.id, { extra: AdminUI.actionLink(L["Nav_Lessons"], `/lesson?subject_id=${s.id}`) }), cls: 'text-right' }
    ]), 4, { rowIds: data.data.map(s => s.id), selectable: !showDeleted });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteSubject"])) return;
    const res = await AdminUI.del(`/subject/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
