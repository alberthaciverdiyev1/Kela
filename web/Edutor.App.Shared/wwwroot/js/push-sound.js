// Tətbiq görünən halda push gələndə qısa bildiriş səsi çalır.
// Service worker (push-worker.js) görünən səhifəyə { type: 'edutor-push' } göndərir.
window.edutorSound = (function () {
    const src = '_content/Edutor.App.Shared/sounds/notification-2.wav';
    let audio;
    let unlocked = false;

    function player() {
        if (!audio) {
            audio = new Audio(src);
            audio.preload = 'auto';
        }
        return audio;
    }

    // Mobil brauzerlər yalnız istifadəçi hərəkətindən sonra səs çalmağa icazə verir;
    // ilk toxunuşda səssiz oynadıb dərhal dayandırırıq ki, sonrakı çalma keçsin.
    function unlock() {
        if (unlocked) return;
        unlocked = true;
        document.removeEventListener('pointerdown', unlock, true);
        document.removeEventListener('keydown', unlock, true);

        const a = player();
        a.muted = true;
        const playing = a.play();
        if (playing && playing.then) {
            playing.then(() => { a.pause(); a.currentTime = 0; a.muted = false; }).catch(() => { a.muted = false; });
        } else {
            a.muted = false;
        }
    }

    function play() {
        const a = player();
        try {
            a.currentTime = 0;
            const playing = a.play();
            if (playing && playing.catch) playing.catch(() => { /* autoplay bloklanıb */ });
        } catch { /* sükutla */ }
    }

    document.addEventListener('pointerdown', unlock, true);
    document.addEventListener('keydown', unlock, true);

    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.addEventListener('message', event => {
            if (event.data && event.data.type === 'edutor-push') play();
        });
    }

    return { play };
})();
