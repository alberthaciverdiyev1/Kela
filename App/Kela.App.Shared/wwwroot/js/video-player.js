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
// Böyük fayllarda yaddaş qənaəti: yalnız ekrana yaxınlaşan səhifələr render olunur.
window.lmsPdf = {
    _worker: new URL('_content/Kela.App.Shared/lib/pdfjs/pdf.worker.min.js', document.baseURI).href,
    _docs: new WeakMap(),
    _minZoom: 0.5,
    _maxZoom: 3,
    _step: 0.25,

    _message: function (container, text) {
        container.innerHTML = '';
        var box = document.createElement('div');
        box.className = 'mx-3 my-6 rounded-2xl bg-[#f6f8f7] px-4 py-6 text-center text-sm text-[#727c8d]';
        box.textContent = text || 'PDF';
        container.appendChild(box);
    },

    _zoomKey: function (url) { return 'lms.pdf.zoom:' + url; },

    _readZoom: function (url) {
        try {
            var value = Number(localStorage.getItem(this._zoomKey(url)));
            return isFinite(value) && value >= this._minZoom && value <= this._maxZoom ? value : 1;
        } catch (e) { return 1; }
    },

    _writeZoom: function (url, zoom) {
        try { localStorage.setItem(this._zoomKey(url), String(zoom)); } catch (e) { /* noop */ }
    },

    render: async function (container, url, errorText) {
        if (!container || !url) return;
        this.clear(container);
        container.classList.add('lms-pdf');

        var lib = window.pdfjsLib;
        if (!lib) { this._message(container, errorText); return; }

        var state = {
            url: url,
            zoom: this._readZoom(url),
            observer: null,
            rendered: new Set(),
            queue: Promise.resolve()
        };
        this._docs.set(container, state);

        var toolbar = document.createElement('div');
        toolbar.className = 'sticky top-0 z-10 flex items-center justify-center gap-1 bg-[#f0f2f1]/95 py-2 backdrop-blur';
        toolbar.innerHTML = `
            <button type="button" data-pdf-zoom="-1" aria-label="Zoom out" class="grid h-9 w-9 place-items-center rounded-full border border-[#dfe5e2] bg-white text-lg leading-none text-[#31423a]">−</button>
            <span data-pdf-zoom-label class="min-w-[4.5rem] text-center text-xs font-semibold text-[#4b5b53]"></span>
            <button type="button" data-pdf-zoom="1" aria-label="Zoom in" class="grid h-9 w-9 place-items-center rounded-full border border-[#dfe5e2] bg-white text-lg leading-none text-[#31423a]">+</button>
            <button type="button" data-pdf-fit class="ml-1 rounded-full border border-[#dfe5e2] bg-white px-3 py-1.5 text-xs font-semibold text-[#31423a]">Fit</button>`;
        container.appendChild(toolbar);

        var pages = document.createElement('div');
        pages.className = 'overflow-x-auto';
        var track = document.createElement('div');
        track.style.width = '100%';
        pages.appendChild(track);
        container.appendChild(pages);

        var label = toolbar.querySelector('[data-pdf-zoom-label]');
        var applyZoom = function () {
            state.zoom = Math.min(window.lmsPdf._maxZoom, Math.max(window.lmsPdf._minZoom, state.zoom));
            label.textContent = Math.round(state.zoom * 100) + '%';
            track.style.width = (state.zoom * 100) + '%';
        };
        applyZoom();

        toolbar.addEventListener('click', function (e) {
            var zoomBtn = e.target.closest('[data-pdf-zoom]');
            var fitBtn = e.target.closest('[data-pdf-fit]');

            if (zoomBtn) {
                state.zoom += Number(zoomBtn.dataset.pdfZoom) * window.lmsPdf._step;
                applyZoom();
                window.lmsPdf._writeZoom(url, state.zoom);
            } else if (fitBtn) {
                state.zoom = 1;
                applyZoom();
                window.lmsPdf._writeZoom(url, 1);
            }
        });

        try {
            lib.GlobalWorkerOptions.workerSrc = this._worker;
            var pdf = await lib.getDocument({ url: url, isEvalSupported: false }).promise;
            var first = await pdf.getPage(1);
            var base = first.getViewport({ scale: 1 });
            var width = container.clientWidth || window.innerWidth;
            var renderScale = Math.min(2.5, Math.max(1, (width - 24) / base.width));

            state.observer = new IntersectionObserver(function (entries) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) return;
                    state.observer.unobserve(entry.target);
                    renderPage(Number(entry.target.dataset.page), entry.target);
                });
            }, { rootMargin: '800px 0px' });

            var renderPage = function (n, wrap) {
                state.queue = state.queue.then(async function () {
                    if (state.rendered.has(n)) return;

                    var page = n === 1 ? first : await pdf.getPage(n);
                    var viewport = page.getViewport({ scale: renderScale });
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

                    await page.render({ canvasContext: canvas.getContext('2d'), viewport: viewport }).promise;
                    wrap.innerHTML = '';
                    wrap.appendChild(canvas);
                    state.rendered.add(n);
                }).catch(function () {
                    wrap.innerHTML = '';
                    wrap.textContent = errorText || 'PDF';
                });
            };

            for (var n = 1; n <= pdf.numPages; n++) {
                var wrap = document.createElement('div');
                wrap.dataset.page = String(n);
                wrap.style.width = '100%';
                wrap.style.padding = '0 12px 14px';
                wrap.style.aspectRatio = base.width + ' / ' + base.height;
                track.appendChild(wrap);

                if (n === 1) renderPage(1, wrap);
                else state.observer.observe(wrap);
            }
        } catch (e) {
            this._message(container, errorText);
        }
    },

    clear: function (container) {
        try {
            var state = container && this._docs.get(container);
            if (state && state.observer) state.observer.disconnect();
            if (container) {
                container.innerHTML = '';
                this._docs.delete(container);
            }
        } catch (e) { /* noop */ }
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
