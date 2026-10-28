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
            '<path stroke-linecap="round" stroke-linejoin="round" d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.325.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 0 1 1.37.49l1.296 2.247a1.125 1.125 0 0 1-.26 1.431l-1.003.827c-.293.241-.438.613-.43.992a7.723 7.723 0 0 1 0 .255c-.008.378.137.75.43.991l1.004.827c.424.35.534.955.26 1.43l-1.298 2.247a1.125 1.125 0 0 1-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.47 6.47 0 0 1-.22.128c-.331.183-.581.495-.644.869l-.213 1.281c-.09.543-.56.94-1.11.94h-2.594c-.55 0-1.019-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 0 1-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 0 1-1.369-.49l-1.297-2.247a1.125 1.125 0 0 1 .26-1.431l1.004-.827c.292-.24.437-.613.43-.991a6.932 6.932 0 0 1 0-.255c.007-.38-.138-.751-.43-.992l-1.004-.827a1.125 1.125 0 0 1-.26-1.43l1.297-2.247a1.125 1.125 0 0 1 1.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.086.22-.128.332-.183.582-.495.644-.869l.214-1.28Z" /><path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />',
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
