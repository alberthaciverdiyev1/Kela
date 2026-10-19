(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    function showError(message) { errBox.textContent = message; errBox.classList.remove('hidden'); }
    function done() { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); }

    function selectedLessonIds() {
        return [...document.getElementById('lesson_ids').selectedOptions].map(o => o.value);
    }


    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Common_NewMaterial"];
        document.getElementById('entityId').value = '';
        document.getElementById('sort_order').value = '';
        document.getElementById('description').value = '';
        document.getElementById('lesson_id').value = '';
        [...document.getElementById('lesson_ids').options].forEach(o => { o.selected = false; });
        document.getElementById('lesson_ids').dispatchEvent(new Event('change'));
        document.getElementById('title').value = '';
        document.getElementById('type').value = 'video';
        document.getElementById('is_active').checked = true;
        // Fayl girdisi ilə yanaşı önizləmə də sıfırlanmalıdır, əks halda əvvəlki
        // videonun kadrı modalda qalır.
        if (AdminUI.fileReset) AdminUI.fileReset('file');
        document.getElementById('fileBlock').classList.remove('hidden');
        document.getElementById('currentFile').classList.add('hidden');
        document.getElementById('lesson_id').disabled = false;
        document.getElementById('type').disabled = false;
        AdminUI.openModal('modal');
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

    function resetSpeed(totalBytes) {
        speedTracker = { total: totalBytes, lastAt: performance.now(), lastBytes: 0, samples: [] };
    }

    function startProgress(totalBytes) {
        resetSpeed(totalBytes);

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

    // Yarımçıq yükləmə itməsin deyə bağlanmanı dayandırırıq (ESC, fon klik, X, İmtina).
    document.getElementById('modal').addEventListener('modal:before-close', function (e) {
        if (uploading) e.preventDefault();
    });

    function lockModal(locked) {
        uploading = locked;

        ['saveBtn', 'cancelBtn', 'file'].forEach(function (id) {
            const el = document.getElementById(id);
            if (el) el.disabled = locked;
        });

        const close = document.querySelector('#modal [data-modal-close]');
        if (close) close.disabled = locked;
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

    async function uploadMultipart(file, lessonId, contentType, onProgress, session) {
        let path, uploadId, partSize;

        // Yalnız tam sessiya "davam" sayılır: yeni yükləmədə də sessiya obyekti
        // ötürülür, amma onda hələ uploadId/path olmur.
        if (session && session.uploadId && session.path) {
            ({ path, uploadId, partSize } = session);
        } else {
            const fd = new FormData();
            fd.append('lessonId', lessonId);
            fd.append('type', 'video');
            fd.append('fileName', file.name);
            fd.append('contentType', contentType);

            const startRes = await AdminUI.postForm('/lesson-content/upload/multipart/start', fd);
            if (!startRes.ok || !startRes.data?.uploadId) return null;

            ({ path, uploadId, partSize } = startRes.data);
        }

        const total = Math.ceil(file.size / partSize);

        try {
            // Artıq yüklənmiş hissələr təkrar göndərilmir.
            const doneParts = session?.parts || [];
            const doneNumbers = new Set(doneParts.map(p => p.partNumber));
            const missing = Array.from({ length: total }, (_, i) => i + 1).filter(n => !doneNumbers.has(n));

            const urlsRes = missing.length
                ? await AdminUI.postJson('/lesson-content/upload/multipart/parts', { path, uploadId, partNumbers: missing })
                : { ok: true, data: [] };
            if (!urlsRes.ok) throw new Error('parts');

            const parts = new Array(total);
            doneParts.forEach(p => { parts[p.partNumber - 1] = p; });
            const loaded = new Array(total).fill(0);
            doneParts.forEach(p => { loaded[p.partNumber - 1] = partSize; });
            if (doneParts.length) {
                const already = Math.min(file.size, doneParts.length * partSize);
                if (onProgress) onProgress(already, file.size);
            }
            let next = 0;

            const overall = () => loaded.reduce((a, b) => a + b, 0) / file.size * 100;

            const worker = async () => {
                while (true) {
                    const i = next++;
                    if (i >= urlsRes.data.length) return;

                    const u = urlsRes.data[i];
                    const blob = file.slice((u.partNumber - 1) * partSize, u.partNumber * partSize);

                    const put = await putWithProgress(u.url, blob, 'application/octet-stream',
                        (sent) => {
                            loaded[i] = sent;
                            const done = loaded.reduce((a, b) => a + b, 0);
                            showProgress(overall(), done, file.size);
                            if (onProgress) onProgress(done, file.size);
                        });

                    if (!put.ok) throw new Error('part ' + u.partNumber + ': ' + put.status);

                    loaded[i] = blob.size;
                    parts[u.partNumber - 1] = { partNumber: u.partNumber, eTag: put.etag };
                    // Sessiya yenilənir: səhifə bağlansa, qayıdanda davam etmək mümkün olsun.
                    saveSession({ ...session, path, uploadId, partSize, parts: parts.filter(Boolean) });
                    const totalDone = loaded.reduce((a, b) => a + b, 0);
                    showProgress(overall(), totalDone, file.size);
                    if (onProgress) onProgress(totalDone, file.size);
                }
            };

            await Promise.all(Array.from({ length: Math.min(MP_CONCURRENCY, total) }, worker));

            const finish = await AdminUI.postJson('/lesson-content/upload/multipart/complete',
                { path, uploadId, parts });
            if (!finish.ok) throw new Error('complete');

            return path;
        } catch (err) {
            // Yarımçıq yükləmə R2-də yer tutur və 7 gün saxlanılır — ləğv edirik.
            // path/uploadId olmadan abort R2-də xəta verir.
            if (path && uploadId) {
                AdminUI.postJson('/lesson-content/upload/multipart/abort', { path, uploadId });
            }
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

    let uploadInFlight = false;
    let uploadSession = null;

    // ---- Yarımçıq yükləmə sessiyası ----
    // Səhifə dəyişəndə brauzer faylı itirir; uploadId və tamamlanmış hissələr
    // yadda saxlanılır ki, istifadəçi qayıtdıqda yalnız qalan hissələr yüklənsin.
    const UPLOAD_SESSION = 'lms.upload.session';

    function saveSession(session) {
        try { localStorage.setItem(UPLOAD_SESSION, JSON.stringify(session)); } catch (e) { /* localStorage bağlı ola bilər */ }
    }

    function readSession() {
        try { return JSON.parse(localStorage.getItem(UPLOAD_SESSION) || 'null'); } catch (e) { return null; }
    }

    function clearSession() {
        try { localStorage.removeItem(UPLOAD_SESSION); } catch (e) { }
    }

    // Brauzerin öz xəbərdarlığı dizaynlaşdırıla bilmir; daxili keçidlərdə öz modalımızı
    // göstəririk. beforeunload isə tab bağlama/yeniləmə üçün son qoruyucu olaraq qalır.
    window.addEventListener('beforeunload', function (e) {
        if (!uploadInFlight) return;
        e.preventDefault();
        e.returnValue = '';
    });

    document.addEventListener('click', async function (e) {
        if (!uploadInFlight) return;

        const link = e.target.closest && e.target.closest('a[href]');
        if (!link || link.target === '_blank') return;

        const href = link.getAttribute('href') || '';
        if (!href || href.startsWith('#') || link.origin !== location.origin) return;

        e.preventDefault();
        e.stopPropagation();

        const res = await AdminUI.swal({
            title: L["Upload_LeaveTitle"],
            text: L["Upload_LeaveText"],
            icon: 'info',
            showCancelButton: true,
            confirmButtonText: L["Upload_LeaveConfirm"],
            cancelButtonText: L["Common_Cancel"]
        });
        if (res.isConfirmed) { uploadInFlight = false; location.href = link.href; }
    }, true);

    // ---- Sağ alt künc: fon rejimində gedən yükləmə ----
    // İstifadəçi modalı bağlayır, material artıq yaranıb, video isə hələ yüklənir.
    function widgetShow(fileName) {
        const w = document.getElementById('uploadWidget');
        if (!w) return;

        const bar = document.getElementById('uwBar');
        w.classList.remove('hidden');
        document.getElementById('uwTitle').textContent = fileName;
        document.getElementById('uwStatus').textContent = L["Upload_Preparing"].replace('{0}', '');
        document.getElementById('uwPercent').textContent = '0%';
        bar.style.width = '0%';
        bar.classList.remove('bg-emerald-500', 'bg-red-600');
        bar.classList.add('bg-primary');
        document.getElementById('uwSpeed').textContent = '';
        document.getElementById('uwRemaining').textContent = '';
    }

    function formatClock(seconds) {
        const s = Math.max(0, Math.round(seconds));
        const m = Math.floor(s / 60);
        return m > 0 ? m + ':' + String(s % 60).padStart(2, '0') : s + ' san';
    }

    function widgetProgress(loaded, total, speed) {
        const pct = total ? Math.round(loaded / total * 100) : 0;
        document.getElementById('uwPercent').textContent = pct + '%';
        document.getElementById('uwBar').style.width = pct + '%';
        document.getElementById('uwSpeed').textContent = speed ? formatBytes(speed) + '/s' : '';

        const remaining = document.getElementById('uwRemaining');
        if (speed > 0 && total > loaded) {
            remaining.textContent = L["Upload_Remaining"].replace('{0}', formatClock((total - loaded) / speed));
        } else {
            remaining.textContent = '';
        }
    }

    function widgetDone() {
        const bar = document.getElementById('uwBar');
        bar.style.width = '100%';
        bar.classList.remove('bg-primary');
        bar.classList.add('bg-emerald-500');
        document.getElementById('uwPercent').textContent = '100%';
        document.getElementById('uwStatus').textContent = L["Upload_Done"];
        document.getElementById('uwSpeed').textContent = '';
        document.getElementById('uwRemaining').textContent = '';

        setTimeout(function () {
            document.getElementById('uploadWidget').classList.add('hidden');
        }, 4000);
    }

    function widgetFail(message) {
        const bar = document.getElementById('uwBar');
        bar.classList.remove('bg-primary', 'bg-emerald-500');
        bar.classList.add('bg-red-600');
        document.getElementById('uwStatus').textContent = message || L["Upload_Failed"];
        document.getElementById('uwSpeed').textContent = '';
        document.getElementById('uwRemaining').textContent = '';
    }

    // ---- Yükləmə strategiyası ----
    async function uploadVideo(file, lessonId, contentType, onProgress, session) {
        if (file.size > MP_THRESHOLD) {
            return await uploadMultipart(file, lessonId, contentType, onProgress, session);
        }

        const urlRes = await AdminUI.post('/lesson-content/upload-url',
            { lessonId, type: 'video', fileName: file.name, contentType });
        if (!urlRes.ok || !urlRes.data?.uploadUrl) return null;

        const put = await putWithProgress(urlRes.data.uploadUrl, file, contentType, onProgress);
        return put.ok ? urlRes.data.path : null;
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

        // Video: istifadəçi yükləməni gözləmir — material dərhal yaranır, fayl fonda
        // yüklənib sonra bağlanır. Digər növlər kiçikdir: əvvəl yüklənir, sonra yaranır
        // (serverdən keçən ehtiyat yol da yalnız o axında mümkündür).
        if (type === 'video') {
            await submitDeferredVideo(file, lessonId, type, title, isActive, contentType);
            return;
        }

        try {
            lockModal(true);
            startProgress(file.size);

            const urlRes = await AdminUI.post('/lesson-content/upload-url',
                { lessonId, type, fileName: file.name, contentType });
            if (urlRes.ok && urlRes.data?.uploadUrl) {
                const put = await putWithProgress(urlRes.data.uploadUrl, file, contentType,
                    (sent, totalBytes) => showProgress(sent / totalBytes * 100, sent, totalBytes));

                if (put.ok) {
                    showSuccess(file.size);
                    const res = await AdminUI.post('/lesson-content',
                        { lesson_id: lessonId, lesson_ids: selectedLessonIds(), title, type, path: urlRes.data.path, is_active: isActive });
                    if (res.ok) { await finishSuccess(file.size); return; }
                    showError(AdminUI.errText(res));
                    return;
                }
            }

            // Ehtiyat: faylı serverə göndər — yalnız limitə sığan fayllar üçün
            if (file.size > SERVER_FALLBACK_MAX) { showError(L["Js_FileTooLarge"]); return; }

            const fd = new FormData();
            fd.append('lesson_id', lessonId);
            selectedLessonIds().forEach(id => fd.append('lesson_ids', id));
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

    // Materialı dərhal yaradır, modalı bağlayır və yükləməni fonda davam etdirir.
    async function submitDeferredVideo(file, lessonId, type, title, isActive, contentType) {
        try {
            lockModal(true);

            const created = await AdminUI.post('/lesson-content',
                { lesson_id: lessonId, lesson_ids: selectedLessonIds(), title, type, is_active: isActive, defer_file: 'true' });

            if (!created.ok) { showError(AdminUI.errText(created)); return; }

            const contentId = created.data?.id;
            if (!contentId) { showError(L["Js_ErrorOccured"]); return; }

            lockModal(false);
            AdminUI.closeModal('modal');
            loadList(1);

            widgetShow(file.name);
            resetSpeed(file.size);
            uploadInFlight = true;
            uploadSession = {
                contentId, lessonId,
                fileName: file.name, fileSize: file.size, fileLastModified: file.lastModified,
                contentType, parts: []
            };
            saveSession(uploadSession);

            let speed = 0;
            const path = await uploadVideo(file, lessonId, contentType, function (loaded, total) {
                speed = measureSpeed(loaded) || speed;
                widgetProgress(loaded, total, speed);
            }, uploadSession);

            if (!path) { widgetFail(L["Js_FileTooLarge"]); return; }

            // Yükləmə bitdi — faylı materiala bağlayırıq.
            // Form göndərilir: 'type' enum-dur və form binder onu adından çözür,
            // JSON binder isə yalnız rəqəm qəbul edərdi.
            const attached = await AdminUI.post(`/lesson-content/${contentId}/file`,
                { path: path, type: type });

            uploadInFlight = false;
            uploadSession = null;
            clearSession();

            if (attached.ok) { widgetDone(); loadList(1); }
            else { widgetFail(AdminUI.errText(attached)); }
        } catch (err) {
            // Nişan qəsdən silinmir: bura həm xəta, həm də səhifədən çıxış düşür.
            // Hər iki halda yükləmə tamamlanmayıb və növbəti açılışda istifadəçiyə demək lazımdır.
            uploadInFlight = false;
            widgetFail(String(err && err.message ? err.message : err));
        } finally {
            lockModal(false);
            hideProgress();
            AdminUI.loader(false);
        }
    }

    // Səhifə açılanda yarımçıq yükləmə varsa panel göstərilir; istifadəçi eyni
    // faylı seçəndə yalnız qalan hissələr göndərilir.
    function showResume() {
        const session = readSession();
        const panel = document.getElementById('resumePanel');
        if (!session || !panel) return;
        const hint = document.getElementById('resumeHint');
        if (hint) hint.textContent = (L["Upload_ResumeHint"] || '{0}').replace('{0}', session.fileName || '');
        panel.classList.remove('hidden');
        panel.classList.add('flex');
    }

    function hideResume() {
        const panel = document.getElementById('resumePanel');
        if (!panel) return;
        panel.classList.add('hidden');
        panel.classList.remove('flex');
    }

    window.cancelResume = async function () {
        const session = readSession();
        if (session?.uploadId) {
            // R2-də yer tutmasın.
            await AdminUI.postJson('/lesson-content/upload/multipart/abort',
                { path: session.path, uploadId: session.uploadId });
        }
        clearSession();
        hideResume();
    };

    window.resumeUpload = async function (input) {
        const session = readSession();
        const file = input.files[0];
        if (!session || !file) return;

        if (file.name !== session.fileName || file.size !== session.fileSize) {
            AdminUI.error(L["Upload_ResumeMismatch"]);
            input.value = '';
            return;
        }

        hideResume();
        widgetShow(file.name);
        resetSpeed(file.size);
        uploadInFlight = true;

        try {
            let speed = 0;
            const path = await uploadMultipart(file, session.lessonId, session.contentType, function (loaded, total) {
                speed = measureSpeed(loaded) || speed;
                widgetProgress(loaded, total, speed);
            }, session);

            if (!path) { widgetFail(L["Js_FileTooLarge"]); return; }

            const attached = await AdminUI.post(`/lesson-content/${session.contentId}/file`,
                { path, type: 'video' });

            uploadInFlight = false;
            clearSession();

            if (attached.ok) { widgetDone(); loadList(1); }
            else { widgetFail(AdminUI.errText(attached)); }
        } catch (err) {
            uploadInFlight = false;
            widgetFail(String(err && err.message ? err.message : err));
        } finally {
            input.value = '';
            if (readSession()) showResume();
        }
    };

    document.addEventListener('DOMContentLoaded', showResume);

})();
