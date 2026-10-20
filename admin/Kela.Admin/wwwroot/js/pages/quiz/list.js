var pager = AdminUI.createPager('pagination', loadList);
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/quiz/bulk', id, () => loadList(pager.page));
}

function quizActions(q) {
    return AdminUI.rowActions([
        AdminUI.actionButton(L["Common_Questions"], `openQuestions(${q.id})`),
        AdminUI.actionButton(L["Js_Edit"], `openEdit(${q.id})`, { tone: 'primary' }),
        AdminUI.actionButton(L["Common_Delete"], `removeItem(${q.id})`, { tone: 'danger' })
    ]);
}

async function loadList(page) {
    var pageNo = page || pager.page;
    const lessonId = document.getElementById('filter_lesson').value;
    const search = document.getElementById('search').value;
    const params = new URLSearchParams({ page: pager.page, lesson_id: lessonId, is_deleted: showDeleted ? 1 : 0, search });
    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/quiz/list?${params.toString()}`));
    pager.apply(data);

    AdminUI.table('rows', data.data.map((q, i) => [
        { html: AdminUI.rowNo(q, i, pager.page, pager.perPage), cls: 'text-slate-400' },
        { html: q.lessonId ? (window.LESSONS[q.lessonId] ?? '-') : '-', cls: 'text-slate-500' },
        { html: q.title, cls: 'font-medium text-slate-800' },
        { html: AdminUI.badge(q.type === 'general' ? L["Common_General"] : L["Common_Lesson"], 'neutral') },
        { html: q.durationMinutes, cls: 'text-slate-500' },
        { html: q.questionCount, cls: 'text-slate-500' },
        { html: AdminUI.statusSwitch(q.isActive, `setStatus(${q.id}, ${!q.isActive})`) },
        { html: showDeleted ? AdminUI.restoreButton(q.id) : quizActions(q), cls: 'text-right' }
    ]), 8, { rowIds: data.data.map(q => q.id), selectable: !showDeleted });
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteQuiz"])) return;
    const res = await AdminUI.del(`/quiz/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(pager.page); }
    else AdminUI.error();
}

async function setStatus(id, isActive) {
    const d = await AdminUI.get(`/quiz/${id}`);
    const fd = new FormData();
    fd.append('lesson_id', d.lessonId ?? '');
    fd.append('title', d.title);
    fd.append('type', d.type);
    fd.append('duration_minutes', d.durationMinutes ?? 0);
    fd.append('sort_order', d.sortOrder ?? 0);
    fd.append('is_repeat', String(!!d.isRepeat));
    fd.append('is_active', String(isActive));

    const res = await AdminUI.putForm(`/quiz/${id}`, fd);
    if (res.ok) { AdminUI.notify(L["Js_StatusChanged"]); loadList(pager.page); }
    else AdminUI.error(AdminUI.errText(res));
}
document.addEventListener('DOMContentLoaded', () => loadList(1));
