// Edutor admin panel — table
// Parçalanmışdır: hər fayl eyni AdminUI obyektini genişləndirir.
window.AdminUI = window.AdminUI || {};

(function (UI) {
    // Ümumi sıralama modalının vəziyyəti
    let _sortItems = [];
    let _sortSaveUrl = null;
    let _sortOnSaved = null;
    let _dragIndex = null;

    function rowNo(item, index, page, perPage) {
        return ((page || 1) - 1) * (perPage || 20) + index + 1;
    }

    // Mobil görünüşdə cədvəl kartlara çevrilir; hər hüceyrənin başlığı
    // ::before ilə göstərilir. Etiket mənbəyi thead-dır — səhifələr heç nə etmir.
    function decorateTable(table) {
        const headRow = table?.tHead?.rows?.[0];
        if (!headRow) return;
        const labels = [...headRow.cells].map(th => (th.textContent || '').trim());
        table.querySelectorAll('tbody tr').forEach(tr => {
            const cells = [...tr.cells];
            if (cells.length === 1 && cells[0].hasAttribute('colspan')) return;
            cells.forEach((td, i) => {
                const label = labels[i] || '';
                if (label) td.dataset.label = label;
                else td.removeAttribute('data-label');
            });
        });
    }

    function decorateTables(root) {
        (root || document).querySelectorAll('table.admin-table').forEach(decorateTable);
    }

    // Siyahılar JS ilə yenidən çəkilir — yeni sətirlər də etiket alsın.
    let decorateQueued = false;
    function queueDecorate() {
        if (decorateQueued) return;
        decorateQueued = true;
        requestAnimationFrame(() => {
            decorateQueued = false;
            decorateTables();
        });
    }

    if (typeof MutationObserver === 'function') {
        new MutationObserver(queueDecorate).observe(document.documentElement, { childList: true, subtree: true });
    }
    document.addEventListener('DOMContentLoaded', queueDecorate);

    function table(tbodyId, rows, colspan, opts) {
        const tbody = document.getElementById(tbodyId);
        if (!tbody) return;
        opts = opts || {};
        const selectable = !!opts.selectable;
        const rowIds = opts.rowIds || [];
        // Verilərsə yalnız bu id-lərin sətri seçilə bilər (məs. yalnız öz bannerləri).
        const selectableIds = opts.selectableIds ? new Set(opts.selectableIds.map(String)) : null;
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
                const editable = !selectableIds || selectableIds.has(String(rowIds[idx]));
                td.innerHTML = editable
                    ? `<input type="checkbox" class="row-check h-5 w-5 rounded-md border-slate-300 text-primary focus:ring-primary" value="${rowIds[idx] ?? ''}" />`
                    : `<input type="checkbox" class="h-5 w-5 cursor-not-allowed rounded-md border-slate-200 opacity-40" disabled title="" />`;
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
        decorateTable(tbody.closest('table'));
        updateBulkBar();
    }

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

    /**
     * Səhifələmə vəziyyəti: cari səhifə, son səhifə və səhifə ölçüsünü saxlayır,
     * `apply(data)` ilə API cavabından yeniləyir və pager düymələrini çəkir.
     * Hər siyahı səhifəsində təkrarlanan kodu əvəz edir.
     */
    function createPager(containerId, load) {
        const pager = {
            page: 1,
            lastPage: 1,
            perPage: 20,
            total: 0,
            apply(data) {
                pager.page = Number(data.currentPage ?? data.page ?? 1) || 1;
                pager.lastPage = Number(data.lastPage ?? 1) || 1;
                pager.perPage = Number(data.perPage ?? 20) || 20;
                pager.total = Number(data.total ?? 0) || 0;
                setInfo(pager.total);
                pagination(containerId, pager.page, pager.lastPage, p => load(p));
                return pager;
            }
        };
        return pager;
    }

    function wireSelection() {
        document.querySelectorAll('#rows .row-check').forEach(box => {
            box.addEventListener('change', () => { syncCheckAll(); updateBulkBar(); });
        });
        const all = document.getElementById('checkAll');
        if (all && !all.dataset.bound) {
            all.dataset.bound = '1';
            all.addEventListener('change', () => {
                document.querySelectorAll('#rows .row-check:not(:disabled)').forEach(box => { box.checked = all.checked; });
                updateBulkBar();
            });
        }
    }

    function syncCheckAll() {
        const all = document.getElementById('checkAll');
        if (!all) return;
        const boxes = [...document.querySelectorAll('#rows .row-check:not(:disabled)')];
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

    document.addEventListener('click', (e) => {
        if (e.target.closest?.('[data-sort-close]')) UI.closeSort();
    });

    document.addEventListener('click', (e) => {
        const btn = e.target.closest?.('[data-bulk-action]');
        if (!btn) return;
        const bar = btn.closest('[data-bulk-url]');
        if (bar) UI.bulkDo(bar.dataset.bulkUrl, btn.dataset.bulkAction);
    });

    async function bulkDo(url, action, cb) {
        const ids = selectedIds();
        if (!ids.length) { UI.notify(L["Js_NothingSelected"], 'warning'); return; }
        if (action === 'delete' && !await UI.confirmDelete((L["Js_ConfirmBulkDelete"] || '{0}').replace('{0}', ids.length))) return;
        const res = await UI.postJson(url, { ids, action });
        if (res.ok) {
            UI.notify(L["Js_Applied"]);
            if (typeof cb === 'function') cb();
            else if (typeof loadList === 'function' && typeof pager === 'object') loadList(pager.page);
        }
        else UI.error(res.data?.message || L["Js_ErrorOccured"]);
    }

    function sortToggle() { /* səhifə xüsusi tətbiq edə bilər */ }

    function openSort(opts) {
        _sortItems = (opts.items || []).map(x => ({ ...x }));
        _sortSaveUrl = opts.saveUrl;
        _sortOnSaved = opts.onSaved;
        const title = document.getElementById('sortTitle');
        if (title) title.textContent = opts.title || L["Sort_Title"];
        renderSort();
        UI.openModal('sortModal');
    }

    function closeSort() {
        UI.closeModal('sortModal');
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

    async function persistSort() {
        if (!_sortSaveUrl) return;
        const ids = _sortItems.map(x => x.id);
        const res = await UI.postJson(_sortSaveUrl, { ids });
        if (res.ok) {
            UI.notify(L["Js_OrderUpdated"]);
            if (typeof _sortOnSaved === 'function') _sortOnSaved();
        } else {
            UI.error(UI.errText(res));
        }
    }

    function addSort(url) {
        const el = document.getElementById('sort');
        if (!el || !el.value) return url;
        const [field, dir] = el.value.split('_');
        return `${url}${url.includes('?') ? '&' : '?'}sort_by=${field}&sort_dir=${dir}`;
    }

    // Siyahı səhifəsindəki bütün qeydləri sıralama modalına yükləyir.
    async function openListSort(options) {
        const value = options.filterId ? document.getElementById(options.filterId)?.value : '';
        if (options.requireFilter && !value) {
            UI.notify(L[options.requireFilter], 'warning');
            return;
        }

        const params = new URLSearchParams({ page: '1', perPage: '500' });
        if (options.filterId && value) params.set(options.filterParam, value);

        const data = await UI.get(`${options.api}?${params.toString()}`);
        const title = typeof options.title === 'function' ? options.title(value) : options.title;
        UI.openSort({
            title,
            items: (data.data || []).map(item => ({ id: item.id, label: options.label(item) })),
            saveUrl: options.saveUrl,
            onSaved: options.onSaved || (() => loadList(pager.page))
        });
    }

    // Siyahı səhifələrinin ortaq skeleti: pager, silinmişlər, silmə/bərpa və cədvəl yenilənməsi.
    function listPage(options) {
        const state = { showDeleted: false };
        let pager;

        function query(page) {
            const search = document.getElementById('search')?.value ?? '';
            const params = options.params
                ? options.params({ page, search, showDeleted: state.showDeleted })
                : { page, search, is_deleted: state.showDeleted ? 1 : 0 };
            const qs = new URLSearchParams();
            Object.entries(params).forEach(([key, value]) => {
                if (value !== undefined && value !== null) qs.set(key, value);
            });
            return UI.addSort(`${options.api}?${qs.toString()}`);
        }

        async function loadList(page) {
            const pageNo = page || pager.page;
            UI.loader(true);
            const data = await UI.get(query(pageNo));
            pager.apply(data);
            if (options.onLoaded) options.onLoaded(data);

            const ctx = { page: pager.page, perPage: pager.perPage, showDeleted: state.showDeleted };
            const rows = data.data.map((item, index) => options.row(item, index, ctx));
            const rowIds = options.selectableIds
                ? data.data.filter(options.selectableIds).map(x => x.id)
                : data.data.map(x => x.id);
            UI.table(options.table || 'rows', rows, options.columns ?? (rows[0]?.length || 1), {
                rowIds: options.selectable ? rowIds : undefined,
                selectable: options.selectable === true && !state.showDeleted
            });
            UI.loader(false);
        }

        function toggleTrash() {
            state.showDeleted = !state.showDeleted;
            UI.syncTrashButton('trashBtn', state.showDeleted);
            loadList(1);
        }

        async function restoreRow(id) {
            if (options.restore) return options.restore(id, () => loadList(pager.page));
            await UI.restoreItem(options.bulkUrl, id, () => loadList(pager.page));
        }

        async function removeItem(id) {
            if (!await UI.confirmDelete(L[options.confirmKey])) return;
            const res = await UI.del(`${options.deleteBase}/${id}`);
            if (res.ok) { UI.notify(L["Js_Deleted"]); loadList(pager.page); }
            else UI.error();
        }

        pager = UI.createPager(options.pagerId || 'pagination', loadList);
        Object.assign(window, { loadList, toggleTrash, restoreRow, removeItem, pager });

        document.addEventListener('DOMContentLoaded', () => {
            if (options.onReady) options.onReady();
            loadList(1);
        });

        return { loadList, pager, state };
    }

    Object.assign(UI, { rowNo, table, setInfo, pagination, createPager, listPage, openListSort, wireSelection, syncCheckAll, selectedIds, updateBulkBar, bulkDo, sortToggle, openSort, closeSort, renderSort, wireSortDnD, persistSort, addSort, decorateTable, decorateTables });
})(window.AdminUI);
