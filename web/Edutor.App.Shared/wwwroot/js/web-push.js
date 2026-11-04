// Brauzer Web Push (VAPID) köməkçisi. Abunəlik JSON-u serverdə token kimi saxlanır.
window.edutorPush = (function () {
    const supported = () => 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;

    // iOS-da bildiriş yalnız "Ana ekrana əlavə et" edilmiş tətbiqdə soruşulur;
    // Safari tabında requestPermission() sükutla heç nə etmir.
    const isIos = () => /iPad|iPhone|iPod/.test(navigator.userAgent)
        || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
    const isStandalone = () => window.matchMedia?.('(display-mode: standalone)').matches
        || navigator.standalone === true;

    let registration = null;

    async function ensureRegistration() {
        if (!registration) {
            registration = await navigator.serviceWorker.register('/push-worker.js', { scope: '/' });
        }
        await navigator.serviceWorker.ready;
        return registration;
    }

    function decode(base64Url) {
        const padding = '='.repeat((4 - (base64Url.length % 4)) % 4);
        const base64 = (base64Url + padding).replace(/-/g, '+').replace(/_/g, '/');
        const raw = atob(base64);
        return Uint8Array.from(raw, c => c.charCodeAt(0));
    }

    async function subscriptionJson() {
        const reg = await ensureRegistration();
        const sub = await reg.pushManager.getSubscription();
        return sub ? JSON.stringify(sub.toJSON()) : null;
    }

    async function subscribeNow(publicKey) {
        if (!supported() || !publicKey || (isIos() && !isStandalone())) return null;
        try {
            const reg = await ensureRegistration();
            let sub = await reg.pushManager.getSubscription();
            if (!sub) {
                sub = await reg.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: decode(publicKey)
                });
            }
            return JSON.stringify(sub.toJSON());
        } catch {
            return null;
        }
    }

    return {
        supported,

        // iOS Safari tabında push icazəsi soruşula bilmir; istifadəçi əvvəlcə
        // tətbiqi ana ekrana əlavə etməlidir (iOS 16.4+).
        needsInstall: () => isIos() && !isStandalone(),
        isStandalone,
        isIos,

        permission: () => supported() ? Notification.permission : 'unsupported',

        // İcazə verilibsə mövcud abunəliyi, əks halda null qaytarır (istifadəçi hərəkəti tələb olunmur).
        current: async function () {
            if (!supported() || Notification.permission !== 'granted') return null;
            try { return await subscriptionJson(); } catch { return null; }
        },

        // Yalnız istifadəçi hərəkəti ilə çağırılmalıdır — brauzer icazə pəncərəsini göstərir.
        subscribe: subscribeNow,

        // İstifadəçi klikindən ÇAĞIRILMALIDIR: ilk await birbaşa brauzer icazə
        // sorğusudur, əvvəlcədən HTTP gözləməsi klik jestini itirir (xüsusən Safari).
        request: async function (publicKey) {
            if (!supported() || !publicKey) return null;
            if (isIos() && !isStandalone()) return null;

            if (Notification.permission === 'default') {
                try { await Notification.requestPermission(); } catch { return null; }
            }
            if (Notification.permission !== 'granted') return null;

            return await subscribeNow(publicKey);
        },

        unsubscribe: async function () {
            try {
                const reg = await navigator.serviceWorker.getRegistration();
                const sub = await reg?.pushManager.getSubscription();
                await sub?.unsubscribe();
            } catch { /* sükutla */ }
        }
    };
})();
