self.addEventListener('push', event => {
    let payload = {};
    try {
        payload = event.data ? event.data.json() : {};
    } catch {
        payload = { title: 'Edutor', body: event.data ? event.data.text() : '' };
    }

    const title = payload.title || 'Edutor';

    event.waitUntil((async () => {
        // Görünən səhifə varsa səs orada çalınır; əks halda OS bildiriş səsi işləyir.
        // Beləliklə eyni bildiriş üçün iki səs eşidilmir.
        const windows = await clients.matchAll({ type: 'window', includeUncontrolled: true });
        const visible = windows.filter(c => c.visibilityState === 'visible');
        visible.forEach(c => c.postMessage({ type: 'edutor-push' }));

        await self.registration.showNotification(title, {
            body: payload.body || '',
            icon: payload.image || '/icon-192.png',
            badge: '/icon-192.png',
            silent: visible.length > 0,
            // Səs faylı mütləq yolla verilir; dəstəkləməyən brauzerlər (iOS) OS səsi çalır.
            sound: '/_content/Edutor.App.Shared/sounds/notification-2.wav',
            vibrate: visible.length > 0 ? undefined : [200, 100, 200],
            data: { url: (payload.data && payload.data.url) || '/notifications' }
        });
    })());
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    const url = event.notification.data && event.notification.data.url ? event.notification.data.url : '/notifications';

    event.waitUntil(clients.matchAll({ type: 'window', includeUncontrolled: true }).then(list => {
        for (const client of list) {
            if (client.url.startsWith(self.location.origin) && 'focus' in client) return client.focus();
        }
        return clients.openWindow(url);
    }));
});
