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
        ? `<img src="${AdminUI.escapeHtml(lesson.image)}" alt="" class="absolute inset-0 h-full w-full object-cover" data-img-remove />`
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
        AdminUI.actionIcon(
            '<path stroke-linecap="round" stroke-linejoin="round" d="M10.3 4.3a2 2 0 0 1 3.4 0l.5.9 1-.3a2 2 0 0 1 2.5 2.5l-.3 1 .9.5a2 2 0 0 1 0 3.4l-.9.5.3 1a2 2 0 0 1-2.5 2.5l-1-.3-.5.9a2 2 0 0 1-3.4 0l-.5-.9-1 .3a2 2 0 0 1-2.5-2.5l.3-1-.9-.5a2 2 0 0 1 0-3.4l.9-.5-.3-1a2 2 0 0 1 2.5-2.5l1 .3.5-.9Z" /><circle cx="12" cy="12" r="2.6" />',
            L["Lesson_SettingsTitle"], `openSettings(${id})`),
        AdminUI.actionButton(L["Js_Edit"], `openEdit(${id})`, { tone: 'primary' }),
        AdminUI.actionButton(L["Common_Delete"], `removeItem(${id})`, { tone: 'danger' })
    ]);
}

function setStatus(id, isActive) {
    AdminUI.setActive('/lesson', id, isActive, () => loadList(pager.page));
}

AdminUI.listPage({
    api: '/lesson/list',
    bulkUrl: '/lesson/bulk',
    deleteBase: '/lesson',
    confirmKey: 'Js_ConfirmDeleteLesson',
    columns: 6,
    selectable: true,
    params: ({ page, search, showDeleted }) => ({
        page,
        search,
        subject_id: document.getElementById('filter_subject').value || undefined,
        is_active: document.getElementById('filter_status').value || undefined,
        is_deleted: showDeleted ? 1 : undefined
    }),
    onLoaded: data => {
        const total = data.total ?? (data.data || []).length;
        const el = document.getElementById('stat_total');
        if (el) el.textContent = total;
    },
    onReady: () => {
        document.getElementById('search')?.addEventListener('keydown', (event) => {
            if (event.key === 'Enter') loadList(1);
        });
    },
    row: (l, i, ctx) => {
        const subjectName = window.SUBJECTS[l.subjectId] ?? '-';
        return [
            { html: AdminUI.rowNo(l, i, ctx.page, ctx.perPage), cls: 'text-slate-400' },
            {
                html: `<div class="flex items-center gap-4">
                    ${lessonThumb(l)}
                    <div>
                        <a href="/lesson-content?lesson_id=${l.id}" class="font-bold text-slate-950 hover:text-primary">${AdminUI.escapeHtml(l.title)}</a>
                        <p class="mt-0.5 flex items-center gap-2 text-sm text-slate-500">
                            <span>${subjectName} dərsi</span>
                            ${l.isShared ? AdminUI.badge(L["Lesson_Shared"], 'primary') : ''}
                        </p>
                    </div>
                </div>`
            },
            { html: subjectBadge(subjectName) },
            { html: `<span class="inline-flex items-center gap-2 text-slate-700">${l.sortOrder ?? 0}</span>` },
            { html: AdminUI.statusSwitch(l.isActive, `setStatus(${l.id}, ${!l.isActive})`) },
            { html: ctx.showDeleted ? AdminUI.restoreButton(l.id) : rowActions(l.id), cls: 'text-right' }
        ];
    }
});
