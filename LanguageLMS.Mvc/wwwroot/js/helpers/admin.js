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

    async function postJson(url, obj) {
        const res = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token() },
            body: JSON.stringify(obj)
        });
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
            btn.className = 'rounded-xl px-3 py-1.5 text-sm font-semibold ' +
                (i === currentPage ? 'bg-primary text-white' : 'border border-border bg-white text-slate-600 hover:bg-surface-secondary');
            btn.addEventListener('click', () => onSelect(i));
            box.appendChild(btn);
        }
    }

    // cədvəl sətri üçün standart "Redaktə / Sil" düymələri
    function actions(id) {
        return `<div class="flex items-center justify-end gap-2">
            <button type="button" class="px-2 py-1 text-primary hover:bg-primary/10 rounded-lg" onclick="openEdit(${id})">Redaktə</button>
            <button type="button" class="px-2 py-1 text-danger hover:bg-danger-bg rounded-lg" onclick="removeItem(${id})">Sil</button>
        </div>`;
    }

    // rows: hər sətir = [{ html, cls }], boş olduqda colspan-lı "Nəticə yoxdur"
    function table(tbodyId, rows, colspan) {
        const tbody = document.getElementById(tbodyId);
        if (!tbody) return;
        tbody.innerHTML = '';

        if (!rows.length) {
            tbody.innerHTML = `<tr><td colspan="${colspan}" class="px-4 py-10 text-center text-slate-400">Nəticə yoxdur</td></tr>`;
            return;
        }

        rows.forEach(cells => {
            const tr = document.createElement('tr');
            tr.className = 'h-[62px] hover:bg-primary-light/35';
            cells.forEach(cell => {
                const td = document.createElement('td');
                td.className = 'px-3 py-3 align-middle ' + (cell.cls || '');
                td.innerHTML = cell.html;
                tr.appendChild(td);
            });
            tbody.appendChild(tr);
        });
    }

    function setPreview(id, url) {
        const img = document.getElementById(id + '_preview');
        if (!img) return;
        img.src = url;
        img.classList.remove('hidden');
        document.getElementById(id + '_x').classList.remove('hidden');
        const nameEl = document.getElementById(id + 'Name');
        if (nameEl) nameEl.textContent = '';
        document.getElementById(id + '_remove').value = 'false';
    }

    function filePreview(id) {
        const input = document.getElementById(id);
        if (!input || !input.files.length) return;
        setPreview(id, URL.createObjectURL(input.files[0]));
    }

    function fileSet(id, url) { if (url) setPreview(id, url); }

    function fileRemove(id) {
        document.getElementById(id + '_preview').classList.add('hidden');
        document.getElementById(id + '_x').classList.add('hidden');
        document.getElementById(id + '_remove').value = 'true';
        const input = document.getElementById(id);
        if (input) input.value = '';
    }

    function loader(show) {
        const el = document.getElementById('listLoader');
        if (!el) return;
        el.classList.toggle('hidden', !show);
        el.classList.toggle('flex', show);
    }

    function reloadAfter(delay) { setTimeout(() => location.reload(), delay || 700); }

    return { notify, error, confirmDelete, get, post, postForm, putForm, postJson, del, table, actions, loader, pagination, filePreview, fileSet, fileRemove, token, reloadAfter };
})();
