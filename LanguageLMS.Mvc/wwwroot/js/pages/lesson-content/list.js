let currentPage = 1;
let lastPage = 1;
const TYPE_TEXT = { video: 'Video', file: 'Fayl', voice: 'Səs' };

async function loadList(page) {
    currentPage = page || 1;
    const lessonId = document.getElementById('filter_lesson').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/lesson-content/list?page=${currentPage}&lesson_id=${lessonId}`);

    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map(c => [
        { html: c.id, cls: 'text-slate-400' },
        { html: c.title, cls: 'font-medium text-slate-800' },
        { html: TYPE_TEXT[c.type] ?? '-', cls: 'text-slate-500' },
        { html: c.url ? `<a href="${c.url}" target="_blank" class="text-primary hover:underline">aç</a>` : '-' },
        { html: c.sortOrder, cls: 'text-slate-500' },
        { html: c.isActive ? AdminUI.badge('Aktiv', 'success') : AdminUI.badge('Deaktiv', 'neutral') },
        { html: AdminUI.actions(c.id), cls: 'text-right' }
    ]), 7);

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu material silinəcək.')) return;
    const res = await AdminUI.del(`/lesson-content/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
