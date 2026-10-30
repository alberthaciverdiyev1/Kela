// Brauzer Web Push (VAPID) köməkçisi. Abunəlik JSON-u serverdə token kimi saxlanır.
window.edutorPush = (function () {
    const supported = () => 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;
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

    return {
        supported,

        permission: () => supported() ? Notification.permission : 'unsupported',

        // İcazə verilibsə mövcud abunəliyi, əks halda null qaytarır (istifadəçi hərəkəti tələb olunmur).
        current: async function () {
            if (!supported() || Notification.permission !== 'granted') return null;
            try { return await subscriptionJson(); } catch { return null; }
        },

        // Yalnız istifadəçi hərəkəti ilə çağırılmalıdır — brauzer icazə pəncərəsini göstərir.
        subscribe: async function (publicKey) {
            if (!supported() || !publicKey) return null;
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
