(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    function showError(message) { errBox.textContent = message; errBox.classList.remove('hidden'); }
    function done() { AdminUI.notify('Əlavə olundu'); window.closeModal(); loadList(1); }

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
        document.getElementById('lesson_id').value = '';
        document.getElementById('title').value = '';
        document.getElementById('type').value = 'video';
        document.getElementById('sort_order').value = 0;
        document.getElementById('is_active').checked = true;
        document.getElementById('file').value = '';
        document.getElementById('fileBlock').classList.remove('hidden');
        document.getElementById('lesson_id').disabled = false;
        document.getElementById('type').disabled = false;
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js idarə edir

        const file = document.getElementById('file').files[0];
        if (!file) { showError('Fayl seçin'); return; }

        const lessonId = document.getElementById('lesson_id').value;
        const type = document.getElementById('type').value;
        const title = document.getElementById('title').value;
        const sortOrder = document.getElementById('sort_order').value || '0';
        const isActive = document.getElementById('is_active').checked ? 'true' : 'false';
        const contentType = file.type || 'application/octet-stream';

        try {
            // 1) Presigned (birbaşa R2) cəhd
            const urlRes = await AdminUI.post('/lesson-content/upload-url', { lessonId, type, fileName: file.name, contentType });
            if (urlRes.ok && urlRes.data?.uploadUrl) {
                await fetch(urlRes.data.uploadUrl, { method: 'PUT', body: file, headers: { 'Content-Type': contentType } });
                const res = await AdminUI.post('/lesson-content',
                    { lesson_id: lessonId, title, type, path: urlRes.data.path, sort_order: sortOrder, is_active: isActive });
                if (res.ok) { done(); return; }
                showError(res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi');
                return;
            }

            // 2) Fallback: faylı serverə yüklə
            const fd = new FormData();
            fd.append('lesson_id', lessonId);
            fd.append('title', title);
            fd.append('type', type);
            fd.append('sort_order', sortOrder);
            fd.append('is_active', isActive);
            fd.append('File', file);
            const res = await AdminUI.postForm('/lesson-content', fd);
            if (res.ok) { done(); return; }

            showError(res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi');
        } finally {
            AdminUI.loader(false);
        }
    });
})();
