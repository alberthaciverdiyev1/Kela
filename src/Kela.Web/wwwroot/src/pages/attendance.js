(function () {
    'use strict';

    const container = document.getElementById('attendance-page');
    if (!container) return;

    const picker = document.getElementById('attendance-picker');
    const saveUrl = '/teacher/workspaces/' + container.dataset.workspaceId + '/attendance';
    const daysInMonth = parseInt(container.dataset.days, 10);
    const saveSuccess = container.dataset.saveSuccess;
    const saveError = container.dataset.saveError;
    const unknownLabel = container.dataset.labelUnknown;

    const ICON_PATHS = {
        'check-circle': '<path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>',
        'x': '<line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>',
        'clock': '<circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>',
        'shield': '<path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>'
    };

    const statusLabels = {};
    picker.querySelectorAll('[data-pick-status]').forEach(function (b) {
        statusLabels[parseInt(b.dataset.pickStatus, 10)] = b.textContent.trim();
    });

    const statuses = new Map();
    const counts = [0, 0, 0, 0, 0];
    let activeCell = null;

    function svgIcon(name, cls) {
        return '<svg class="' + cls + '" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="20" height="20">' + ICON_PATHS[name] + '</svg>';
    }

    function iconNameFor(status) {
        return status === 1 ? 'check-circle'
            : status === 2 ? 'x'
            : status === 3 ? 'clock'
            : 'shield';
    }

    function colorFor(status) {
        return status === 1 ? 'bg-success text-white'
            : status === 2 ? 'bg-error text-white'
            : status === 3 ? 'bg-warning text-white'
            : status === 4 ? 'bg-info text-white'
            : 'bg-base-200 text-base-content/40';
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = String(value);
    }

    function initStatuses() {
        document.querySelectorAll('[data-cell]').forEach(function (cell) {
            const studentId = parseInt(cell.dataset.studentId, 10);
            const date = cell.dataset.date;
            const status = parseInt(cell.dataset.status, 10);
            let map = statuses.get(studentId);
            if (!map) {
                map = new Map();
                statuses.set(studentId, map);
            }
            if (status > 0 && !map.has(date)) {
                map.set(date, status);
                counts[status]++;
            }
        });
    }

    function renderStats() {
        setText('stat-total', statuses.size);
        setText('stat-present', counts[1]);
        setText('stat-absent', counts[2]);
        setText('stat-late', counts[3]);
        setText('stat-excused', counts[4]);
        const marked = counts[1] + counts[2] + counts[3] + counts[4];
        const rate = marked === 0 ? 0 : Math.round((counts[1] + counts[3] + counts[4]) / marked * 100);
        setText('stat-rate', rate + '%');
    }

    function setCellStatus(cell, status) {
        cell.dataset.status = String(status);
        cell.title = status === 0 ? unknownLabel : (statusLabels[status] || '');
        if (cell.dataset.cellLayout === 'mobile') {
            cell.className = 'attendance-cell flex size-7 items-center justify-center rounded-md text-xs font-semibold ' +
                colorFor(status) + ' cursor-pointer transition hover:ring-2 hover:ring-primary/40 disabled:opacity-50 disabled:cursor-wait';
            cell.textContent = String(Number(cell.dataset.date.slice(-2)));
        } else {
            cell.className = 'attendance-cell mx-auto flex size-6 items-center justify-center rounded-md ' +
                colorFor(status) + ' cursor-pointer transition hover:ring-2 hover:ring-primary/40 disabled:opacity-50 disabled:cursor-wait';
            cell.innerHTML = status === 0
                ? '<span class="size-1.5 rounded-full bg-base-content/20"></span>'
                : svgIcon(iconNameFor(status), 'w-3.5 h-3.5');
        }
    }

    function openPicker(cell) {
        activeCell = cell;
        const rect = cell.getBoundingClientRect();
        picker.classList.remove('hidden');
        const pickerRect = picker.getBoundingClientRect();
        let left = rect.right + 6;
        let top = rect.top;
        if (left + pickerRect.width > window.innerWidth - 8) left = rect.left - pickerRect.width - 6;
        if (left < 8) left = 8;
        if (top + pickerRect.height > window.innerHeight - 8) top = window.innerHeight - pickerRect.height - 8;
        if (top < 8) top = 8;
        picker.style.left = left + 'px';
        picker.style.top = top + 'px';
    }

    function closePicker() {
        picker.classList.add('hidden');
        activeCell = null;
    }

    async function applyStatus(cell, newStatus) {
        const studentId = parseInt(cell.dataset.studentId, 10);
        const date = cell.dataset.date;
        const oldStatus = (statuses.get(studentId) || new Map()).get(date) || 0;
        const cells = document.querySelectorAll('[data-cell][data-student-id="' + studentId + '"][data-date="' + date + '"]');
        closePicker();
        cells.forEach(function (c) { c.disabled = true; });

        try {
            const res = await Kela.axios.put(saveUrl, {
                studentId: studentId,
                date: date,
                status: newStatus
            });
            if (!res.data || !res.data.success) {
                throw new Error('save failed');
            }
            let map = statuses.get(studentId);
            if (!map) {
                map = new Map();
                statuses.set(studentId, map);
            }
            map.set(date, newStatus);
            if (oldStatus > 0) counts[oldStatus]--;
            counts[newStatus]++;
            cells.forEach(function (c) { setCellStatus(c, newStatus); });
            renderStats();
            Kela.toast(saveSuccess, 'success');
        } catch (e) {
            cells.forEach(function (c) { setCellStatus(c, oldStatus); });
            Kela.toast(saveError, 'error');
        } finally {
            cells.forEach(function (c) { c.disabled = false; });
        }
    }

    function matches(name, studentId, q, f) {
        if (q && name.indexOf(q) === -1) return false;
        if (f === -1) return true;
        const map = statuses.get(studentId);
        if (f === 0) return !map || map.size < daysInMonth;
        if (!map) return false;
        let has = false;
        map.forEach(function (status) { if (status === f) has = true; });
        return has;
    }

    const searchInput = document.getElementById('attendance-search');
    const filterSelect = document.getElementById('attendance-status-filter');

    function applyFilters() {
        const q = searchInput ? searchInput.value.trim().toLowerCase() : '';
        const f = filterSelect ? parseInt(filterSelect.value, 10) : -1;
        document.querySelectorAll('[data-student-row]').forEach(function (row) {
            row.hidden = !matches(row.dataset.studentName, parseInt(row.dataset.studentId, 10), q, f);
        });
        document.querySelectorAll('[data-student-card]').forEach(function (card) {
            card.hidden = !matches(card.dataset.studentName, parseInt(card.dataset.studentId, 10), q, f);
        });
    }

    let searchDebounce;
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            clearTimeout(searchDebounce);
            searchDebounce = setTimeout(applyFilters, 150);
        });
        searchInput.addEventListener('search', applyFilters);
    }
    if (filterSelect) {
        filterSelect.addEventListener('change', applyFilters);
    }

    const monthInput = document.getElementById('attendance-month');
    if (monthInput) {
        monthInput.addEventListener('change', function () {
            if (monthInput.value) {
                window.location.href = location.pathname + '?month=' + encodeURIComponent(monthInput.value);
            }
        });
    }

    document.addEventListener('click', function (event) {
        const cell = event.target.closest('[data-cell]');
        if (cell && !cell.disabled) {
            openPicker(cell);
            return;
        }
        const pick = event.target.closest('[data-pick-status]');
        if (pick && activeCell) {
            applyStatus(activeCell, parseInt(pick.dataset.pickStatus, 10));
            return;
        }
        if (event.target.closest('#attendance-picker')) return;
        closePicker();
    });

    initStatuses();
    renderStats();
})();
