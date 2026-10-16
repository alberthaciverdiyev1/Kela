let currentPage = 1;
let lastPage = 1;
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/quiz/bulk', id, () => loadList(currentPage));
}

function quizActions(q) {
    return AdminUI.rowActions([
        AdminUI.actionButton(L["Common_Questions"], `openQuestions(${q.id})`),
        AdminUI.actionButton(L["Js_Edit"], `openEdit(${q.id})`, { tone: 'primary' }),
        AdminUI.actionButton(L["Common_Delete"], `removeItem(${q.id})`, { tone: 'danger' })
    ]);
}

async function loadList(page) {
    currentPage = page || 1;
    const lessonId = document.getElementById('filter_lesson').value;
    const search = document.getElementById('search').value;
    const params = new URLSearchParams({ page: currentPage, lesson_id: lessonId, is_deleted: showDeleted ? 1 : 0, search });
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/quiz/list?${params.toString()}`));
    lastPage = data.lastPage;
    AdminUI.setInfo(data.total);

    AdminUI.table('rows', data.data.map((q, i) => [
        { html: AdminUI.rowNo(q, i, currentPage, data.perPage), cls: 'text-slate-400' },
        { html: q.lessonId ? (window.LESSONS[q.lessonId] ?? '-') : '-', cls: 'text-slate-500' },
        { html: q.title, cls: 'font-medium text-slate-800' },
        { html: AdminUI.badge(q.type === 'general' ? L["Common_General"] : L["Common_Lesson"], 'neutral') },
        { html: q.durationMinutes, cls: 'text-slate-500' },
        { html: q.questionCount, cls: 'text-slate-500' },
        { html: q.isActive ? AdminUI.badge(L["Common_Active"], 'success') : AdminUI.badge(L["Common_Archived"], 'neutral') },
        { html: showDeleted ? AdminUI.restoreButton(q.id) : quizActions(q), cls: 'text-right' }
    ]), 8, { rowIds: data.data.map(q => q.id), selectable: !showDeleted });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteQuiz"])) return;
    const res = await AdminUI.del(`/quiz/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
