// Admin panel üçün ümumi köməkçilər: toast, təsdiq, antiforgery-li fetch, cədvəl, badge.
window.AdminUI = (function () {
    function token() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function notify(message, icon) {
        Swal.fire({
            toast: true, position: 'top-end', icon: icon || 'success', title: message,
            showConfirmButton: false, timer: 2500, timerProgressBar: true
        });
    }

    function error(message) { notify(message || 'Xəta baş verdi', 'error'); }

    async function confirmDelete(text) {
        const result = await Swal.fire({
            title: 'Əminsiniz?', text: text || 'Bu əməliyyat geri alına bilməz.',
            icon: 'warning', showCancelButton: true,
            confirmButtonText: 'Sil', cancelButtonText: 'İmtina',
            confirmButtonColor: '#ef4444', cancelButtonColor: '#64748b'
        });
        return result.isConfirmed;
    }

    async function get(url) { return (await fetch(url)).json(); }

    async function postForm(url, formData) {
        const res = await fetch(url, { method: 'POST', body: formData, headers: { 'RequestVerificationToken': token() } });
        let body = null; try { body = await res.json(); } catch (e) { /* boş */ }
        return { ok: res.ok, status: res.status, data: body };
    }

    async function post(url, data) {
        const fd = new FormData();
        Object.entries(data || {}).forEach(([k, v]) => fd.append(k, v));
        return postForm(url, fd);
    }

    async function putForm(url, formData) {
        const res = await fetch(url, { method: 'PUT', body: formData, headers: { 'RequestVerificationToken': token() } });
        let body = null; try { body = await res.json(); } catch (e) { /* boş */ }
        return { ok: res.ok, status: res.status, data: body };
    }

    async function postJson(url, obj) {
        const res = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token() },
            body: JSON.stringify(obj)
        });
        let body = null; try { body = await res.json(); } catch (e) { /* boş */ }
        return { ok: res.ok, status: res.status, data: body };
    }

    async function del(url) {
        const res = await fetch(url, { method: 'DELETE', headers: { 'RequestVerificationToken': token() } });
        let body = null; try { body = await res.json(); } catch (e) { /* boş */ }
        return { ok: res.ok, status: res.status, data: body };
    }

    // Xəta cavabından göstəriləcək mətni çıxarır: əvvəl validation errors, sonra message/error.
    function errText(res) {
        const d = res && res.data;
        if (d) {
            if (d.errors) return Object.values(d.errors).flat().join(' • ');
            if (d.message) return d.message;
            if (d.error) return d.error;
        }
        return 'Xəta baş verdi';
    }

    function badge(text, variant) {
        return `<span class="badge badge-${variant || 'neutral'}">${text}</span>`;
    }

    function pagination(containerId, currentPage, lastPage, onSelect) {
        const box = document.getElementById(containerId);
        if (!box) return;
        box.innerHTML = '';
        if (lastPage <= 1) return;
        for (let i = 1; i <= lastPage; i++) {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.textContent = i;
            btn.className = 'min-w-9 rounded-lg px-3 py-1.5 text-sm font-semibold transition ' +
                (i === currentPage ? 'bg-primary text-white' : 'border border-border bg-white text-slate-600 hover:bg-surface-secondary');
            btn.addEventListener('click', () => onSelect(i));
            box.appendChild(btn);
        }
    }

    // cədvəl sətri üçün standart "Redaktə / Sil" düymələri
    function actions(id, opts) {
        opts = opts || {};
        const parts = [];
        if (opts.extra) parts.push(opts.extra);
        parts.push(`<button type="button" class="inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-sm font-semibold text-primary hover:bg-primary-light" onclick="openEdit(${id})">Redaktə</button>`);
        parts.push(`<button type="button" class="inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-bg" onclick="removeItem(${id})">Sil</button>`);
        return `<div class="flex items-center justify-end gap-1">${parts.join('')}</div>`;
    }

    // rows: hər sətir = [{ html, cls }] ; opts = { selectable, rowIds, empty }
    function table(tbodyId, rows, colspan, opts) {
        const tbody = document.getElementById(tbodyId);
        if (!tbody) return;
        opts = opts || {};
        const selectable = !!opts.selectable;
        const rowIds = opts.rowIds || [];
        const cols = colspan + (selectable ? 1 : 0);

        tbody.innerHTML = '';

        if (!rows.length) {
            tbody.innerHTML = `<tr><td colspan="${cols}">
                <div class="admin-empty">
                    <svg class="h-10 w-10 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5"><path stroke-linecap="round" stroke-linejoin="round" d="M3 7h18M3 12h18M3 17h10" /></svg>
                    <p class="text-base font-semibold text-slate-500">Nəticə tapılmadı</p>
                    <p class="text-sm">${opts.empty || 'Axtarış şərtlərini dəyişdirib yenidən yoxlayın'}</p>
                </div></td></tr>`;
            updateBulkBar();
            return;
        }

        rows.forEach((cells, idx) => {
            const tr = document.createElement('tr');
            if (selectable) {
                const td = document.createElement('td');
                td.className = 'w-12';
                td.innerHTML = `<input type="checkbox" class="row-check h-5 w-5 rounded-md border-slate-300 text-primary focus:ring-primary" value="${rowIds[idx] ?? ''}" />`;
                tr.appendChild(td);
            }
            cells.forEach(cell => {
                const td = document.createElement('td');
                if (cell.cls) td.className = cell.cls;
                td.innerHTML = cell.html;
                tr.appendChild(td);
            });
            tbody.appendChild(tr);
        });

        if (selectable) wireSelection();
        updateBulkBar();
    }

    function wireSelection() {
        document.querySelectorAll('#rows .row-check').forEach(box => {
            box.addEventListener('change', () => { syncCheckAll(); updateBulkBar(); });
        });
        const all = document.getElementById('checkAll');
        if (all && !all.dataset.bound) {
            all.dataset.bound = '1';
            all.addEventListener('change', () => {
                document.querySelectorAll('#rows .row-check').forEach(box => { box.checked = all.checked; });
                updateBulkBar();
            });
        }
    }

    function syncCheckAll() {
        const all = document.getElementById('checkAll');
        if (!all) return;
        const boxes = [...document.querySelectorAll('#rows .row-check')];
        all.checked = boxes.length > 0 && boxes.every(b => b.checked);
        all.indeterminate = !all.checked && boxes.some(b => b.checked);
    }

    function selectedIds() {
        return [...document.querySelectorAll('#rows .row-check:checked')].map(b => Number(b.value));
    }

    function updateBulkBar() {
        const ids = selectedIds();
        const count = document.getElementById('bulkCount');
        if (count) count.textContent = `${ids.length} seçilib`;
        const bar = document.getElementById('bulkBar');
        if (bar) {
            bar.classList.toggle('hidden', ids.length === 0);
            bar.classList.toggle('flex', ids.length > 0);
        }
    }

    async function bulkDo(url, action, cb) {
        const ids = selectedIds();
        if (!ids.length) { notify('Heç nə seçilməyib', 'warning'); return; }
        if (action === 'delete' && !await confirmDelete(`${ids.length} element silinəcək.`)) return;
        const res = await postJson(url, { ids, action });
        if (res.ok) { notify('Tətbiq olundu'); if (typeof cb === 'function') cb(); }
        else error(res.data?.message || 'Xəta baş verdi');
    }

    function sortToggle() { /* səhifə xüsusi tətbiq edə bilər */ }

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
        el.classList.toggle('is-active', show);
    }

    function reloadAfter(delay) { setTimeout(() => location.reload(), delay || 700); }

    return { notify, error, errText, confirmDelete, get, post, postForm, putForm, postJson, del, badge, table, actions, selectedIds, bulkDo, sortToggle, loader, pagination, filePreview, fileSet, fileRemove, token, reloadAfter };
})();
