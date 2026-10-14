let currentPage = 1;
let lastPage = 1;

async function loadList(page) {
    currentPage = page || 1;
    const subjectId = document.getElementById('filter_subject').value;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/question/list?page=${currentPage}&subject_id=${subjectId}&search=${encodeURIComponent(search)}`);
    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map(q => [
        { html: q.id, cls: 'text-slate-400' },
        { html: window.SUBJECTS[q.subjectId] ?? '-', cls: 'text-slate-500' },
        { html: q.titleType === 'image' && q.titleUrl
                ? `<img src="${q.titleUrl}" class="h-8 w-12 rounded border border-slate-200 object-cover" />`
                : (q.title ?? '-'), cls: 'font-medium text-slate-800' },
        { html: `<span class="rounded bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">${q.correctAnswer}</span>` },
        { html: AdminUI.actions(q.id), cls: 'text-right' }
    ]), 5, { rowIds: data.data.map(q => q.id), selectable: true });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu sual silinəcək.')) return;
    const res = await AdminUI.del(`/question/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
