let currentPage = 1;
let lastPage = 1;

function quizActions(q) {
    return `<div class="flex items-center justify-end gap-2">
        <button type="button" class="px-2 py-1 text-navy hover:bg-slate-100 rounded-lg" onclick="openQuestions(${q.id})">Suallar</button>
        <button type="button" class="px-2 py-1 text-primary hover:bg-primary/10 rounded-lg" onclick="openEdit(${q.id})">Redaktə</button>
        <button type="button" class="px-2 py-1 text-red-600 hover:bg-red-50 rounded-lg" onclick="removeItem(${q.id})">Sil</button>
    </div>`;
}

async function loadList(page) {
    currentPage = page || 1;
    const lessonId = document.getElementById('filter_lesson').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/quiz/list?page=${currentPage}&lesson_id=${lessonId}`);
    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map(q => [
        { html: q.id, cls: 'text-slate-400' },
        { html: q.lessonId ? (window.LESSONS[q.lessonId] ?? '-') : '-', cls: 'text-slate-500' },
        { html: q.title, cls: 'font-medium text-slate-800' },
        { html: AdminUI.badge(q.type === 'general' ? 'Ümumi' : 'Dərs', 'neutral') },
        { html: q.durationMinutes, cls: 'text-slate-500' },
        { html: q.questionCount, cls: 'text-slate-500' },
        { html: q.isActive ? AdminUI.badge('Aktiv', 'success') : AdminUI.badge('Deaktiv', 'neutral') },
        { html: quizActions(q), cls: 'text-right' }
    ]), 8);

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu imtahan silinəcək.')) return;
    const res = await AdminUI.del(`/quiz/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
