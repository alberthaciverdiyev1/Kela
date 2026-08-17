import Alpine from 'alpinejs';
import axios from 'axios';
import deleteForm from './teacher/delete-form';

window.Alpine = Alpine;
window.axios = axios;

axios.defaults.headers.common['Accept'] = 'application/json';

// CSRF token-i hər istəkdə meta-dan təzə oxu (köhnə fetch davranışı).
// Niyə? Session regenerate olarsa (login/çıxış) və ya səhifə uzun müddət
// açıq qalarsa köhnə token-i göndərmək 419 CSRF mismatch verir.
axios.interceptors.request.use((config) => {
    config.headers['X-CSRF-TOKEN'] =
        document.querySelector('meta[name="csrf-token"]')?.content ?? '';
    return config;
});

/**
 * JSON köməkçisi (axios). Web səhifələri yalnız lazım olan hissəni
 * web controller endpointləri (məs. /teacher/..., /notes) ilə yeniləyir.
 * Frontend /api/v1/*-ə birbaşa toxunmur — bütün sorğular web controller
 * içindəki funksiyalar vasitəsilə servislərə gedir.
 */
window.KelaApi = async (method, url, body = null) => {
    try {
        const res = await axios({ method, url, data: body ?? undefined });
        return res.status === 204 ? null : res.data;
    } catch (err) {
        let msg = 'Xəta baş verdi.';
        if (err.response?.data?.message) msg = err.response.data.message;
        throw new Error(msg);
    }
};

/** Server-rendered fragment-i yenidən çəkir (axios). */
window.KelaFragment = async (url) => {
    try {
        const res = await axios.get(url, { responseType: 'text' });
        return res.data;
    } catch {
        throw new Error('Bölmə yüklənə bilmədi.');
    }
};

// Ümumi komponentlər (silmə formaları).
Alpine.data('deleteForm', deleteForm);

// Alpine.start() burada DEYİL — hər səhifənin öz entry faylı başladır
// (teacher/index.js, teacher/quiz-editor.js, teacher/workspace.js).
// start() idempotentdir, buna görə təhlükəsizdir.
