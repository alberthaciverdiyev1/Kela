(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/lesson-content/${id}`);
        document.getElementById('modalTitle').textContent = L["Js_EditMaterial"];
        document.getElementById('entityId').value = data.id;
        document.getElementById('sort_order').value = data.sortOrder ?? 0;
        document.getElementById('description').value = data.description ?? '';
        // Əsas dərs sabitdir; əlavə dərslər seçilə bilər.
        document.getElementById('primary_lesson_id').value = data.lessonId;
        const selected = new Set([String(data.lessonId), ...(data.lessonIds || []).map(String)]);
        [...document.getElementById('lesson_ids').options].forEach(o => { o.selected = selected.has(o.value); });
        document.getElementById('lesson_ids').dispatchEvent(new Event('change'));
        if (window.renderLessonSorts) window.renderLessonSorts(data.lessonSorts || {});
        document.getElementById('title').value = data.title;
        document.getElementById('type').value = data.type ?? 'video';
        document.getElementById('is_active').checked = data.isActive;
        document.getElementById('fileBlock').classList.add('hidden');
        document.getElementById('type').disabled = true;
        renderCurrentFile(data);
        AdminUI.openModal('modal');
    };

    // Redaktədə fayl dəyişdirilə bilmir — mövcud material göstərilir ki,
    // istifadəçi nə olduğunu görsün.
    function renderCurrentFile(data) {
        const box = document.getElementById('currentFile');
        const url = data.playbackUrl || data.url;
        if (!url) { box.innerHTML = ''; box.classList.add('hidden'); return; }

        let inner;
        if (data.type === 'video') {
            inner = data.conversionStatus && data.conversionStatus !== 'ready'
                ? `<div class="rounded-xl bg-slate-100 p-4 text-center text-sm text-slate-500">${statusBadge(data)}</div>`
                : `<video controls playsinline preload="metadata" data-src="${url}"
                       class="preview-video mx-auto block max-h-[40vh] max-w-full rounded-xl bg-black"></video>`;
        } else if (data.type === 'voice') {
            inner = `<audio controls data-src="${url}" class="w-full"></audio>`;
        } else if (/\.(png|jpe?g|gif|webp|svg)($|\?)/i.test(url)) {
            inner = `<img src="${url}" alt="" class="mx-auto max-h-[40vh] rounded-xl" />`;
        } else {
            inner = `<iframe src="${url}" class="h-[40vh] w-full rounded-xl" title=""></iframe>`;
        }

        box.innerHTML = `<div class="mb-1.5 text-xs font-semibold text-slate-500">${L["Common_CurrentFile"]}</div>` +
            `<div class="rounded-xl border border-border bg-surface-secondary p-2">${inner}</div>`;
        box.classList.remove('hidden');
        AdminMedia.hydrate(box);
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js idarə edir

        const fd = new FormData();
        fd.append('title', document.getElementById('title').value);
        fd.append('sort_order', document.getElementById('sort_order').value || '0');
        fd.append('description', document.getElementById('description').value || '');
        fd.append('is_active', document.getElementById('is_active').checked ? 'true' : 'false');
        [...document.getElementById('lesson_ids').selectedOptions].forEach(o => fd.append('lesson_ids', o.value));
        if (window.lessonSortsJson) fd.append('lesson_sorts', window.lessonSortsJson());

        const res = await AdminUI.putForm(`/lesson-content/${id}`, fd);
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); AdminUI.closeModal('modal'); loadList(pager.page); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
