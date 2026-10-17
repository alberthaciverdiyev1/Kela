// Kela admin panel — table
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
        if (!ids.length) { UI.notify(L["Js_NothingSelected"], 'warning'); return; }
        if (action === 'delete' && !await UI.confirmDelete((L["Js_ConfirmBulkDelete"] || '{0}').replace('{0}', ids.length))) return;
        const res = await UI.postJson(url, { ids, action });
        if (res.ok) { UI.notify(L["Js_Applied"]); if (typeof cb === 'function') cb(); }
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

    Object.assign(UI, { rowNo, table, setInfo, pagination, wireSelection, syncCheckAll, selectedIds, updateBulkBar, bulkDo, sortToggle, openSort, closeSort, renderSort, wireSortDnD, persistSort, addSort });
})(window.AdminUI);
