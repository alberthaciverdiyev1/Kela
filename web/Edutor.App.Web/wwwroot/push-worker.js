self.addEventListener('push', event => {
    let payload = {};
    try {
        payload = event.data ? event.data.json() : {};
    } catch {
        payload = { title: 'Edutor', body: event.data ? event.data.text() : '' };
    }

    const title = payload.title || 'Edutor';
    const options = {
        body: payload.body || '',
        icon: payload.image || '/icon-192.png',
        badge: '/icon-192.png',
        data: { url: (payload.data && payload.data.url) || '/notifications' }
    };

    event.waitUntil(self.registration.showNotification(title, options));
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
