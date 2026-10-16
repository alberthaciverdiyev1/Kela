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
        { html: statusBadge(c) },
        { html: c.url ? `<button type="button" class="text-primary hover:underline" onclick="previewContent(${c.id})">${L["Js_View"]}</button>` : '-' },
        { html: c.isActive ? AdminUI.badge('Aktiv', 'success') : AdminUI.badge('Deaktiv', 'neutral') },
        { html: showDeleted ? AdminUI.restoreButton(c.id) : AdminUI.actions(c.id), cls: 'text-right' }
    ]), 7, { rowIds: data.data.map(c => c.id), selectable: !showDeleted });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

const STATUS_BADGE = {
    pending:    { text: () => L["Video_Pending"],    tone: 'neutral' },
    processing: { text: () => L["Video_Processing"], tone: 'warning' },
    ready:      { text: () => L["Video_Ready"],      tone: 'success' },
    failed:     { text: () => L["Video_Failed"],     tone: 'danger'  }
};

function statusBadge(c) {
    if (c.type !== 'video') return '<span class="text-slate-300">—</span>';
    const b = STATUS_BADGE[c.conversionStatus];
    return b ? AdminUI.badge(b.text(), b.tone) : '<span class="text-slate-300">—</span>';
}

let previewHls = null;

function previewContent(id) {
    const c = CONTENT[id];
    if (!c || !c.url) return;

    if (previewHls) { try { previewHls.destroy(); } catch (e) {} previewHls = null; }

    document.getElementById('previewTitle').textContent = c.title || '';
    const body = document.getElementById('previewBody');
    const url = c.playbackUrl || c.url;

    if (c.type === 'video') {
        // m3u8-i yalnız <video src> ilə oynatmaq olmur (Safari xaric) — hls.js lazımdır.
        if (c.conversionStatus && c.conversionStatus !== 'ready') {
            body.innerHTML = `<div class="rounded-xl bg-slate-100 p-6 text-center text-sm text-slate-500">
                ${statusBadge(c)}<p class="mt-3">${L["Video_NotReady"]}</p></div>`;
        } else {
            body.innerHTML = `<video id="previewVideo" controls autoplay playsinline class="w-full rounded-xl bg-black"></video>`;
            const v = document.getElementById('previewVideo');
            if (/\.m3u8(\?|$)/i.test(url)) {
                if (v.canPlayType('application/vnd.apple.mpegurl')) {
                    v.src = url;
                } else if (window.Hls && window.Hls.isSupported()) {
                    previewHls = new window.Hls();
                    previewHls.loadSource(url);
                    previewHls.attachMedia(v);
                } else {
                    v.src = url;
                }
            } else {
                v.src = url;
            }
        }
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
