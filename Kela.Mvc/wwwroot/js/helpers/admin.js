// Admin panel üçün ümumi köməkçilər: toast, təsdiq, antiforgery-li fetch, cədvəl, badge.
window.AdminUI = (function () {
    function token() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    // SweetAlert panelin dizaynına salınır: brend rəngi, künclər və öz düymələrimiz
    // (buttonsStyling: false → .admin-button-* istifadə olunur).
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

    // "#" sütunu üçün sıra nömrəsi: order (sortOrder) varsa onu, yoxsa səhifə üzrə ardıcıl indeksi qaytarır.
    // # sütunu — sıralama dəyəri deyil, cədvəldəki sıra nömrəsi
    // (səhifələmə nəzərə alınmaqla), bütün cədvəllərdə eyni.
    function rowNo(item, index, page, perPage) {
        return ((page || 1) - 1) * (perPage || 20) + index + 1;
    }

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

    // Xəta cavabından göstəriləcək mətni çıxarır: əvvəl validation errors, sonra message/error.
    function errText(res) {
        const d = res && res.data;
        if (d) {
            if (d.errors) return Object.values(d.errors).flat().join(' • ');
            if (d.message) return d.message;
            if (d.error) return d.error;
        }
        return L["Js_ErrorOccured"];
    }

    async function confirmRestore() {
        const result = await swal({
            title: L["Js_ConfirmRestore"], icon: 'question', showCancelButton: true,
            confirmButtonText: L["Common_Restore"], cancelButtonText: L["Common_Cancel"]
        });
        return result.isConfirmed;
    }

    // Silinmiş elementi bərpa edir (bulk endpoint-in 'restore' əməliyyatı ilə).
    async function restoreItem(url, id, cb) {
        if (!await confirmRestore()) return;
        const res = await postJson(url, { ids: [id], action: 'restore' });
        if (res.ok) { notify(L["Js_Restored"]); if (typeof cb === 'function') cb(); }
        else error(errText(res));
    }

    // Silinənlər rejimində sətir üçün "Bərpa et" düyməsi.
    function restoreButton(id) {
        return rowActions([actionButton(L["Common_Restore"], `restoreRow(${id})`, { tone: 'primary' })]);
    }

    // Silinənlər rejimi düyməsinin görünüşünü yeniləyir.
    function syncTrashButton(btnId, on) {
        const btn = document.getElementById(btnId);
        if (btn) {
            btn.classList.toggle('admin-button-secondary', !on);
            btn.classList.toggle('admin-button-primary', on);
        }
        // Silinənlər rejimində yalnız bərpa mümkündür — aktiv/deaktiv mənasızdır.
        toggleHidden('bulkRestore', !on);
        toggleHidden('bulkActivate', on);
        toggleHidden('bulkDeactivate', on);
    }

    function toggleHidden(id, hidden) {
        const el = document.getElementById(id);
        if (el) el.classList.toggle('hidden', hidden);
    }

    function badge(text, variant, opts) {
        const dot = (opts && opts.dot) ? '<span class="badge-dot"></span>' : '';
        return `<span class="badge badge-${variant || 'neutral'}">${dot}${text}</span>`;
    }

    // JS tərəfdən göstərici kartı — Razor-daki Ui/_StatCard (Compact) ilə eyni görünüş.
    function statCard(label, value, tone) {
        return `<article class="admin-card p-4">
            <p class="text-xs font-semibold uppercase tracking-wide text-slate-400">${label}</p>
            <p class="mt-1 text-2xl font-extrabold ${tone || 'text-slate-950'}">${value}</p>
        </article>`;
    }

    // Status deyil, sadəcə etiket (məs. fənn adı).
    function chip(text, tone) {
        return `<span class="chip chip-${tone || 'neutral'}">${text}</span>`;
    }

    // Cari sıralama seçimini siyahı sorğusuna əlavə edir ("field_dir" formatı).
    function addSort(url) {
        const el = document.getElementById('sort');
        if (!el || !el.value) return url;
        const [field, dir] = el.value.split('_');
        return `${url}${url.includes('?') ? '&' : '?'}sort_by=${field}&sort_dir=${dir}`;
    }

    // Cədvəl altlığındaki "Cəmi N nəticə" mətni.
    function setInfo(total) {
        const el = document.getElementById('tableInfo');
        if (!el) return;
        el.textContent = (L["Common_ResultCount"] || '{0}').replace('{0}', total ?? 0);
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

    const TONE_CLASS = { primary: 'is-primary', danger: 'is-danger' };
    const toneOf = (opts) => TONE_CLASS[(opts && opts.tone) || ''] || '';

    // Sətir əməliyyatı — bütün cədvəllər eyni görünüşü paylaşır (bax: .admin-row-action).
    function actionButton(label, onclick, opts) {
        return `<button type="button" class="admin-row-action ${toneOf(opts)}" onclick="${onclick}">${label}</button>`;
    }

    function actionLink(label, href, opts) {
        return `<a href="${href}" class="admin-row-action ${toneOf(opts)}">${label}</a>`;
    }

    function rowActions(parts) {
        return `<div class="admin-row-actions">${parts.filter(Boolean).join('')}</div>`;
    }

    // cədvəl sətri üçün standart redaktə/silmə düymələri
    function actions(id, opts) {
        opts = opts || {};
        return rowActions([
            opts.extra,
            actionButton(L["Js_Edit"], `openEdit(${id})`, { tone: 'primary' }),
            actionButton(L["Common_Delete"], `removeItem(${id})`, { tone: 'danger' })
        ]);
    }

    // rows: hər sətir = [{ html, cls }] ; opts = { selectable, rowIds, empty }
    function table(tbodyId, rows, colspan, opts) {
        const tbody = document.getElementById(tbodyId);
        if (!tbody) return;
        opts = opts || {};
        const selectable = !!opts.selectable;
        const rowIds = opts.rowIds || [];
        // Boş nəticə sətrinin colspan-ı başlıqdan götürülür: səhifələr sayı əl ilə
        // yazmadıqda da uyğun gəlir. (colspan parametri yalnız ehtiyat variantdır.)
        const headRow = tbody.closest('table')?.tHead?.rows?.[0];
        const cols = headRow ? headRow.cells.length : colspan + (selectable ? 1 : 0);

        tbody.innerHTML = '';

        // Siyahı yenidən çəkilir — başlıqdaki seçim qutusu köhnə vəziyyətdə qalmasın.
        const head = document.getElementById('checkAll');
        if (head) head.checked = false;

        if (!rows.length) {
            tbody.innerHTML = `<tr><td colspan="${cols}">
                <div class="admin-empty">
                    <svg class="h-10 w-10 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5"><path stroke-linecap="round" stroke-linejoin="round" d="M3 7h18M3 12h18M3 17h10" /></svg>
                    <p class="text-base font-semibold text-slate-500">${L["Js_NoResults"]}</p>
                    <p class="text-sm">${opts.empty || L["Js_AdjustSearch"]}</p>
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
        if (count) count.textContent = (L["Js_SelectedCount"] || '{0}').replace('{0}', ids.length);
        const bar = document.getElementById('bulkBar');
        if (bar) {
            bar.classList.toggle('hidden', ids.length === 0);
            bar.classList.toggle('flex', ids.length > 0);
        }
    }

    async function bulkDo(url, action, cb) {
        const ids = selectedIds();
        if (!ids.length) { notify(L["Js_NothingSelected"], 'warning'); return; }
        if (action === 'delete' && !await confirmDelete((L["Js_ConfirmBulkDelete"] || '{0}').replace('{0}', ids.length))) return;
        const res = await postJson(url, { ids, action });
        if (res.ok) { notify(L["Js_Applied"]); if (typeof cb === 'function') cb(); }
        else error(res.data?.message || L["Js_ErrorOccured"]);
    }

    function sortToggle() { /* səhifə xüsusi tətbiq edə bilər */ }

    // Açılan siyahının etiketini native select-dən yenidən oxuyur.
    function syncSelect(select) {
        const wrap = document.querySelector(`.admin-select[data-for="${select.id}"]`);
        const trigger = wrap && wrap.querySelector('.admin-select-trigger');
        if (!trigger) return;
        const opts = [...select.options];
        const multiple = !!select.multiple;
        const text = multiple
            ? (opts.filter(o => o.selected).length
                ? (L["Js_SelectedCount"] || '{0}').replace('{0}', opts.filter(o => o.selected).length)
                : '')
            : ((opts[select.selectedIndex] || {}).text || '');
        trigger.textContent = text;
        trigger.classList.toggle('is-empty', !text);
    }

    // --- Modallar ---
    // Bütün modallar üçün ortaq aç/bağla. Bağlanmanı dayandırmaq lazımdırsa,
    // səhifə "modal:before-close" hadisəsini preventDefault edir (məs. yükləmə gedərkən).
    function openModal(id) {
        const modal = typeof id === 'string' ? document.getElementById(id) : id;
        if (!modal) return;
        // Sahələr JS ilə doldurulub — açılan siyahıların etiketi təzələnsin.
        modal.querySelectorAll('select[data-enhanced]').forEach(syncSelect);
        modal.dispatchEvent(new CustomEvent('modal:open'));
        modal.classList.remove('hidden');
        modal.classList.add('flex');
    }

    function closeModal(id) {
        const modal = typeof id === 'string' ? document.getElementById(id) : id;
        if (!modal || modal.classList.contains('hidden')) return;
        if (!modal.dispatchEvent(new CustomEvent('modal:before-close', { cancelable: true }))) return;

        modal.classList.add('hidden');
        modal.classList.remove('flex');

        // Bağlanan modalın forması təmizlənir — növbəti açılış köhnə məlumatla başlamasın.
        modal.querySelectorAll('form').forEach(resetForm);
        closeSelects();

        const err = modal.querySelector('[data-modal-error]');
        if (err) err.classList.add('hidden');

        modal.dispatchEvent(new CustomEvent('modal:closed'));
    }

    // --- Forma yoxlaması ---
    // Zəruri sahələr HTML `required` ilə deyil, burada yoxlanılır ki, xəbərdarlıq
    // brauzerin öz baloncuğu yerinə panelin dizaynında görünsün.
    function validateForm(formOrId) {
        const form = typeof formOrId === 'string' ? document.getElementById(formOrId) : formOrId;
        if (!form) return true;

        form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
        const empty = [...form.querySelectorAll('[data-required="true"]')]
            .filter(el => !el.disabled && !String(el.value ?? '').trim());

        if (!empty.length) return true;

        empty.forEach(el => el.classList.add('is-invalid'));
        const box = form.closest('[data-modal]')?.querySelector('[data-modal-error]')
            || form.querySelector('[data-modal-error]');
        if (box) {
            box.textContent = L["Common_RequiredFields"];
            box.classList.remove('hidden');
        }
        empty[0].focus();
        return false;
    }

    // Sahə doldurulan kimi qırmızı işarə götürülür.
    document.addEventListener('input', (e) => {
        if (e.target.classList) e.target.classList.remove('is-invalid');
    });

    // Göndərmədən əvvəl yoxla — səhifələrin öz submit handler-i işə düşməsin.
    document.addEventListener('submit', (e) => {
        if (!e.target.querySelector || !e.target.querySelector('[data-required="true"]')) return;
        if (!validateForm(e.target)) {
            e.preventDefault();
            e.stopImmediatePropagation();
        }
    }, true);

    // --- Status açarı ---
    // Görünüşü ortaqdır; əməliyyatı səhifə özü edir (adi redaktə endpoint-i ilə),
    // ona görə hər modul üçün ayrıca endpoint lazım deyil.
    function statusSwitch(checked, onclick) {
        return `<button type="button" class="admin-switch${checked ? ' is-on' : ''}" role="switch"
                        aria-checked="${checked}" title="${checked ? L["Js_Deactivate"] : L["Js_Activate"]}"
                        onclick="${onclick}"><span></span></button>`;
    }

    // --- Axtarışlı select ---
    // Native <select> gizlədilir, yerinə dizayn sisteminə uyğun açılan siyahı qurulur.
    // Seçim yenə native select-də saxlanılır, ona görə formalar və səhifə JS-i
    // dəyişmədən işləyir.
    let openSelectWrap = null;

    function closeSelects() {
        document.querySelectorAll('.admin-select.is-open').forEach(w => w.classList.remove('is-open'));
        openSelectWrap = null;
    }

    function enhanceSelect(select) {
        if (!select || select.dataset.enhanced === '1' || select.dataset.native === '1') return;
        select.dataset.enhanced = '1';

        const multiple = !!select.multiple;
        const wrap = document.createElement('div');
        wrap.className = 'admin-select';
        wrap.dataset.for = select.id;

        const trigger = document.createElement('button');
        trigger.type = 'button';
        trigger.className = 'admin-select-trigger';
        wrap.appendChild(trigger);

        const menu = document.createElement('div');
        menu.className = 'admin-select-menu hidden';
        menu.innerHTML = `
            <div class="admin-select-search">
                <input type="search" class="admin-field" placeholder="${L["Common_SearchPlaceholder"] || ''}" />
            </div>
            <div class="admin-select-options"></div>`;
        wrap.appendChild(menu);

        select.classList.add('hidden');
        select.parentNode.insertBefore(wrap, select.nextSibling);

        const search = menu.querySelector('input');
        const list = menu.querySelector('.admin-select-options');

        const label = () => {
            const opts = [...select.options];
            if (multiple) {
                const picked = opts.filter(o => o.selected);
                return picked.length
                    ? (L["Js_SelectedCount"] || '{0}').replace('{0}', picked.length)
                    : (select.dataset.placeholder || '');
            }
            const sel = opts[select.selectedIndex];
            return sel ? sel.text : '';
        };

        // "İ" hərfi: AZ/TR mətnlərində toLowerCase gözlənilən nəticəni vermir,
        // ona görə hər iki tərəf normallaşdırılır ("İngilis dili" ≈ "ingilis").
        const norm = (v) => String(v).normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().replace(/ı/g, 'i');

        function renderOptions() {
            const q = norm(search.value || '').trim();
            const opts = [...select.options].filter(o => !q || norm(o.text).includes(q));
            if (!opts.length) {
                list.innerHTML = `<div class="admin-select-empty">${L["Js_NoResults"]}</div>`;
                return;
            }
            list.innerHTML = opts.map(o => `
                <button type="button" class="admin-select-option${(multiple ? o.selected : o.index === select.selectedIndex) ? ' is-selected' : ''}"
                        data-value="${o.value.replace(/"/g, '&quot;')}">
                    ${multiple ? '<span class="admin-select-box"></span>' : ''}
                    <span class="truncate">${o.text}</span>
                </button>`).join('');
        }

        function sync() {
            trigger.textContent = label();
            trigger.classList.toggle('is-empty', !label());
        }

        // Modal gövdəsi scroll olunur və açılan siyahını kəsir — ona görə siyahı
        // düymənin koordinatlarına görə fixed yerləşdirilir.
        function place() {
            const r = trigger.getBoundingClientRect();
            const below = window.innerHeight - r.bottom;
            const height = Math.min(280, menu.scrollHeight || 280);
            menu.style.position = 'fixed';
            menu.style.width = r.width + 'px';
            menu.style.left = r.left + 'px';
            menu.style.top = (below < height + 16 && r.top > height
                ? r.top - height - 6
                : r.bottom + 6) + 'px';
        }

        function open() {
            closeSelects();
            search.value = '';
            renderOptions();
            menu.classList.remove('hidden');
            wrap.classList.add('is-open');
            openSelectWrap = wrap;
            place();
            search.focus();
        }

        trigger.addEventListener('click', () => {
            if (select.disabled) return;
            wrap.classList.contains('is-open') ? closeSelects() : open();
        });

        search.addEventListener('input', renderOptions);

        // ESC yalnız siyahını bağlayır — modal bağlanmasın.
        wrap.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') { e.stopPropagation(); closeSelects(); trigger.focus(); }
        });
        search.addEventListener('keydown', (e) => { if (e.key === 'Enter') e.preventDefault(); });

        list.addEventListener('click', (e) => {
            const btn = e.target.closest('.admin-select-option');
            if (!btn) return;
            const option = [...select.options].find(o => o.value === btn.dataset.value);
            if (!option) return;

            if (multiple) {
                option.selected = !option.selected;
                btn.classList.toggle('is-selected', option.selected);
            } else {
                select.value = option.value;
                closeSelects();
            }
            sync();
            select.dispatchEvent(new Event('change', { bubbles: true }));
        });

        select.addEventListener('change', sync);
        document.addEventListener('click', (e) => {
            if (!wrap.contains(e.target)) wrap.classList.remove('is-open');
        });

        sync();
    }

    document.addEventListener('DOMContentLoaded', () => enhanceSelects());

    window.addEventListener('scroll', () => {
        const wrap = document.querySelector('.admin-select.is-open');
        if (!wrap) return;
        const trigger = wrap.querySelector('.admin-select-trigger');
        const menu = wrap.querySelector('.admin-select-menu');
        const r = trigger.getBoundingClientRect();
        menu.style.left = r.left + 'px';
        menu.style.top = (r.bottom + 6) + 'px';
    }, true);

    function enhanceSelects(root) {
        (root || document).querySelectorAll('select:not([data-enhanced]):not([data-native])').forEach(select => {
            if (select.closest('.admin-select')) return;
            enhanceSelect(select);
        });
    }

    // Formanı ilkin vəziyyətinə qaytarır: sahələr + fayl önizləmələri.
    function resetForm(formOrId) {
        const form = typeof formOrId === 'string' ? document.getElementById(formOrId) : formOrId;
        if (!form || !form.reset) return;
        form.reset();
        // Gizli sahələri JS doldurur və onların default dəyəri yoxdur —
        // reset() onları boşaltmır, ona görə əl ilə sıfırlanır.
        form.querySelectorAll('input[type=hidden]').forEach(input => { input.value = ''; });
        form.querySelectorAll('input[type=file]').forEach(input => { if (input.id) fileReset(input.id); });
    }

    function openModals() {
        return [...document.querySelectorAll('[data-modal]')].filter(m => !m.classList.contains('hidden'));
    }

    document.addEventListener('keydown', (e) => {
        if (e.key !== 'Escape') return;
        const open = openModals();
        if (open.length) closeModal(open[open.length - 1]);
    });

    // Fon sahəsinə (paneldən kənara) klik modalı bağlayır.
    document.addEventListener('click', (e) => {
        const backdrop = e.target.closest ? e.target.closest('[data-modal]') : null;
        if (backdrop && e.target === backdrop) closeModal(backdrop);
    });

    // --- Ümumi sıralama modalı ---
    let _sortItems = [];
    let _sortSaveUrl = null;
    let _sortOnSaved = null;

    function openSort(opts) {
        _sortItems = (opts.items || []).map(x => ({ ...x }));
        _sortSaveUrl = opts.saveUrl;
        _sortOnSaved = opts.onSaved;
        const title = document.getElementById('sortTitle');
        if (title) title.textContent = opts.title || L["Sort_Title"];
        renderSort();
        openModal('sortModal');
    }

    function closeSort() {
        closeModal('sortModal');
    }

    function renderSort() {
        const box = document.getElementById('sortList');
        if (!box) return;
        box.innerHTML = '';
        if (!_sortItems.length) {
            box.innerHTML = '<div class="text-sm text-slate-400">' + L["Js_NothingToSort"] + '</div>';
            return;
        }
        _sortItems.forEach((item, idx) => {
            const row = document.createElement('div');
            row.className = 'sort-row flex items-center gap-3 rounded-lg border border-border bg-white px-3 py-2.5 cursor-grab active:cursor-grabbing transition';
            row.draggable = true;
            row.dataset.index = idx;

            const handle = document.createElement('span');
            handle.className = 'shrink-0 text-slate-300';
            handle.innerHTML = '<svg class="h-5 w-5" fill="currentColor" viewBox="0 0 20 20"><circle cx="7" cy="5" r="1.5"/><circle cx="13" cy="5" r="1.5"/><circle cx="7" cy="10" r="1.5"/><circle cx="13" cy="10" r="1.5"/><circle cx="7" cy="15" r="1.5"/><circle cx="13" cy="15" r="1.5"/></svg>';

            const label = document.createElement('span');
            label.className = 'flex-1 truncate text-sm text-slate-700';
            label.textContent = item.label || ('#' + item.id);

            row.append(handle, label);
            box.appendChild(row);
        });
        wireSortDnD();
    }

    let _dragIndex = null;

    function wireSortDnD() {
        const box = document.getElementById('sortList');
        if (!box) return;
        box.querySelectorAll('.sort-row').forEach(row => {
            row.addEventListener('dragstart', e => {
                _dragIndex = Number(row.dataset.index);
                row.classList.add('opacity-50');
                e.dataTransfer.effectAllowed = 'move';
                try { e.dataTransfer.setData('text/plain', String(_dragIndex)); } catch (err) { /* boş */ }
            });
            row.addEventListener('dragend', () => {
                row.classList.remove('opacity-50');
                box.querySelectorAll('.sort-row').forEach(r => r.classList.remove('border-primary', 'bg-primary-light'));
            });
            row.addEventListener('dragover', e => {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';
                box.querySelectorAll('.sort-row').forEach(r => r.classList.remove('border-primary', 'bg-primary-light'));
                if (Number(row.dataset.index) !== _dragIndex) row.classList.add('border-primary', 'bg-primary-light');
            });
            row.addEventListener('drop', e => {
                e.preventDefault();
                const from = _dragIndex;
                const to = Number(row.dataset.index);
                _dragIndex = null;
                if (from === null || from === to) { renderSort(); return; }
                const [moved] = _sortItems.splice(from, 1);
                _sortItems.splice(to, 0, moved);
                renderSort();
                persistSort();
            });
        });
    }

    // Drag&amp;drop bitdikdə sıranı avtomatik yadda saxlayır (Save düyməsi yoxdur).
    async function persistSort() {
        if (!_sortSaveUrl) return;
        const ids = _sortItems.map(x => x.id);
        const res = await postJson(_sortSaveUrl, { ids });
        if (res.ok) {
            notify(L["Js_OrderUpdated"]);
            if (typeof _sortOnSaved === 'function') _sortOnSaved();
        } else {
            error(errText(res));
        }
    }

    function isVideoUrl(url) { return /\.(mp4|mov|m4v|webm|m3u8|mkv|avi)(\?|$)/i.test(url || ''); }

    function setPreview(id, url, video) {
        const img = document.getElementById(id + '_preview');
        const vid = document.getElementById(id + '_preview_video');
        if (!img && !vid) return;

        if (video && vid) {
            vid.src = url;
            vid.classList.remove('hidden');
            if (img) { img.classList.add('hidden'); img.removeAttribute('src'); }
        } else if (img) {
            img.src = url;
            img.classList.remove('hidden');
            if (vid) { vid.classList.add('hidden'); vid.removeAttribute('src'); }
        }

        document.getElementById(id + '_x').classList.remove('hidden');
        toggleHidden(id + '_upload', true);   // fayl varsa yükləmə sahəsi görünməsin
        const nameEl = document.getElementById(id + 'Name');
        if (nameEl) nameEl.textContent = '';
        document.getElementById(id + '_remove').value = 'false';
    }

    // Formu sıfırlayır: önizləmə elementlərini də təmizləyir. fileRemove-dan fərqi —
    // _remove bayrağını 'false' edir, çünki burada silinəcək server faylı yoxdur.
    function fileReset(id) {
        const img = document.getElementById(id + '_preview');
        const vid = document.getElementById(id + '_preview_video');

        if (img) { img.classList.add('hidden'); img.removeAttribute('src'); }
        if (vid) {
            try { vid.pause(); } catch (e) { /* əhəmiyyətsiz */ }
            vid.classList.add('hidden');
            vid.removeAttribute('src');
        }

        const x = document.getElementById(id + '_x');
        if (x) x.classList.add('hidden');

        toggleHidden(id + '_upload', false);   // fayl götürüldü — yükləmə sahəsi geri gəlir

        const rm = document.getElementById(id + '_remove');
        if (rm) rm.value = 'false';

        const input = document.getElementById(id);
        if (input) input.value = '';

        const nameEl = document.getElementById(id + 'Name');
        if (nameEl) nameEl.textContent = nameEl.dataset.hint || '';
    }

    function filePreview(id) {
        const input = document.getElementById(id);
        if (!input || !input.files.length) return;

        const file = input.files[0];
        setPreview(id, URL.createObjectURL(file), (file.type || '').startsWith('video/'));
    }

    function fileSet(id, url) { if (url) setPreview(id, url, isVideoUrl(url)); }

    function fileRemove(id) {
        const img = document.getElementById(id + '_preview');
        const vid = document.getElementById(id + '_preview_video');
        if (img) { img.classList.add('hidden'); img.removeAttribute('src'); }
        if (vid) { vid.classList.add('hidden'); vid.removeAttribute('src'); }
        document.getElementById(id + '_x').classList.add('hidden');
        toggleHidden(id + '_upload', false);
        document.getElementById(id + '_remove').value = 'true';
        const input = document.getElementById(id);
        if (input) input.value = '';
    }


    // CSV faylı yükləyir (BOM ilə, Excel uyğun).
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
        if (!el || el.tagName !== 'SELECT') return;
        if (!el.closest('.admin-toolbar')) return;
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

    return { notify, error, errText, openModal, closeModal, swal, resetForm, validateForm, enhanceSelects, syncSelect, badge, chip, statCard, statusSwitch, actions, actionButton, actionLink, rowActions, confirmDelete, confirmRestore, restoreItem, restoreButton, syncTrashButton, toggleHidden, get, post, postForm, putForm, postJson, putJson, del, table, setInfo, addSort, selectedIds, bulkDo, sortToggle, openSort, closeSort, persistSort, loader, pagination, filePreview, fileSet, fileRemove, fileReset, rowNo, token, reloadAfter, downloadCsv };
})();
