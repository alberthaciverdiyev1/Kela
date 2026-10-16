let currentPage = 1;
let lastPage = 1;
let showDeleted = false;

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/lesson/bulk', id, () => loadList(currentPage));
}


// Dərs üçün tək ikon — digər səhifələrdəki kimi sətir başına dəyişən rəng yox.
function lessonIcon() {
    return '<path stroke-linecap="round" stroke-linejoin="round" d="M12 6.253v13m0-13C10.8 5.5 9 5 7 5S3.2 5.5 2 6.253v13C3.2 18.5 5 18 7 18s3.8.5 5 1.253m0-13C13.2 5.5 15 5 17 5s3.8.5 5 1.253v13C20.8 18.5 19 18 17 18s-3.8.5-5 1.253" />';
}

function hasImage(url) {
    // Storage şəkil olmadıqda placeholder qaytarır (bax: IFileStorage.GetUrl) — onu real şəkil saymırıq.
    return !!url && !String(url).includes('noPhoto');
}

function lessonThumb(lesson) {
    // İkon həmişə altdadır: şəkil yüklənməzsə sınıq şəkil işarəsi yerinə ikon görünür.
    const img = hasImage(lesson.image)
        ? `<img src="${lesson.image}" alt="" class="absolute inset-0 h-full w-full object-cover" onerror="this.remove()" />`
        : '';
    return `<span class="relative grid h-11 w-11 shrink-0 place-items-center overflow-hidden rounded-xl bg-primary-light text-primary">
        <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.9">${lessonIcon()}</svg>${img}</span>`;
}

function subjectBadge(subjectName) {
    return AdminUI.chip(subjectName || '-', 'primary');
}

function rowActions(id) {
    return AdminUI.rowActions([
        AdminUI.actionLink(L["Nav_Materials"], `/lesson-content?lesson_id=${id}`),
        AdminUI.actionButton(L["Lesson_SettingsTitle"], `openSettings(${id})`),
        AdminUI.actionButton(L["Js_Edit"], `openEdit(${id})`, { tone: 'primary' }),
        AdminUI.actionButton(L["Common_Delete"], `removeItem(${id})`, { tone: 'danger' })
    ]);
}


function updateStats(data) {
    const total = data.total ?? data.Total ?? (data.data || []).length;
    const el = document.getElementById('stat_total');
    if (el) el.textContent = total;
}

function renderPagination(containerId, current, last, onSelect) {
    const box = document.getElementById(containerId);
    if (!box) return;
    box.innerHTML = '';

    const makeButton = (label, page, active, disabled) => {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.innerHTML = label;
        btn.disabled = disabled;
        btn.className = 'grid h-10 min-w-10 place-items-center rounded-xl text-sm font-bold transition ' +
            (active ? 'bg-primary text-white shadow-[0_10px_24px_rgba(0,184,132,0.25)]' : 'bg-surface-secondary text-slate-700 hover:bg-primary-light hover:text-primary') +
            (disabled ? ' opacity-50' : '');
        if (!disabled) btn.addEventListener('click', () => onSelect(page));
        return btn;
    };

    box.appendChild(makeButton('‹', Math.max(1, current - 1), false, current <= 1));
    for (let i = 1; i <= Math.max(1, last); i++) {
        box.appendChild(makeButton(i, i, i === current, false));
    }
    box.appendChild(makeButton('›', Math.min(last, current + 1), false, current >= last));
}

async function loadList(page) {
    currentPage = page || 1;
    const subjectId = document.getElementById('filter_subject').value;
    const status = document.getElementById('filter_status').value;
    const search = document.getElementById('search').value;
    const params = new URLSearchParams({ page: currentPage, search });
    if (subjectId) params.set('subject_id', subjectId);
    if (status) params.set('is_active', status);
    if (showDeleted) params.set('is_deleted', '1');

    AdminUI.loader(true);
    const data = await AdminUI.get(AdminUI.addSort(`/lesson/list?${params.toString()}`));
    const items = data.data || data.Data || [];
    const perPage = data.perPage || data.PerPage || 20;

    lastPage = data.lastPage || data.LastPage || 1;
    updateStats(data);

    const rows = items.map((l, index) => {
        const subjectName = window.SUBJECTS[l.subjectId] ?? '-';
        return [
            { html: AdminUI.rowNo(l, index, currentPage, perPage), cls: 'text-slate-400' },
            {
                html: `<div class="flex items-center gap-4">
                    ${lessonThumb(l)}
                    <div>
                        <a href="/lesson-content?lesson_id=${l.id}" class="font-bold text-slate-950 hover:text-primary">${l.title}</a>
                        <p class="mt-0.5 text-sm text-slate-500">${subjectName} dərsi</p>
                    </div>
                </div>`
            },
            { html: subjectBadge(subjectName) },
            { html: `<span class="inline-flex items-center gap-2 text-slate-700">${l.sortOrder ?? rowNumber}</span>` },
            {
                html: AdminUI.statusSwitch(l.isActive, `setStatus(${l.id}, ${!l.isActive})`)
            },
            { html: showDeleted ? AdminUI.restoreButton(l.id) : rowActions(l.id), cls: 'text-right' }
        ];
    });

    AdminUI.table('rows', rows, 6, { rowIds: items.map(l => l.id), selectable: !showDeleted });
    renderPagination('pagination', currentPage, lastPage, loadList);

    AdminUI.setInfo(data.total ?? rows.length);

    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteLesson"])) return;
    const res = await AdminUI.del(`/lesson/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(currentPage); }
    else AdminUI.error();
}

// Sətirdən status dəyişmə: adi redaktə endpoint-i (əlavə endpoint yoxdur).
async function setStatus(id, isActive) {
    const d = await AdminUI.get(`/lesson/${id}`);
    const fd = new FormData();
    fd.append('subject_id', d.subjectId);
    fd.append('title', d.title);
    fd.append('sort_order', d.sortOrder ?? 0);
    fd.append('is_active', String(isActive));

    const res = await AdminUI.putForm(`/lesson/${id}`, fd);
    if (res.ok) { AdminUI.notify(L["Js_StatusChanged"]); loadList(currentPage); }
    else AdminUI.error(AdminUI.errText(res));
}
document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('search')?.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') loadList(1);
    });
    loadList(1);
});
