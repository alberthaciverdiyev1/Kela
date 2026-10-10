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

    async function putForm(url, formData) {
        const res = await fetch(url, { method: 'PUT', body: formData, headers: { 'RequestVerificationToken': token() } });
        let body = null;
        try { body = await res.json(); } catch (e) { /* boş */ }
        return { ok: res.ok, status: res.status, data: body };
    }

    async function del(url) {
        const res = await fetch(url, { method: 'DELETE', headers: { 'RequestVerificationToken': token() } });
        let body = null;
        try { body = await res.json(); } catch (e) { /* boş */ }
        return { ok: res.ok, status: res.status, data: body };
    }

    function pagination(containerId, currentPage, lastPage, onSelect) {
        const box = document.getElementById(containerId);
        if (!box) return;
        box.innerHTML = '';
        for (let i = 1; i <= lastPage; i++) {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.textContent = i;
            btn.className = 'rounded-lg px-3 py-1.5 text-sm ' +
                (i === currentPage ? 'bg-primary text-white' : 'border border-slate-200 bg-white text-slate-600 hover:bg-slate-50');
            btn.addEventListener('click', () => onSelect(i));
            box.appendChild(btn);
        }
    }

    function loader(show) {
        const el = document.getElementById('listLoader');
        if (!el) return;
        el.classList.toggle('hidden', !show);
        el.classList.toggle('flex', show);
    }

    function reloadAfter(delay) { setTimeout(() => location.reload(), delay || 700); }

    return { notify, error, confirmDelete, get, post, postForm, putForm, del, loader, pagination, token, reloadAfter };
})();
