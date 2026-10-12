// In-app video player köməkçisi — m3u8 (HLS) dəstəyi ilə.
window.lmsVideoPlayer = {
    _hls: null,

    attach: function (video, url) {
        this.destroy();
        if (!video || !url) return;

        var isHls = /\.m3u8(\?|$)/i.test(url);

        if (!isHls) {
            video.src = url;
            return;
        }

        if (video.canPlayType('application/vnd.apple.mpegurl')) {
            // Safari / iOS native HLS
            video.src = url;
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
