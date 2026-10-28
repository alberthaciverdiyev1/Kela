let qzId = null;
let qzOptions = null;

function fillQuestionSelect(items) {
    document.getElementById('questionsSelect').innerHTML =
        items.map(o => `<option value="${o.id}">${AdminUI.escapeHtml(o.text)}</option>`).join('');
    renderQuestionPreview();
}

function questionImages(q) {
    if (!q) return [];
    return [q.titleType === 'image' ? q.titleUrl : null,
        q.variantAUrl, q.variantBUrl, q.variantCUrl, q.variantDUrl, q.variantEUrl]
        .filter(Boolean);
}

function thumbs(urls, cls) {
    return urls.map(u => `<img src="${AdminUI.escapeHtml(u)}" class="${cls} rounded-lg border border-slate-200 object-cover" />`).join('');
}

// Seçilmiş sual şəkillidirsə modalda önizləmə göstərir — başlıq boş olduğu üçün
// istifadəçi hansı sualı əlavə etdiyini yalnız şəkillə tanıya bilir.
function renderQuestionPreview() {
    const box = document.getElementById('questionPreview');
    if (!box) return;

    const value = document.getElementById('questionsSelect').value;
    const option = (qzOptions || []).find(o => String(o.id) === value);
    const urls = option ? [option.titleUrl, ...(option.imageUrls || [])].filter(Boolean) : [];

    if (!urls.length) {
        box.innerHTML = '';
        box.classList.add('hidden');
        box.classList.remove('flex');
        return;
    }

    box.innerHTML = thumbs(urls, 'h-20 w-28');
    box.classList.remove('hidden');
    box.classList.add('flex');
}

document.getElementById('questionsSelect').addEventListener('change', renderQuestionPreview);

async function openQuestions(id) {
    qzId = id;
    const q = await AdminUI.get('/quiz/' + id);
    document.getElementById('questionsTitle').textContent = (q.title || '') + ' — suallar';

    // Siyahı imtahana görə dəyişir — hər açılışda yenidən alınır.
    qzOptions = await AdminUI.get('/quiz/options?quiz_id=' + id);
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
        row.className = 'flex items-center justify-between gap-3 rounded-lg bg-slate-50 px-3 py-2';

        const images = questionImages(q);
        const title = q.titleType === 'image' || !q.title ? ('#' + q.id) : q.title;

        row.innerHTML = `<div class="flex min-w-0 items-center gap-3">
                ${images.length ? `<div class="flex shrink-0 gap-1">${thumbs(images, 'h-10 w-14')}</div>` : ''}
                <span class="truncate text-sm text-slate-700">${AdminUI.escapeHtml(title)}</span>
            </div>
            <button type="button" class="shrink-0 text-sm text-red-600 hover:text-red-800"${AdminUI.actionAttrs('removeQuestion', [q.id])}>Sil</button>`;
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
