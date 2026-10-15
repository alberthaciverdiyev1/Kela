let currentPage = 1;
let lastPage = 1;
let showDeleted = false;
const TYPE_TEXT = { video: 'Video', file: 'Fayl', voice: L["Js_Voice"] };
const CONTENT = {};

function toggleTrash() {
    showDeleted = !showDeleted;
    AdminUI.syncTrashButton('trashBtn', showDeleted);
    loadList(1);
}

async function restoreRow(id) {
    await AdminUI.restoreItem('/lesson-content/bulk', id, () => loadList(currentPage));
}

async function loadList(page) {
    currentPage = page || 1;
    const lessonId = document.getElementById('filter_lesson').value;
    const search = document.getElementById('search').value;
    const params = new URLSearchParams({ page: currentPage, lesson_id: lessonId, is_deleted: showDeleted ? 1 : 0, search });
    AdminUI.loader(true);
    const data = await AdminUI.get(`/lesson-content/list?${params.toString()}`);

    lastPage = data.lastPage;
    data.data.forEach(c => { CONTENT[c.id] = c; });

    AdminUI.table('rows', data.data.map((c, i) => [
        { html: AdminUI.rowNo(c, i, currentPage, data.perPage), cls: 'text-slate-400' },
        { html: c.title, cls: 'font-medium text-slate-800' },
        { html: TYPE_TEXT[c.type] ?? '-', cls: 'text-slate-500' },
        { html: c.url ? `<button type="button" class="text-primary hover:underline" onclick="previewContent(${c.id})">${L["Js_View"]}</button>` : '-' },
        { html: c.isActive ? AdminUI.badge('Aktiv', 'success') : AdminUI.badge('Deaktiv', 'neutral') },
        { html: showDeleted ? AdminUI.restoreButton(c.id) : AdminUI.actions(c.id), cls: 'text-right' }
    ]), 6, { rowIds: data.data.map(c => c.id), selectable: !showDeleted });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

function previewContent(id) {
    const c = CONTENT[id];
    if (!c || !c.url) return;

    document.getElementById('previewTitle').textContent = c.title || '';
    const body = document.getElementById('previewBody');
    const url = c.playbackUrl || c.url;

    if (c.type === 'video') {
        body.innerHTML = `<video src="${url}" controls autoplay playsinline class="w-full rounded-xl bg-black"></video>`;
    } else if (c.type === 'voice') {
        body.innerHTML = `<audio src="${c.url}" controls autoplay class="w-full"></audio>`;
    } else if (/\.(png|jpe?g|gif|webp|svg)($|\?)/i.test(c.url)) {
        body.innerHTML = `<img src="${c.url}" alt="" class="mx-auto max-h-[70vh] rounded-xl" />`;
    } else if (/\.pdf($|\?)/i.test(c.url)) {
        body.innerHTML = `<iframe src="${c.url}" class="h-[70vh] w-full rounded-xl"></iframe>`;
    } else {
        body.innerHTML = `<iframe src="${c.url}" class="h-[70vh] w-full rounded-xl" title="${c.title || ''}"></iframe>`;
    }

    const modal = document.getElementById('previewModal');
    modal.classList.remove('hidden');
    modal.classList.add('flex');
}

function closePreview() {
    const body = document.getElementById('previewBody');
    body.innerHTML = '';
    const modal = document.getElementById('previewModal');
    modal.classList.add('hidden');
    modal.classList.remove('flex');
}

document.addEventListener('keydown', (e) => { if (e.key === 'Escape') closePreview(); });

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteMaterial"])) return;
    const res = await AdminUI.del(`/lesson-content/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
