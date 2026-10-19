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

// In-app PDF görüntüləyici — pdf.js ilə. Səhifələr canvas-a çəkilir, ona görə
// native PDF pəncərəsi (endirmə/çap düymələri ilə) heç vaxt açılmır.
window.lmsPdf = {
    _worker: new URL('_content/Kela.App.Shared/lib/pdfjs/pdf.worker.min.js', document.baseURI).href,

    _message: function (container, text) {
        container.innerHTML = '';
        var box = document.createElement('div');
        box.className = 'mx-3 my-6 rounded-2xl bg-[#f6f8f7] px-4 py-6 text-center text-sm text-[#727c8d]';
        box.textContent = text || 'PDF';
        container.appendChild(box);
    },

    render: async function (container, url, errorText) {
        if (!container || !url) return;
        container.innerHTML = '';
        container.classList.add('lms-pdf');

        var lib = window.pdfjsLib;
        if (!lib) { this._message(container, errorText); return; }

        try {
            lib.GlobalWorkerOptions.workerSrc = this._worker;
            var pdf = await lib.getDocument({ url: url, isEvalSupported: false }).promise;
            var width = container.clientWidth || window.innerWidth;

            for (var n = 1; n <= pdf.numPages; n++) {
                var page = await pdf.getPage(n);
                var base = page.getViewport({ scale: 1 });
                // Səhifə konteyner eninə sığdırılır; 2.5x-dən yuxarı böyütmə mənasızdır.
                var scale = Math.min(2.5, Math.max(1, (width - 24) / base.width));
                var viewport = page.getViewport({ scale: scale });

                var canvas = document.createElement('canvas');
                canvas.className = 'lms-pdf-page';
                canvas.width = Math.floor(viewport.width);
                canvas.height = Math.floor(viewport.height);
                canvas.style.width = '100%';
                canvas.style.height = 'auto';
                canvas.style.display = 'block';
                canvas.style.borderRadius = '14px';
                canvas.style.background = '#fff';
                canvas.style.boxShadow = '0 2px 14px rgba(15, 43, 30, .08)';

                var wrap = document.createElement('div');
                wrap.style.padding = '0 12px 14px';
                wrap.appendChild(canvas);
                container.appendChild(wrap);

                await page.render({ canvasContext: canvas.getContext('2d'), viewport: viewport }).promise;
            }
        } catch (e) {
            this._message(container, errorText);
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
