let currentPage = 1;
let lastPage = 1;
let showDeleted = false;
const TYPE_TEXT = { video: 'Video', file: 'Fayl', voice: L["Js_Voice"] };
const CONTENT = {};

const TYPE_TONE = {
    video: 'bg-violet-100 text-violet-700',
    file:  'bg-sky-100 text-sky-600',
    voice: 'bg-warning-bg text-warning'
};

const TYPE_ICON = {
    video: '<path stroke-linecap="round" stroke-linejoin="round" d="m10 9 5 3-5 3V9ZM4 6.5A2.5 2.5 0 0 1 6.5 4h11A2.5 2.5 0 0 1 20 6.5v11a2.5 2.5 0 0 1-2.5 2.5h-11A2.5 2.5 0 0 1 4 17.5v-11Z" />',
    file:  '<path stroke-linecap="round" stroke-linejoin="round" d="M7 3h7l5 5v13H7V3Zm7 0v5h5M10 13h6M10 17h4" />',
    voice: '<path stroke-linecap="round" stroke-linejoin="round" d="M12 3a3 3 0 0 1 3 3v5a3 3 0 0 1-6 0V6a3 3 0 0 1 3-3Zm-7 8a7 7 0 0 0 14 0M12 18v3" />',
    other: '<path stroke-linecap="round" stroke-linejoin="round" d="M12 8v5m0 3h.01M10.3 3.9 2.4 17.5A2 2 0 0 0 4.1 20.5h15.8a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0Z" />'
};

// Şəkil kimi göstərilə bilən material: video üçün konversiyadan çıxan thumbnail,
// qalan hallarda (şəkil faylı, şəkil URL-i) materialın özü.
function previewImage(c) {
    if (c.thumbnailUrl) return c.thumbnailUrl;
    if (/^data:image\//i.test(c.url || '')) return c.url;
    if (/\.(png|jpe?g|gif|webp|svg|avif)($|\?)/i.test(c.url || '')) return c.url;
    return null;
}

function contentThumb(c) {
    const tone = TYPE_TONE[c.type] ?? 'bg-slate-100 text-slate-500';
    const icon = TYPE_ICON[c.type] ?? TYPE_ICON.other;
    const src = previewImage(c);
    // İkon həmişə altdadır: şəkil yüklənməzsə (obyekt silinmiş ola bilər) sınıq şəkil
    // işarəsi yerinə növ ikonu görünür.
    const img = src
        ? `<img src="${src}" alt="" class="absolute inset-0 h-full w-full object-cover" onerror="this.remove()" />`
        : '';
    return `<span class="relative grid h-11 w-11 shrink-0 place-items-center overflow-hidden rounded-xl ${tone}">
        <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.9">${icon}</svg>${img}
    </span>`;
}

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
    const data = await AdminUI.get(AdminUI.addSort(`/lesson-content/list?${params.toString()}`));

    lastPage = data.lastPage;
    AdminUI.setInfo(data.total);
    data.data.forEach(c => { CONTENT[c.id] = c; });

    AdminUI.table('rows', data.data.map((c, i) => [
        { html: AdminUI.rowNo(c, i, currentPage, data.perPage), cls: 'text-slate-400' },
        { html: `<div class="flex items-center gap-4">${contentThumb(c)}<span class="font-medium text-slate-800">${c.title}</span></div>` },
        { html: TYPE_TEXT[c.type] ?? '-', cls: 'text-slate-500' },
        { html: statusBadge(c) },
        { html: c.url ? `<button type="button" class="text-primary hover:underline" onclick="previewContent(${c.id})">${L["Js_View"]}</button>` : '-' },
        { html: AdminUI.statusSwitch(c.isActive, `setStatus(${c.id}, ${!c.isActive})`) },
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

function previewContent(id) {
    const c = CONTENT[id];
    if (!c || !c.url) return;

    AdminMedia.destroy();

    document.getElementById('previewTitle').textContent = c.title || '';
    const body = document.getElementById('previewBody');
    const url = c.playbackUrl || c.url;

    if (c.type === 'video') {
        // m3u8-i yalnız <video src> ilə oynatmaq olmur (Safari xaric) — hls.js lazımdır.
        if (c.conversionStatus && c.conversionStatus !== 'ready') {
            body.innerHTML = `<div class="rounded-xl bg-slate-100 p-6 text-center text-sm text-slate-500">
                ${statusBadge(c)}<p class="mt-3">${L["Video_NotReady"]}</p></div>`;
        } else {
            // Şaquli videolar modal-ı ekrandan uzun edirdi — hündürlük məhdudlaşdırılır,
            // tam ölçü üçün fullscreen düyməsi var.
            body.innerHTML = `<video id="previewVideo" controls autoplay playsinline data-src="${url}"
                class="preview-video mx-auto block max-h-[55vh] max-w-full rounded-xl bg-black"></video>`;
            AdminMedia.hydrate(body);
        }
    } else if (c.type === 'voice') {
        body.innerHTML = `<audio src="${c.url}" controls autoplay class="mx-auto block w-full"></audio>`;
    } else if (/\.(png|jpe?g|gif|webp|svg)($|\?)/i.test(c.url)) {
        body.innerHTML = `<img src="${c.url}" alt="" class="mx-auto max-h-[70vh] rounded-xl" />`;
    } else if (/\.pdf($|\?)/i.test(c.url)) {
        body.innerHTML = `<iframe src="${c.url}" class="h-[70vh] w-full rounded-xl"></iframe>`;
    } else {
        body.innerHTML = `<iframe src="${c.url}" class="h-[70vh] w-full rounded-xl" title="${c.title || ''}"></iframe>`;
    }

    document.getElementById('previewFullscreenBtn')
        .classList.toggle('hidden', !document.getElementById('previewVideo'));

    AdminUI.openModal('previewModal');
}

function togglePreviewFullscreen() {
    AdminMedia.toggleFullscreen(document.getElementById('previewVideo'));
}

function closePreview() {
    AdminUI.closeModal('previewModal');
}

// ESC/fon klik ortaq modallarda idarə olunur (admin.js) — burada yalnız
// preview-a aid resurslar (video/hls) buraxılır.
document.getElementById('previewModal').addEventListener('modal:closed', () => {
    document.getElementById('previewBody').innerHTML = '';
    AdminMedia.destroy();
    document.getElementById('previewFullscreenBtn').classList.add('hidden');
});

async function removeItem(id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteMaterial"])) return;
    const res = await AdminUI.del(`/lesson-content/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(currentPage); }
    else AdminUI.error();
}

async function setStatus(id, isActive) {
    const d = await AdminUI.get(`/lesson-content/${id}`);
    const fd = new FormData();
    fd.append('title', d.title);
    fd.append('description', d.description ?? '');
    fd.append('sort_order', d.sortOrder ?? 0);
    fd.append('is_active', String(isActive));

    const res = await AdminUI.putForm(`/lesson-content/${id}`, fd);
    if (res.ok) { AdminUI.notify(L["Js_StatusChanged"]); loadList(currentPage); }
    else AdminUI.error(AdminUI.errText(res));
}
document.addEventListener('DOMContentLoaded', () => loadList(1));
