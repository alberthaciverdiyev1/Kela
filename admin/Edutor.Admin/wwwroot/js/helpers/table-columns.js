// Edutor admin panel — table columns
// Hər siyahı cədvəlində sütunları gizlət/göstər və sırasını dəyiş.
// Seçim brauzerdə (localStorage) səhifə + cədvəl üzrə saxlanılır.
window.AdminUI = window.AdminUI || {};

(function (UI) {
    const STORAGE_PREFIX = 'edutor.table.columns:';
    const SKIP_SELECTOR = 'input[type="checkbox"], .row-check';

    function tableKey(table) {
        const body = table.tBodies[0];
        if (body?.id) return STORAGE_PREFIX + location.pathname + ':' + body.id;
        const all = [...document.querySelectorAll('table.admin-table')];
        return STORAGE_PREFIX + location.pathname + ':table' + all.indexOf(table);
    }

    function read(table) {
        try {
            const value = JSON.parse(localStorage.getItem(tableKey(table)));
            return value && Array.isArray(value.order) ? value : null;
        } catch (e) {
            return null;
        }
    }

    function write(table, value) {
        try { localStorage.setItem(tableKey(table), JSON.stringify(value)); } catch (e) { /* gizli rejim */ }
    }

    function columns(table) {
        const head = table.tHead?.rows?.[0];
        if (!head) return [];
        return [...head.cells].map((th, i) => ({
            index: th.dataset.col ? Number(th.dataset.col) : i,
            th,
            label: (th.textContent || '').trim(),
            // Seçim sütunu (checkbox) həmişə birinci və görünən qalır.
            fixed: !(th.textContent || '').trim() || !!th.querySelector(SKIP_SELECTOR)
        }));
    }

    function visibleColumns(table) {
        return columns(table).filter(c => !c.fixed);
    }

    function prefsOf(table) {
        const cols = columns(table);
        const saved = read(table);
        const known = new Set(cols.map(c => c.index));
        const order = (saved?.order || []).filter(i => known.has(i));
        // Standart sıra həmişə orijinal sütun indeksinə görədir — DOM artıq
        // dəyişdirilmiş olsa belə "sıfırla" düzgün bərpa edir.
        cols.map(c => c.index).sort((a, b) => a - b)
            .forEach(i => { if (!order.includes(i)) order.push(i); });
        const hidden = new Set((saved?.hidden || []).filter(i => known.has(i)));
        cols.forEach(c => { if (c.fixed) hidden.delete(c.index); });
        return { order, hidden };
    }

    function currentOrder(table) {
        const head = table.tHead?.rows?.[0];
        if (!head) return [];
        return [...head.cells].map(th => Number(th.dataset.col));
    }

    // Sütun sırası və görünürlüyünü DOM-a tətbiq edir. Hər render-dən sonra çağırılır.
    function apply(table) {
        const head = table.tHead?.rows?.[0];
        if (!head || !table.tBodies[0]) return;

        const cols = columns(table);
        cols.forEach(c => { if (!c.th.dataset.col) c.th.dataset.col = String(c.index); });

        const { order, hidden } = prefsOf(table);
        const byIndex = new Map(cols.map(c => [Number(c.th.dataset.col), c]));

        // Sətirlər JS ilə yenidən çəkilir — hüceyrələrə sabit sütun indeksi verilir.
        const rows = [...table.tBodies[0].rows].filter(tr => !(tr.cells.length === 1 && tr.cells[0].hasAttribute('colspan')));
        rows.forEach(tr => {
            [...tr.cells].forEach((td, i) => { if (!td.dataset.col) td.dataset.col = String(i); });
        });

        // Eyni sıra təkrar tətbiq olunmasın — MutationObserver dövrəsi yaranmasın.
        const headOrder = currentOrder(table);
        const changed = order.length !== headOrder.length || order.some((i, k) => headOrder[k] !== i);
        if (changed) {
            order.forEach(i => {
                const col = byIndex.get(i);
                if (col) head.appendChild(col.th);
            });
        }

        // Sətirlər siyahı yeniləndikcə yenidən çəkilir və orijinal sırada gəlir;
        // hər dəfə sütun sırasına uyğunlaşdırılır ki, başlıqla üst-üstə düşsün.
        rows.forEach(tr => {
            const cells = new Map([...tr.cells].map(td => [Number(td.dataset.col), td]));
            const wanted = order.filter(i => cells.has(i));
            const current = [...tr.cells].map(td => Number(td.dataset.col));
            if (current.length === wanted.length && current.every((v, k) => v === wanted[k])) return;
            wanted.forEach(i => tr.appendChild(cells.get(i)));
        });

        order.forEach(i => {
            const col = byIndex.get(i);
            if (!col) return;
            const hide = hidden.has(i);
            col.th.classList.toggle('hidden', hide);
            rows.forEach(tr => {
                const td = [...tr.cells].find(c => Number(c.dataset.col) === i);
                if (td) td.classList.toggle('hidden', hide);
            });
        });
    }

    function move(order, from, to) {
        const list = order.filter(i => i !== from);
        const at = list.indexOf(to);
        list.splice(at < 0 ? list.length : at, 0, from);
        return list;
    }

    function wrapper(table) {
        const parent = table.parentElement;
        const grand = parent?.parentElement;
        if (parent?.classList.contains('relative')) return { wrap: parent, anchor: parent.firstChild };
        if (grand?.classList.contains('relative')) return { wrap: grand, anchor: grand.firstChild };

        const wrap = document.createElement('div');
        wrap.className = 'relative';
        parent.insertBefore(wrap, table);
        wrap.appendChild(table);
        return { wrap, anchor: wrap.firstChild };
    }

    function toolbar(table) {
        const { wrap, anchor } = wrapper(table);

        let bar = wrap.querySelector(':scope > .table-col-tools');
        if (bar) return bar;

        // Düymə cədvəl başlığının sağ küncündə üç nöqtə kimi görünür — cədvəlin
        // öz başlığının hissəsi kimi oxunur, ayrıca alət paneli kimi yox.
        bar = document.createElement('div');
        bar.className = 'table-col-tools pointer-events-none absolute right-0 top-0 z-20';
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'table-col-trigger pointer-events-auto grid h-[46px] w-10 place-items-center text-slate-500 transition hover:text-slate-800';
        btn.title = L["Table_Columns"];
        btn.setAttribute('aria-label', L["Table_Columns"]);
        btn.setAttribute('aria-haspopup', 'dialog');
        btn.setAttribute('aria-expanded', 'false');
        btn.innerHTML = '<svg class="h-5 w-5" fill="currentColor" viewBox="0 0 20 20"><circle cx="10" cy="4" r="1.5"/><circle cx="10" cy="10" r="1.5"/><circle cx="10" cy="16" r="1.5"/></svg>';
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            togglePanel(table, bar, btn);
        });
        bar.appendChild(btn);
        wrap.insertBefore(bar, anchor);
        return bar;
    }

    function closePanels() {
        document.querySelectorAll('.table-col-panel').forEach(p => p.remove());
        document.querySelectorAll('.table-col-tools [aria-expanded="true"]').forEach(b => b.setAttribute('aria-expanded', 'false'));
    }

    function togglePanel(table, bar, btn) {
        const open = bar.querySelector(':scope > .table-col-panel');
        closePanels();
        if (open) return;
        btn.setAttribute('aria-expanded', 'true');

        const panel = document.createElement('div');
        panel.className = 'table-col-panel pointer-events-auto absolute right-2 top-[46px] z-30 w-64 rounded-xl border border-border bg-white p-3 shadow-lg';

        const hint = document.createElement('p');
        hint.className = 'mb-2 text-xs text-slate-500';
        hint.textContent = L["Table_ColumnsHint"];
        panel.appendChild(hint);

        const list = document.createElement('div');
        list.className = 'flex max-h-64 flex-col gap-1 overflow-y-auto';
        panel.appendChild(list);

        const reset = document.createElement('button');
        reset.type = 'button';
        reset.className = 'mt-2 w-full rounded-lg px-2 py-1.5 text-xs font-semibold text-slate-500 hover:bg-surface-secondary';
        reset.textContent = L["Table_Reset"];
        reset.addEventListener('click', () => {
            write(table, { order: [], hidden: [] });
            apply(table);
            renderPanelList(table, list);
        });
        panel.appendChild(reset);

        renderPanelList(table, list);
        bar.appendChild(panel);
    }

    function renderPanelList(table, list) {
        const { order, hidden } = prefsOf(table);
        const cols = columns(table).filter(c => !c.fixed);
        const byIndex = new Map(cols.map(c => [c.index, c]));
        const ordered = order.filter(i => byIndex.has(i));
        list.innerHTML = '';

        ordered.forEach((index, position) => {
            const col = byIndex.get(index);
            const row = document.createElement('div');
            row.className = 'flex items-center gap-2 rounded-lg border border-transparent px-2 py-1.5 hover:border-border hover:bg-surface-secondary';
            row.draggable = true;

            const handle = document.createElement('span');
            handle.className = 'shrink-0 cursor-grab text-slate-300 active:cursor-grabbing';
            handle.innerHTML = '<svg class="h-4 w-4" fill="currentColor" viewBox="0 0 20 20"><circle cx="7" cy="5" r="1.4"/><circle cx="13" cy="5" r="1.4"/><circle cx="7" cy="10" r="1.4"/><circle cx="13" cy="10" r="1.4"/><circle cx="7" cy="15" r="1.4"/><circle cx="13" cy="15" r="1.4"/></svg>';

            const label = document.createElement('label');
            label.className = 'flex flex-1 cursor-pointer items-center gap-2 text-sm text-slate-700';
            const box = document.createElement('input');
            box.type = 'checkbox';
            box.className = 'h-4 w-4 rounded border-slate-300 text-primary focus:ring-primary';
            box.checked = !hidden.has(index);
            box.addEventListener('change', () => {
                const prefs = prefsOf(table);
                const set = new Set(prefs.hidden);
                box.checked ? set.delete(index) : set.add(index);
                write(table, { order: prefs.order, hidden: [...set] });
                apply(table);
            });
            const text = document.createElement('span');
            text.className = 'truncate';
            text.textContent = col.label || ('#' + (position + 1));
            label.append(box, text);

            row.addEventListener('dragstart', e => {
                row.dataset.dragging = '1';
                e.dataTransfer.effectAllowed = 'move';
                try { e.dataTransfer.setData('text/plain', String(index)); } catch (err) { /* boş */ }
            });
            row.addEventListener('dragover', e => {
                e.preventDefault();
                row.classList.add('border-primary', 'bg-primary-light');
            });
            row.addEventListener('dragleave', () => row.classList.remove('border-primary', 'bg-primary-light'));
            row.addEventListener('drop', e => {
                e.preventDefault();
                row.classList.remove('border-primary', 'bg-primary-light');
                const from = Number(e.dataTransfer.getData('text/plain'));
                if (from === index) return;
                const prefs = prefsOf(table);
                write(table, { order: move(prefs.order, from, index), hidden: [...prefs.hidden] });
                apply(table);
                renderPanelList(table, list);
            });

            row.append(handle, label);

            // Sürüşdürmə toxunma ekranlarda işləmir — oxlarla da sıralamaq mümkündür.
            const arrows = document.createElement('div');
            arrows.className = 'flex shrink-0 items-center';
            const chevron = up =>
                `<svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="${up ? 'm6 15 6-6 6 6' : 'm6 9 6 6 6-6'}"/></svg>`;

            const moveBy = (delta) => {
                const prefs = prefsOf(table);
                const from = prefs.order.indexOf(index);
                const to = from + delta;
                if (from < 0 || to < 0 || to >= prefs.order.length) return;
                const list2 = [...prefs.order];
                [list2[from], list2[to]] = [list2[to], list2[from]];
                write(table, { order: list2, hidden: [...prefs.hidden] });
                apply(table);
                renderPanelList(table, list);
            };

            const up = document.createElement('button');
            up.type = 'button';
            up.className = 'grid h-6 w-5 place-items-center rounded text-slate-400 transition hover:bg-surface-secondary hover:text-primary disabled:opacity-30 disabled:hover:bg-transparent';
            up.innerHTML = chevron(true);
            up.disabled = position === 0;
            up.addEventListener('click', () => moveBy(-1));

            const down = document.createElement('button');
            down.type = 'button';
            down.className = up.className;
            down.innerHTML = chevron(false);
            down.disabled = position === ordered.length - 1;
            down.addEventListener('click', () => moveBy(1));

            arrows.append(up, down);
            row.appendChild(arrows);
            list.appendChild(row);
        });
    }

    function setup(table) {
        if (!table.tHead || !table.tBodies[0]) return;
        if (visibleColumns(table).length < 2) return;
        apply(table);
        toolbar(table);
    }

    function setupAll(root) {
        (root || document).querySelectorAll('table.admin-table').forEach(setup);
    }

    UI.applyTableColumns = apply;

    document.addEventListener('DOMContentLoaded', () => setupAll());
    document.addEventListener('click', (e) => {
        // Sıralama zamanı siyahı yenidən çəkilir — kliklənən düymə DOM-dan düşür
        // və closest() null qaytarır. Bu halda panelin bağlanması səhv olardı.
        if (e.target instanceof Element && !e.target.isConnected) return;
        if (!e.target.closest?.('.table-col-tools')) closePanels();
    });
    document.addEventListener('keydown', (e) => { if (e.key === 'Escape') closePanels(); });

    if (typeof MutationObserver === 'function') {
        let queued = false;
        new MutationObserver(() => {
            if (queued) return;
            queued = true;
            requestAnimationFrame(() => { queued = false; setupAll(); });
        }).observe(document.documentElement, { childList: true, subtree: true });
    }
})(window.AdminUI);
