// In-app video player köməkçisi — m3u8 (HLS) dəstəyi ilə.
window.lmsVideoPlayer = {
    _hls: null,

    attach: function (video, url) {
        this.destroy();
        if (!video || !url) return;

        var isHls = /\.m3u8(\?|$)/i.test(url);

        if (!isHls) {
            video.src = url;
            video.load();
            return;
        }

        if (video.canPlayType('application/vnd.apple.mpegurl')) {
            // Safari / iOS native HLS
            video.src = url;
            video.load();
        } else if (window.Hls && window.Hls.isSupported()) {
            var hls = new window.Hls();
            hls.loadSource(url);
            hls.attachMedia(video);
            this._hls = hls;
        } else {
            video.src = url;
        }
    },

    destroy: function () {
        if (this._hls) {
            try { this._hls.destroy(); } catch (e) { /* noop */ }
            this._hls = null;
        }
    }
};

// In-app PDF görüntüləyici — pdf.js ilə (mobil brauzerlərdə iframe işləmədiyi üçün).
window.lmsPdf = {
    _worker: 'https://cdn.jsdelivr.net/npm/pdfjs-dist@3.11.174/build/pdf.worker.min.js',

    fallbackIframe: function (container, url) {
        container.innerHTML = '';
        var f = document.createElement('iframe');
        f.src = url;
        f.style.width = '100%';
        f.style.height = '80vh';
        f.style.border = '0';
        container.appendChild(f);
    },

    render: async function (container, url) {
        if (!container || !url) return;
        container.innerHTML = '';

        var lib = window.pdfjsLib;
        if (!lib) { this.fallbackIframe(container, url); return; }

        try {
            lib.GlobalWorkerOptions.workerSrc = this._worker;
            var pdf = await lib.getDocument(url).promise;
            for (var n = 1; n <= pdf.numPages; n++) {
                var page = await pdf.getPage(n);
                var viewport = page.getViewport({ scale: 1.5 });
                var canvas = document.createElement('canvas');
                canvas.width = viewport.width;
                canvas.height = viewport.height;
                canvas.style.width = '100%';
                canvas.style.height = 'auto';
                canvas.style.display = 'block';
                canvas.style.marginBottom = '10px';
                canvas.style.borderRadius = '8px';
                container.appendChild(canvas);
                await page.render({ canvasContext: canvas.getContext('2d'), viewport: viewport }).promise;
            }
        } catch (e) {
            this.fallbackIframe(container, url);
        }
    },

    clear: function (container) {
        try { if (container) container.innerHTML = ''; } catch (e) { /* noop */ }
    }
};

// Alt bölmə audio pleyeri — timeline, seek və sürət üçün.
window.lmsAudio = {
    _el: null,
    _get: function () {
        if (!this._el) {
            this._el = new Audio();
            this._el.preload = 'metadata';
        }
        return this._el;
    },
    load: function (url) {
        var a = this._get();
        a.pause();
        a.src = url;
        a.load();
    },
    play: function () { return this._get().play(); },
    pause: function () { this._get().pause(); },
    seek: function (t) { try { this._get().currentTime = t || 0; } catch (e) { /* noop */ } },
    setRate: function (r) { try { this._get().playbackRate = r || 1; } catch (e) { /* noop */ } },
    stop: function () {
        var a = this._get();
        a.pause();
        a.removeAttribute('src');
        try { a.load(); } catch (e) { /* noop */ }
    },
    state: function () {
        var a = this._get();
        return {
            current: a.currentTime || 0,
            duration: (isFinite(a.duration) ? a.duration : 0),
            paused: a.paused,
            rate: a.playbackRate || 1
        };
    }
};
