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

    // --- Göndərmə düyməsinin kilidi ---
    // Form göndəriləndə düymə deaktiv olur və üzərində spinner görünür;
    // cavab gələndə (və ya 15 san sonra) yenidən aktivləşir.
    let _pendingSubmitBtn = null;
    let _pendingTimer = null;

    function lockSubmitButton(btn) {
        if (!btn) return;
        _pendingSubmitBtn = btn;
        btn.disabled = true;
        btn.classList.add('is-loading');
        clearTimeout(_pendingTimer);
        _pendingTimer = setTimeout(releaseSubmitButton, 15000);
    }

    function releaseSubmitButton() {
        clearTimeout(_pendingTimer);
        const btn = _pendingSubmitBtn;
        _pendingSubmitBtn = null;
        if (!btn) return;
        btn.disabled = false;
        btn.classList.remove('is-loading');
    }

    async function get(url) { return (await fetch(url)).json(); }

    async function post(url, data) {
        const fd = new FormData();
        Object.entries(data || {}).forEach(([k, v]) => {
            // Massiv → təkrar sahələr (məs. lesson_ids) kimi göndərilir.
            if (Array.isArray(v)) v.forEach(item => fd.append(k, item));
            else fd.append(k, v);
        });
        return postForm(url, fd);
    }

    // --- Plan limiti ---
    // Limit dolanda server 403 + code:"limit_exceeded" qaytarır. İstifadəçiyə
    // uyğun əlavə paketi təklif edirik; alınıbsa sorğu bir dəfə təkrar göndərilir.
    async function limitInfo() {
        try {
            const data = await get('/subscription/data');
            return {
                offers: data?.usage?.addonOffers || [],
                currency: data?.plan?.currency || ''
            };
        } catch (e) {
            return { offers: [], currency: '' };
        }
    }

    async function promptLimit(info) {
        const { offers, currency } = await limitInfo();
        const offer = offers.find(o => o.kind === info.kind) || null;
        const canBuy = !!offer && !!(window.ADMIN && window.ADMIN.isAdmin);

        const lines = [`<p class="text-sm text-slate-600">${escapeHtml(info.message || L["Js_ErrorOccured"])}</p>`];

        if (offer) {
            lines.push(`<div class="mt-4 rounded-xl bg-surface-secondary p-3 text-left">
                <p class="text-sm font-semibold text-slate-800">${escapeHtml(offer.label)}</p>
                <p class="mt-1 text-sm text-slate-500">+${offer.packSize} ${escapeHtml(offer.unit)} — ${offer.packPrice} ${escapeHtml(currency)}</p>
            </div>`);
        }

        if (!canBuy) {
            lines.push(`<p class="mt-3 text-sm text-slate-500">${offer ? L["Limit_ContactAdmin"] : L["Limit_NoOffer"]}</p>`);
        }

        const options = {
            title: L["Limit_Title"],
            html: lines.join(''),
            icon: 'warning',
            confirmButtonText: canBuy ? L["Limit_Buy"] : L["Common_Close"],
            showCancelButton: canBuy,
            cancelButtonText: L["Common_Cancel"]
        };

        if (canBuy) {
            options.input = 'number';
            options.inputValue = 1;
            options.inputAttributes = { min: 1, max: offer.maxQuantity };
        }

        const result = await swal(options);
        if (!canBuy || !result.isConfirmed) return false;

        const packs = Math.max(1, Math.min(offer.maxQuantity, parseInt(result.value, 10) || 1));
        const added = await post('/subscription/addons', { kind: offer.kind, packs });
        if (!added.ok) { error(errText(added)); return false; }

        notify(L["Limit_Confirmed"]);
        return true;
    }

    // Bütün fetch sorğuları buradan keçir: limit xətası olsa modal göstərilir və
    // paket alındıqdan sonra eyni sorğu təkrarən (bir dəfə) göndərilir.
    async function request(method, url, body, retryLimit) {
        const init = { method, headers: { 'RequestVerificationToken': token() } };
        if (body !== undefined) init.body = body;

        const res = await fetch(url, init);
        releaseSubmitButton();
        let data = null; try { data = await res.json(); } catch (e) { /* boş */ }
        const result = { ok: res.ok, status: res.status, data };

        if (!result.ok && data?.code === 'limit_exceeded' && retryLimit !== false) {
            if (await promptLimit(data)) return request(method, url, body, false);
        }

        return result;
    }

    async function postForm(url, formData) {
        return request('POST', url, formData);
    }

    async function putForm(url, formData) {
        return request('PUT', url, formData);
    }

    async function copyText(text) {
        try {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                await navigator.clipboard.writeText(text);
                return true;
            }
            const ta = document.createElement('textarea');
            ta.value = text;
            document.body.appendChild(ta);
            ta.select();
            document.execCommand('copy');
            ta.remove();
            return true;
        } catch (e) {
            return false;
        }
    }

    // HTML-ə yazılan istifadəçi mətnini təhlükəsiz etmək üçün vahid escape.
    function escapeHtml(value) {
        return String(value ?? '').replace(/[&<>"']/g, c => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
        }[c]));
    }

    // data-action / data-change-action: HTML-də inline JS yerinə adlandırılmış funksiya.
    function runAction(el, extraArgs) {
        const name = el.dataset.action || el.dataset.changeAction;
        if (!name) return false;
        const fn = name.split('.').reduce((obj, key) => (obj == null ? undefined : obj[key]), window);
        if (typeof fn !== 'function') return false;
        const args = el.dataset.args ? JSON.parse(el.dataset.args) : [];
        fn(...args, ...(extraArgs || []));
        return true;
    }

    document.addEventListener('click', (e) => {
        const el = e.target.closest?.('[data-action]');
        if (el) runAction(el);
    });

    document.addEventListener('change', (e) => {
        const el = e.target.closest?.('[data-change-action]');
        if (el) runAction(el, [el]);
    });

    document.addEventListener('input', (e) => {
        const el = e.target.closest?.('[data-input-action]');
        if (!el) return;
        const fn = el.dataset.inputAction.split('.').reduce((obj, key) => (obj == null ? undefined : obj[key]), window);
        if (typeof fn === 'function') fn(el);
    });

    document.addEventListener('click', (e) => {
        const el = e.target.closest?.('[data-select-all]');
        if (el) el.select?.();
    });

    // data-img-error: şəkil yüklənmədikdə adlandırılmış fallback funksiyası.
    document.addEventListener('error', (e) => {
        if (e.target?.dataset && 'imgRemove' in e.target.dataset) {
            e.target.remove();
            return;
        }

        const name = e.target?.dataset?.imgError;
        if (!name) return;
        const fn = name.split('.').reduce((obj, key) => (obj == null ? undefined : obj[key]), window);
        if (typeof fn === 'function') fn(e.target);
    }, true);

    async function postJson(url, obj) {
        return postFormJson('POST', url, obj, true);
    }

    async function putJson(url, obj) {
        return postFormJson('PUT', url, obj, true);
    }

    async function postFormJson(method, url, obj, retryLimit) {
        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token() },
            body: JSON.stringify(obj)
        });
        releaseSubmitButton();
        let body = null; try { body = await res.json(); } catch (e) { /* boş */ }
        const result = { ok: res.ok, status: res.status, data: body };

        if (!result.ok && body?.code === 'limit_exceeded' && retryLimit) {
            if (await promptLimit(body)) return postFormJson(method, url, obj, false);
        }

        return result;
    }

    // Aktiv/deaktiv dəyişməsi — bütün resurslarda eyni endpoint forması.
    async function setActive(url, id, isActive, done) {
        const res = await putJson(`${url}/${id}/status`, { isActive });
        if (res.ok) {
            notify(L["Js_StatusChanged"]);
            if (typeof done === 'function') done();
        } else {
            error(errText(res));
        }
    }

    async function del(url) {
        return request('DELETE', url);
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

    Object.assign(UI, { token, copyText, escapeHtml, setActive, SWAL_BASE, swal, lockSubmitButton, releaseSubmitButton, notify, error, confirmDelete, get, post, postForm, putForm, postJson, putJson, del, errText, loader, reloadAfter });
})(window.AdminUI);
