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
        if (uploading) return;   // yarımçıq yükləmə itməsin

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

    function formatBytes(bytes) {
        if (bytes >= 1024 * 1024) return (bytes / 1048576).toFixed(1) + ' MB';
        if (bytes >= 1024) return Math.round(bytes / 1024) + ' KB';
        return Math.round(bytes) + ' B';
    }

    // Sürət ani ölçmələrlə təlaşlı olur; son bir neçə nümunənin ortası götürülür.
    let speedTracker = null;

    function startProgress(totalBytes) {
        speedTracker = { total: totalBytes, lastAt: performance.now(), lastBytes: 0, samples: [] };

        const box = document.getElementById('uploadProgress');
        if (box) box.classList.remove('hidden');

        setProgressText(0, 0);
    }

    function measureSpeed(bytes) {
        if (!speedTracker) return null;

        const now = performance.now();
        const seconds = (now - speedTracker.lastAt) / 1000;
        if (seconds < 0.3) return null;   // çox qısa aralıq — etibarsız ölçmə

        const delta = bytes - speedTracker.lastBytes;
        speedTracker.lastAt = now;
        speedTracker.lastBytes = bytes;

        speedTracker.samples.push(delta / seconds);
        if (speedTracker.samples.length > 8) speedTracker.samples.shift();

        const sum = speedTracker.samples.reduce((a, b) => a + b, 0);
        return sum / speedTracker.samples.length;
    }

    function setProgressText(loaded, total) {
        const sizeEl = document.getElementById('uploadSize');
        const speedEl = document.getElementById('uploadSpeed');

        if (sizeEl && total) {
            sizeEl.textContent = formatBytes(loaded) + ' / ' + formatBytes(total);
        }

        if (speedEl) {
            const speed = measureSpeed(loaded);
            speedEl.textContent = speed ? formatBytes(speed) + '/s' : '';
        }
    }

    function showProgress(percent, loaded, total) {
        const box = document.getElementById('uploadProgress');
        const bar = document.getElementById('uploadBar');
        const label = document.getElementById('uploadPercent');
        if (!box || !bar) return;

        const value = Math.max(0, Math.min(100, Math.round(percent)));
        box.classList.remove('hidden');
        bar.style.width = value + '%';
        if (label) label.textContent = value + '%';

        if (loaded != null && total) setProgressText(loaded, total);
    }

    function hideProgress() {
        const box = document.getElementById('uploadProgress');
        const bar = document.getElementById('uploadBar');
        const speedEl = document.getElementById('uploadSpeed');
        const sizeEl = document.getElementById('uploadSize');

        const label = document.getElementById('uploadLabel');

        if (box) box.classList.add('hidden');
        if (bar) {
            bar.style.width = '0%';
            bar.classList.remove('bg-emerald-500');
            bar.classList.add('bg-primary');
        }
        if (label) label.textContent = L["Common_Uploading"];
        if (speedEl) speedEl.textContent = '';
        if (sizeEl) sizeEl.textContent = '';
        speedTracker = null;
    }

    // Yükləmə gedərkən modal bağlanmasın: yarımçıq yükləmə itir və R2-də yetim
    // obyekt qalır. Ona görə saxla/iptal/bağla düymələri və fayl seçimi kilidlənir.
    let uploading = false;

    function lockModal(locked) {
        uploading = locked;

        ['saveBtn', 'cancelBtn', 'modalCloseBtn', 'file'].forEach(function (id) {
            const el = document.getElementById(id);
            if (el) el.disabled = locked;
        });
    }

    function showSuccess(totalBytes) {
        const label = document.getElementById('uploadLabel');
        const bar = document.getElementById('uploadBar');
        const speed = document.getElementById('uploadSpeed');
        const size = document.getElementById('uploadSize');
        const percent = document.getElementById('uploadPercent');

        if (label) label.textContent = L["Common_UploadDone"];
        if (bar) {
            bar.style.width = '100%';
            bar.classList.remove('bg-primary');
            bar.classList.add('bg-emerald-500');
        }
        if (percent) percent.textContent = '100%';
        if (speed) speed.textContent = '';
        if (size && totalBytes) size.textContent = formatBytes(totalBytes) + ' / ' + formatBytes(totalBytes);
    }

    // fetch yükləmə gedişatını bildirmir — irəliləyiş çubuğu üçün XHR lazımdır.
    function putWithProgress(url, blob, contentType, onProgress) {
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            xhr.open('PUT', url, true);
            xhr.setRequestHeader('Content-Type', contentType);

            if (xhr.upload) {
                xhr.upload.onprogress = (e) => {
                    if (e.lengthComputable) onProgress(e.loaded, e.total);
                };
            }

            xhr.onload = () => resolve({
                ok: xhr.status >= 200 && xhr.status < 300,
                status: xhr.status,
                etag: xhr.getResponseHeader('ETag')
            });
            xhr.onerror = () => reject(new Error('network'));
            xhr.onabort = () => reject(new Error('aborted'));
            xhr.send(blob);
        });
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
            const loaded = new Array(total).fill(0);
            let next = 0;

            const overall = () => loaded.reduce((a, b) => a + b, 0) / file.size * 100;

            const worker = async () => {
                while (true) {
                    const i = next++;
                    if (i >= total) return;

                    const u = urlsRes.data[i];
                    const blob = file.slice((u.partNumber - 1) * partSize, u.partNumber * partSize);

                    const put = await putWithProgress(u.url, blob, 'application/octet-stream',
                        (sent) => {
                            loaded[i] = sent;
                            showProgress(overall(), loaded.reduce((a, b) => a + b, 0), file.size);
                        });

                    if (!put.ok) throw new Error('part ' + u.partNumber + ': ' + put.status);

                    loaded[i] = blob.size;
                    parts[u.partNumber - 1] = { partNumber: u.partNumber, eTag: put.etag };
                    showProgress(overall(), loaded.reduce((a, b) => a + b, 0), file.size);
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

    // Uğur vəziyyəti bir an görünsün, sonra modal bağlansın.
    async function finishSuccess(totalBytes) {
        showSuccess(totalBytes);
        await new Promise(r => setTimeout(r, 700));
        lockModal(false);
        done();
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
            lockModal(true);
            startProgress(file.size);

            // 1) Böyük fayl -> çoxhissəli yükləmə (paralel, hissə-hissə bərpa oluna bilən)
            if (file.size > MP_THRESHOLD) {
                const path = await uploadMultipart(file, lessonId, type, contentType);
                if (!path) { showError(L["Js_FileTooLarge"]); return; }

                showSuccess(file.size);
                const res = await AdminUI.post('/lesson-content',
                    { lesson_id: lessonId, title, type, path, is_active: isActive });
                if (res.ok) { await finishSuccess(file.size); return; }
                showError(AdminUI.errText(res));
                return;
            }

            // 2) Kiçik/orta fayl -> nazik presigned PUT (birbaşa R2)
            const urlRes = await AdminUI.post('/lesson-content/upload-url',
                { lessonId, type, fileName: file.name, contentType });
            if (urlRes.ok && urlRes.data?.uploadUrl) {
                const put = await putWithProgress(urlRes.data.uploadUrl, file, contentType,
                    (sent, totalBytes) => showProgress(sent / totalBytes * 100, sent, totalBytes));

                if (put.ok) {
                    showSuccess(file.size);
                    const res = await AdminUI.post('/lesson-content',
                        { lesson_id: lessonId, title, type, path: urlRes.data.path, is_active: isActive });
                    if (res.ok) { await finishSuccess(file.size); return; }
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
            lockModal(false);
            hideProgress();
            AdminUI.loader(false);
        }
    });
})();
