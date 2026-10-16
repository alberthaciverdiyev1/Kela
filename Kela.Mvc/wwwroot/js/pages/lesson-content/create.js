(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    function showError(message) { errBox.textContent = message; errBox.classList.remove('hidden'); }
    function done() { AdminUI.notify(L["Js_Added"]); window.closeModal(); loadList(1); }

    window.showModal = function () {
        document.getElementById('modal').classList.remove('hidden');
        document.getElementById('modal').classList.add('flex');
    };
    window.closeModal = function () {
        document.getElementById('modal').classList.add('hidden');
        document.getElementById('modal').classList.remove('flex');
        errBox.classList.add('hidden');
    };

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = 'Yeni material';
        document.getElementById('entityId').value = '';
        document.getElementById('sort_order').value = '';
        document.getElementById('description').value = '';
        document.getElementById('lesson_id').value = '';
        document.getElementById('title').value = '';
        document.getElementById('type').value = 'video';
        document.getElementById('is_active').checked = true;
        document.getElementById('file').value = '';
        document.getElementById('fileBlock').classList.remove('hidden');
        document.getElementById('lesson_id').disabled = false;
        document.getElementById('type').disabled = false;
        window.showModal();
    };

    // Böyük fayllar hissələrə bölünür: GB-larla videoda nazik PUT nə paralel yüklənmə,
    // nə də kəsilmədən sonra bərpa imkanı verir.
    const MP_THRESHOLD = 100 * 1024 * 1024;
    const MP_CONCURRENCY = 3;

    // Serverdən keçən yol üçün Kestrel limiti ~30 MB-dır — ondan böyüyü qəbul edilmir.
    const SERVER_FALLBACK_MAX = 25 * 1024 * 1024;

    function setProgress(text) {
        const btn = document.getElementById('saveBtn');
        if (!btn) return;
        btn.disabled = !!text;
        btn.textContent = text || L["Common_Save"];
    }

    async function uploadMultipart(file, lessonId, type, contentType) {
        const fd = new FormData();
        fd.append('lessonId', lessonId);
        fd.append('type', type);
        fd.append('fileName', file.name);
        fd.append('contentType', contentType);

        const startRes = await AdminUI.postForm('/lesson-content/upload/multipart/start', fd);
        if (!startRes.ok || !startRes.data?.uploadId) return null;

        const { path, uploadId, partSize } = startRes.data;
        const total = Math.ceil(file.size / partSize);

        try {
            const urlsRes = await AdminUI.postJson('/lesson-content/upload/multipart/parts',
                { path, uploadId, partNumbers: Array.from({ length: total }, (_, i) => i + 1) });
            if (!urlsRes.ok) throw new Error('parts');

            const parts = new Array(total);
            let uploaded = 0;
            let next = 0;

            const worker = async () => {
                while (true) {
                    const i = next++;
                    if (i >= total) return;

                    const u = urlsRes.data[i];
                    const blob = file.slice((u.partNumber - 1) * partSize, u.partNumber * partSize);

                    const put = await fetch(u.url, { method: 'PUT', body: blob });
                    if (!put.ok) throw new Error('part ' + u.partNumber + ': ' + put.status);

                    parts[u.partNumber - 1] = { partNumber: u.partNumber, eTag: put.headers.get('ETag') };
                    uploaded++;
                    setProgress('Yüklənir… ' + Math.round(uploaded / total * 100) + '%');
                }
            };

            await Promise.all(Array.from({ length: Math.min(MP_CONCURRENCY, total) }, worker));

            const finish = await AdminUI.postJson('/lesson-content/upload/multipart/complete',
                { path, uploadId, parts });
            if (!finish.ok) throw new Error('complete');

            return path;
        } catch (err) {
            // Yarımçıq yükləmə R2-də yer tutur və 7 gün saxlanılır — ləğv edirik.
            AdminUI.postJson('/lesson-content/upload/multipart/abort', { path, uploadId });
            throw err;
        }
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js idarə edir

        const file = document.getElementById('file').files[0];
        if (!file) { showError(L["Common_ChooseFile"]); return; }

        const lessonId = document.getElementById('lesson_id').value;
        const type = document.getElementById('type').value;
        const title = document.getElementById('title').value;
        const isActive = document.getElementById('is_active').checked ? 'true' : 'false';
        const contentType = file.type || 'application/octet-stream';

        try {
            setProgress('Yüklənir… 0%');

            // 1) Böyük fayl -> çoxhissəli yükləmə (paralel, hissə-hissə bərpa oluna bilən)
            if (file.size > MP_THRESHOLD) {
                const path = await uploadMultipart(file, lessonId, type, contentType);
                if (!path) { showError(L["Js_FileTooLarge"]); return; }

                setProgress(L["Common_Save"] + '…');
                const res = await AdminUI.post('/lesson-content',
                    { lesson_id: lessonId, title, type, path, is_active: isActive });
                if (res.ok) { done(); return; }
                showError(AdminUI.errText(res));
                return;
            }

            // 2) Kiçik/orta fayl -> nazik presigned PUT (birbaşa R2)
            const urlRes = await AdminUI.post('/lesson-content/upload-url',
                { lessonId, type, fileName: file.name, contentType });
            if (urlRes.ok && urlRes.data?.uploadUrl) {
                const put = await fetch(urlRes.data.uploadUrl,
                    { method: 'PUT', body: file, headers: { 'Content-Type': contentType } });

                if (put.ok) {
                    const res = await AdminUI.post('/lesson-content',
                        { lesson_id: lessonId, title, type, path: urlRes.data.path, is_active: isActive });
                    if (res.ok) { done(); return; }
                    showError(AdminUI.errText(res));
                    return;
                }
            }

            // 3) Ehtiyat: faylı serverə göndər — yalnız limitə sığan fayllar üçün
            if (file.size > SERVER_FALLBACK_MAX) { showError(L["Js_FileTooLarge"]); return; }

            const fd = new FormData();
            fd.append('lesson_id', lessonId);
            fd.append('title', title);
            fd.append('type', type);
            fd.append('is_active', isActive);
            fd.append('File', file);
            const res = await AdminUI.postForm('/lesson-content', fd);
            if (res.ok) { done(); return; }

            showError(AdminUI.errText(res));
        } catch (err) {
            showError(String(err && err.message ? err.message : err));
        } finally {
            setProgress(null);
            AdminUI.loader(false);
        }
    });
})();
