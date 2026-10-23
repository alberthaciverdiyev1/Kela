let qzId = null;
let qzOptions = null;

function fillQuestionSelect(items) {
    document.getElementById('questionsSelect').innerHTML =
        items.map(o => `<option value="${o.id}">${o.text}</option>`).join('');
}

async function openQuestions(id) {
    qzId = id;
    const q = await AdminUI.get('/quiz/' + id);
    document.getElementById('questionsTitle').textContent = (q.title || '') + ' — suallar';

    if (!qzOptions) qzOptions = await AdminUI.get('/quiz/options');
    fillQuestionSelect(qzOptions);

    await refreshQuestions();
    AdminUI.openModal('questionsModal');
}

function closeQuestions() {
    AdminUI.closeModal('questionsModal');
}

async function refreshQuestions() {
    const items = await AdminUI.get(`/quiz/${qzId}/questions`);
    const box = document.getElementById('questionsList');
    box.innerHTML = '';

    if (!items.length) { box.innerHTML = '<div class="text-sm text-slate-400">' + L["Js_Empty"] + '</div>'; return; }

    items.forEach(q => {
        const row = document.createElement('div');
        row.className = 'flex items-center justify-between rounded-lg bg-slate-50 px-3 py-2';
        row.innerHTML = `<span class="text-sm text-slate-700">${AdminUI.escapeHtml(q.title ?? ('#' + q.id))}</span>
            <button type="button" class="text-sm text-red-600 hover:text-red-800"${AdminUI.actionAttrs('removeQuestion', [q.id])}>Sil</button>`;
        box.appendChild(row);
    });
}

async function assignQuestion() {
    const value = document.getElementById('questionsSelect').value;
    if (!value) return;
    const res = await AdminUI.postJson(`/quiz/${qzId}/questions`, { questionIds: [Number(value)] });
    if (res.ok) { AdminUI.notify(L["Js_Added"]); refreshQuestions(); }
    else AdminUI.error();
}

async function removeQuestion(questionId) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmRemove"])) return;
    const res = await AdminUI.del(`/quiz/${qzId}/questions/${questionId}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); refreshQuestions(); loadList(pager.page); }
    else AdminUI.error();
}
