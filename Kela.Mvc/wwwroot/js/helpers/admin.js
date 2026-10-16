// Kela admin panel — 
// Parçalanmışdır: hər fayl eyni AdminUI obyektini genişləndirir.
window.AdminUI = window.AdminUI || {};

(function (UI) {
    function token() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    const SWAL_BASE = {
        buttonsStyling: false,
        reverseButtons: true,
        customClass: {
            popup: 'admin-swal',
            title: 'admin-swal-title',
            htmlContainer: 'admin-swal-text',
            actions: 'admin-swal-actions',
            confirmButton: 'admin-button-primary',
            cancelButton: 'admin-button-secondary'
        }
    };

    function swal(options) {
        return Swal.fire(Object.assign({}, SWAL_BASE, options));
    }

    function notify(message, icon) {
        Swal.fire(Object.assign({}, SWAL_BASE, {
            toast: true, position: 'top-end', icon: icon || 'success', title: message,
            showConfirmButton: false, timer: 2500, timerProgressBar: true,
            customClass: Object.assign({}, SWAL_BASE.customClass, { popup: 'admin-swal admin-swal-toast' })
        }));
    }

    function error(message) { notify(message || L["Js_ErrorOccured"], 'error'); }

    async function confirmDelete(text) {
        const result = await swal({
            title: L["Js_AreYouSure"], text: text || L["Js_Irreversible"],
            icon: 'warning', showCancelButton: true,
            confirmButtonText: L["Common_Delete"], cancelButtonText: L["Common_Cancel"],
            customClass: Object.assign({}, SWAL_BASE.customClass, { confirmButton: 'admin-button-danger' })
        });
        return result.isConfirmed;
    }

    async function get(url) { return (await fetch(url)).json(); }

    async function post(url, data) {
        const fd = new FormData();
        Object.entries(data || {}).forEach(([k, v]) => fd.append(k, v));
        return postForm(url, fd);
    }

    async function postForm(url, formData) {
        const res = await fetch(url, { method: 'POST', body: formData, headers: { 'RequestVerificationToken': token() } });
        let body = null; try { body = await res.json(); } catch (e) { /* boş */ }
        return { ok: res.ok, status: res.status, data: body };
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

    async function putJson(url, obj) {
        const res = await fetch(url, {
            method: 'PUT',
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

    function errText(res) {
        const d = res && res.data;
        if (d) {
            if (d.errors) return Object.values(d.errors).flat().join(' • ');
            if (d.message) return d.message;
            if (d.error) return d.error;
        }
        return L["Js_ErrorOccured"];
    }

    function loader(show) {
        const el = document.getElementById('listLoader');
        if (!el) return;
        el.classList.toggle('is-active', show);
    }

    function reloadAfter(delay) { setTimeout(() => location.reload(), delay || 700); }

    function downloadCsv(filename, headers, rows) {
        const esc = v => `"${String(v ?? '').replace(/"/g, '""')}"`;
        const csv = [headers.map(esc).join(','), ...rows.map(r => r.map(esc).join(','))].join('\r\n');
        const blob = new Blob(['\ufeff' + csv], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);
    }


    // Filtr panelindəki (.admin-toolbar) hər select dəyişəndə siyahını avtomatik yeniləyir.
    // "Axtar" düyməsinə basmağa ehtiyac qalmır. data-no-auto ilə element istisna edilə bilər.
    function filterReload() {
        const reload = window.loadList || window.load;
        if (typeof reload === 'function') reload(1);
    }

    document.addEventListener('change', function (e) {
        const el = e.target;
        if (!el || !el.closest('.admin-toolbar')) return;
        if (el.hasAttribute('data-no-auto')) return;
        filterReload();
    });

    // Axtarış sahəsinə yazdıqca da avtomatik axtarır (debounce ilə).
    let _filterTimer = null;
    document.addEventListener('input', function (e) {
        const el = e.target;
        if (!el || el.tagName !== 'INPUT') return;
        if (!el.closest('.admin-toolbar')) return;
        if (el.type !== 'search' && el.id !== 'search') return;
        if (el.hasAttribute('data-no-auto')) return;
        clearTimeout(_filterTimer);
        _filterTimer = setTimeout(filterReload, 350);
    });

    Object.assign(UI, { token, SWAL_BASE, swal, notify, error, confirmDelete, get, post, postForm, putForm, postJson, putJson, del, errText, loader, reloadAfter, downloadCsv });
})(window.AdminUI);
