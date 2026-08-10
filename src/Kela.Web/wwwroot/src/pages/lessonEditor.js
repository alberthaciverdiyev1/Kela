(function () {
    'use strict';

    const page = document.getElementById('lesson-editor');
    if (!page) return;

    const contentId = Number(page.dataset.contentId);
    const streamUrl = page.dataset.streamUrl || '';
    const thumbUrl = page.dataset.thumbUrl || '';

    const videoEl = document.getElementById('lesson-video');
    const durationEl = document.getElementById('lesson-duration');
    const thumbEl = document.getElementById('lesson-thumb');

    function t(key) {
        return Kela.t(key);
    }

    function esc(v) {
        return Kela.esc ? Kela.esc(String(v == null ? '' : v)) : String(v);
    }

    function fmtDuration(seconds) {
        const s = Number(seconds) || 0;
        const h = Math.floor(s / 3600);
        const m = Math.floor((s % 3600) / 60);
        const sec = s % 60;
        return h > 0
            ? h + ':' + String(m).padStart(2, '0') + ':' + String(sec).padStart(2, '0')
            : String(m).padStart(2, '0') + ':' + String(sec).padStart(2, '0');
    }

    function setVideo(url, poster) {
        if (!videoEl) return;
        videoEl.src = url;
        if (poster) videoEl.poster = poster;
        videoEl.load();
    }

    if (videoEl && streamUrl) {
        setVideo(streamUrl, thumbUrl);
    }

    function uploadVideo(file) {
        const fd = new FormData();
        fd.append('file', file);
        return Kela.axios.post('/teacher/lessons/' + contentId + '/video', fd).then(function (res) {
            const d = res.data || {};
            if (!d.success) {
                Kela.notify.error(d.message || t('common.error'));
                return;
            }
            const dialog = document.getElementById('lesson-video-dialog');
            if (dialog && dialog.open) dialog.close();
            Kela.notify.success(t('lessons.videoUploaded'));
            // Re-render: show the video player + new duration/thumbnail.
            location.reload();
        }).catch(function (e) {
            Kela.notify.error(e && e.response && e.response.data && e.response.data.message
                ? e.response.data.message
                : t('common.error'));
        });
    }

    // Inline upload: clicking the dashed empty-state card opens the file picker.
    const inlineInput = document.getElementById('lesson-video-file-inline');
    if (inlineInput) {
        inlineInput.addEventListener('change', function () {
            const file = inlineInput.files && inlineInput.files[0];
            inlineInput.value = '';
            if (!file) return;
            uploadVideo(file);
        });
    }

    const videoForm = document.getElementById('lesson-video-form');
    if (videoForm) {
        videoForm.addEventListener('submit', function (event) {
            event.preventDefault();
            const fileInput = document.getElementById('lesson-video-file');
            const file = fileInput.files && fileInput.files[0];
            if (!file) return;

            const btn = videoForm.querySelector('button[type="submit"]');
            if (btn) btn.disabled = true;

            uploadVideo(file).finally(function () {
                if (btn) btn.disabled = false;
                fileInput.value = '';
            });
        });
    }

    const orderForm = document.getElementById('lesson-order-form');
    if (orderForm) {
        orderForm.addEventListener('submit', function (event) {
            event.preventDefault();
            const input = document.getElementById('lesson-order');
            const order = Number(input.value);
            if (isNaN(order) || order < 0) return;
            const btn = orderForm.querySelector('button[type="submit"]');
            if (btn) btn.disabled = true;
            Kela.axios.post('/teacher/lessons/' + contentId + '/order', { orderIndex: order }).then(function () {
                Kela.notify.success(t('common.saved'));
            }).catch(function (e) {
                Kela.notify.error(e && e.response && e.response.data && e.response.data.message
                    ? e.response.data.message
                    : t('common.error'));
            }).finally(function () {
                if (btn) btn.disabled = false;
            });
        });
    }
})();
