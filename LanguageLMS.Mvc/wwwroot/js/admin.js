// Admin panel üçün ümumi köməkçilər: notify (toast), təsdiq alert-i, antiforgery-li fetch
window.AdminUI = (function () {
    function token() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function notify(message, icon) {
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: icon || 'success',
            title: message,
            showConfirmButton: false,
            timer: 2500,
            timerProgressBar: true
        });
    }

    function error(message) { notify(message || 'Xəta baş verdi', 'error'); }

    async function confirmDelete(text) {
        const result = await Swal.fire({
            title: 'Əminsiniz?',
            text: text || 'Bu əməliyyat silinəcək.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sil',
            cancelButtonText: 'İmtina',
            confirmButtonColor: '#dc2626',
            cancelButtonColor: '#64748b'
        });
        return result.isConfirmed;
    }

    async function get(url) { return (await fetch(url)).json(); }

    async function postForm(url, formData) {
        const res = await fetch(url, { method: 'POST', body: formData, headers: { 'RequestVerificationToken': token() } });
        let body = null;
        try { body = await res.json(); } catch (e) { /* boş */ }
        return { ok: res.ok, status: res.status, data: body };
    }

    async function post(url, data) {
        const fd = new FormData();
        Object.entries(data || {}).forEach(([k, v]) => fd.append(k, v));
        return postForm(url, fd);
    }

    function reloadAfter(delay) { setTimeout(() => location.reload(), delay || 700); }

    return { notify, error, confirmDelete, get, post, postForm, token, reloadAfter };
})();
