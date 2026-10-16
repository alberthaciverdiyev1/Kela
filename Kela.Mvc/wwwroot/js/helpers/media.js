// Media oynadıcıları üçün ortaq köməkçi: HLS bağlanması və fullscreen.
const AdminMedia = (() => {
    let hls = null;

    function destroy() {
        if (hls) { try { hls.destroy(); } catch (e) { } hls = null; }
    }

    // m3u8-i yalnız <video src> ilə oynatmaq olmur (Safari xaric) — hls.js lazımdır.
    function attach(video, url) {
        destroy();
        if (/\.m3u8(\?|$)/i.test(url)
            && !video.canPlayType('application/vnd.apple.mpegurl')
            && window.Hls && window.Hls.isSupported()) {
            hls = new window.Hls();
            hls.loadSource(url);
            hls.attachMedia(video);
        } else {
            video.src = url;
        }
    }

    // Şablonla yazılmış media elementlərini (data-src) mənbəyə bağlayır.
    function hydrate(root) {
        root.querySelectorAll('video[data-src]').forEach(v => attach(v, v.dataset.src));
        root.querySelectorAll('audio[data-src]').forEach(a => { a.src = a.dataset.src; });
    }

    function toggleFullscreen(el) {
        if (!el) return;
        if (document.fullscreenElement) document.exitFullscreen();
        else if (el.requestFullscreen) el.requestFullscreen();
    }

    return { destroy, hydrate, toggleFullscreen };
})();
