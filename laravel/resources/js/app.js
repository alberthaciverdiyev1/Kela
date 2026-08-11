import Alpine from 'alpinejs';

window.Alpine = Alpine;
Alpine.start();

/**
 * Session-auth JSON API helper (Sanctum web-guard fallback).
 * Web səhifələri yalnız lazım olan hissələri yeniləmək üçün /api/v1/* çağırır.
 */
const csrf = () => document.querySelector('meta[name="csrf-token"]')?.content ?? '';

window.KelaApi = async (method, url, body = null) => {
    const res = await fetch(url, {
        method,
        headers: {
            'Accept': 'application/json',
            'X-CSRF-TOKEN': csrf(),
            ...(body ? { 'Content-Type': 'application/json' } : {}),
        },
        body: body ? JSON.stringify(body) : null,
    });
    if (!res.ok) {
        let msg = 'Xəta baş verdi.';
        try { const j = await res.json(); msg = j.message || msg; } catch { /* json deyil */ }
        throw new Error(msg);
    }
    return res.status === 204 ? null : res.json();
};

/** Server-rendered fragment-i yenidən çəkir; JS DOM-a yalnız bu hissəni yazır. */
window.KelaFragment = async (url) => {
    const res = await fetch(url, { headers: { 'Accept': 'text/html', 'X-CSRF-TOKEN': csrf() } });
    if (!res.ok) throw new Error('Bölmə yüklənə bilmədi.');
    return res.text();
};

/**
 * İndeks səhifələrində silmə formaları:
 * <form method="POST" action="...destroy" data-api-delete="/api/v1/students/1" data-confirm="...">
 * JS confirm + API DELETE edir; JS olmayan tərzdə normal submit işləyir.
 */
document.addEventListener('submit', (e) => {
    const form = e.target.closest('form[data-api-delete]');
    if (!form) return;

    if (!window.confirm(form.dataset.confirm || 'Silmək istəyirsiniz?')) {
        e.preventDefault();
        return;
    }

    e.preventDefault();
    const btn = form.querySelector('[type="submit"]');
    if (btn) btn.disabled = true;

    KelaApi('DELETE', form.dataset.apiDelete)
        .then(() => { window.location.reload(); })
        .catch((err) => {
            window.alert(err.message);
            if (btn) btn.disabled = false;
        });
});
