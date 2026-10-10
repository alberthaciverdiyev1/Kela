let currentPage = 1;
let lastPage = 1;

async function loadList(page) {
    currentPage = page || 1;
    const subjectId = document.getElementById('filter_subject').value;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/lesson/list?page=${currentPage}&subject_id=${subjectId}&search=${encodeURIComponent(search)}`);

    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map(l => [
        { html: l.id, cls: 'text-slate-400' },
        { html: l.title, cls: 'font-medium text-slate-800' },
        { html: window.SUBJECTS[l.subjectId] ?? '-', cls: 'text-slate-500' },
        { html: l.sortOrder, cls: 'text-slate-500' },
        { html: l.isActive ? '<span class="rounded-full bg-emerald-100 px-2 py-0.5 text-xs text-emerald-700">Aktiv</span>' : '<span class="rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-500">Deaktiv</span>' },
        { html: AdminUI.actions(l.id), cls: 'text-right' }
    ]), 6);

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu dərs silinəcək.')) return;
    const res = await AdminUI.del(`/lesson/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
