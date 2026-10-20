var pager = AdminUI.createPager('pagination', loadList);
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/question/bulk', id, () => loadList(pager.page));
}

async function loadList(page) {
    var pageNo = page || pager.page;
    const subjectId = document.getElementById('filter_subject').value;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/question/list?page=${pageNo}&subject_id=${subjectId}&search=${encodeURIComponent(search)}&is_deleted=${showDeleted ? 1 : 0}`));
    pager.apply(data);

    AdminUI.table('rows', data.data.map((q, i) => [
        { html: AdminUI.rowNo(q, i, pager.page, pager.perPage), cls: 'text-slate-400' },
        { html: window.SUBJECTS[q.subjectId] ?? '-', cls: 'text-slate-500' },
        { html: q.titleType === 'image' && q.titleUrl
                ? `<img src="${q.titleUrl}" class="h-8 w-12 rounded border border-slate-200 object-cover" />`
                : (q.title ?? '-'), cls: 'font-medium text-slate-800' },
        { html: `<span class="rounded bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">${q.correctAnswer}</span>` },
        { html: showDeleted ? AdminUI.restoreButton(q.id) : AdminUI.actions(q.id), cls: 'text-right' }
    ]), 5, { rowIds: data.data.map(q => q.id), selectable: !showDeleted });
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteQuestion"])) return;
    const res = await AdminUI.del(`/question/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(pager.page); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
