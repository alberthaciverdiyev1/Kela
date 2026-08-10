(function () {
    'use strict';

    const container = document.getElementById('attendance-page');
    if (!container) return;

    const picker = document.getElementById('attendance-picker');
    const grid = document.getElementById('attendance-grid');
    const saveUrl = '/teacher/attendance';
    const workspaceId = parseInt(container.dataset.workspaceId, 10);
    const year = parseInt(container.dataset.year, 10);
    const month = parseInt(container.dataset.month, 10);
    const daysInMonth = parseInt(container.dataset.days, 10);
    const saveSuccess = Kela.t('attendance.saveSuccess');
    const saveError = Kela.t('attendance.saveError');
    const unknownLabel = Kela.t('attendance.status.unknown');
    const studentLabel = Kela.t('attendance.student');

    const statusLabels = {};
    picker.querySelectorAll('[data-pick-status]').forEach(function (b) {
        statusLabels[parseInt(b.dataset.pickStatus, 10)] = b.textContent.trim();
    });

    const statuses = new Map();
    const counts = [0, 0, 0, 0, 0];
    let activeCell = null;
    let studentIds = [];
    let studentNames = {};

    function dateStr(y, m, d) {
        return y + '-' + String(m).padStart(2, '0') + '-' + String(d).padStart(2, '0');
    }

    function statusFor(id, date) {
        let map = statuses.get(id);
        return map ? (map.get(date) || 0) : 0;
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

    function cellHtml(layout, id, date, status) {
        let title = status === 0 ? unknownLabel : (statusLabels[status] || '');
        if (layout === 'mobile') {
            return '<button type="button" data-cell data-cell-layout="mobile" data-student-id="' + id + '" data-date="' + date + '" data-status="' + status + '" title="' + Kela.esc(title) + '" class="attendance-cell flex size-10 items-center justify-center rounded-md text-sm font-semibold ' + colorFor(status) + ' cursor-pointer transition hover:ring-2 hover:ring-primary/40 disabled:opacity-50 disabled:cursor-wait">' + String(Number(date.slice(-2))) + '</button>';
        }
        let inner = status === 0
            ? '<span class="size-1.5 rounded-full bg-base-content/20"></span>'
            : Kela.icon(iconNameFor(status), 'w-3.5 h-3.5');
        return '<button type="button" data-cell data-cell-layout="desktop" data-student-id="' + id + '" data-date="' + date + '" data-status="' + status + '" title="' + Kela.esc(title) + '" class="attendance-cell mx-auto flex size-9 items-center justify-center rounded-md ' + colorFor(status) + ' cursor-pointer transition hover:ring-2 hover:ring-primary/40 disabled:opacity-50 disabled:cursor-wait">' + inner + '</button>';
    }

    function renderDesktop() {
        let html = '<div class="hidden lg:block"><div class="card overflow-hidden border border-base-200 bg-base-100">';
        html += '<div class="attendance-scroll overflow-auto max-h-[calc(100vh-260px)]">';
        html += '<table class="attendance-table table table-fixed w-full border-separate border-spacing-0 text-sm">';
        html += '<thead><tr>';
        html += '<th class="sticky left-0 top-0 z-30 w-56 bg-base-200 px-3 py-3 text-left text-xs font-semibold uppercase tracking-wider text-base-content/50">' + Kela.esc(Kela.t('attendance.student')) + '</th>';
        for (let day = 1; day <= daysInMonth; day++) {
            html += '<th class="sticky top-0 z-20 w-14 bg-base-200 py-3 text-center text-sm font-medium text-base-content/50">' + day + '</th>';
        }
        html += '</tr></thead><tbody>';
        studentIds.forEach(function (id) {
            html += '<tr class="hover:bg-base-200/50" data-student-row data-student-id="' + id + '" data-student-name="' + Kela.esc(studentNames[id].toLowerCase()) + '">';
            html += '<td class="sticky left-0 z-10 bg-base-100 px-3 py-2 font-medium">' + Kela.esc(studentNames[id]) + '</td>';
            for (let day = 1; day <= daysInMonth; day++) {
                let date = dateStr(year, month, day);
                html += '<td class="p-1 text-center">' + cellHtml('desktop', id, date, statusFor(id, date)) + '</td>';
            }
            html += '</tr>';
        });
        html += '</tbody></table></div></div></div>';
        return html;
    }

    function renderMobile() {
        let html = '<div class="grid grid-cols-1 gap-4 lg:hidden">';
        studentIds.forEach(function (id) {
            html += '<div class="card border border-base-200 bg-base-100 p-4" data-student-card data-student-id="' + id + '" data-student-name="' + Kela.esc(studentNames[id].toLowerCase()) + '">';
            html += '<div class="mb-3 flex items-center justify-between"><span class="text-sm font-semibold">' + Kela.esc(studentNames[id]) + '</span><span class="badge badge-ghost">' + Kela.esc(studentLabel) + '</span></div>';
            html += '<div class="flex flex-wrap gap-1.5">';
            for (let day = 1; day <= daysInMonth; day++) {
                let date = dateStr(year, month, day);
                html += cellHtml('mobile', id, date, statusFor(id, date));
            }
            html += '</div></div>';
        });
        html += '</div>';
        return html;
    }

    function renderGrid(data) {
        studentIds = [];
        studentNames = {};
        (data.students || []).forEach(function (s) {
            studentIds.push(s.id);
            studentNames[s.id] = s.name;
        });

        statuses.clear();
        counts[0] = counts[1] = counts[2] = counts[3] = counts[4] = 0;
        (data.records || []).forEach(function (r) {
            let map = statuses.get(r.studentId);
            if (!map) {
                map = new Map();
                statuses.set(r.studentId, map);
            }
            map.set(r.date, r.status);
            counts[r.status]++;
        });

        if (!grid) return;
        if (!studentIds.length) {
            grid.innerHTML = '<div class="card border border-base-200 bg-base-100 items-center text-center py-10 px-4">' +
                '<span class="text-base-300 mb-3">' + Kela.icon('calendar', 'w-12 h-12') + '</span>' +
                '<p class="text-base-content/60 max-w-sm">' + Kela.esc(Kela.t('attendance.empty')) + '</p></div>';
        } else {
            grid.innerHTML = renderDesktop() + renderMobile();
        }
        renderStats();
        applyFilters();
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = String(value);
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
            cell.className = 'attendance-cell flex size-10 items-center justify-center rounded-md text-sm font-semibold ' +
                colorFor(status) + ' cursor-pointer transition hover:ring-2 hover:ring-primary/40 disabled:opacity-50 disabled:cursor-wait';
            cell.textContent = String(Number(cell.dataset.date.slice(-2)));
        } else {
            cell.className = 'attendance-cell mx-auto flex size-9 items-center justify-center rounded-md ' +
                colorFor(status) + ' cursor-pointer transition hover:ring-2 hover:ring-primary/40 disabled:opacity-50 disabled:cursor-wait';
            cell.innerHTML = status === 0
                ? '<span class="size-1.5 rounded-full bg-base-content/20"></span>'
                : Kela.icon(iconNameFor(status), 'w-3.5 h-3.5');
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
                workspaceId: workspaceId,
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
            Kela.notify.success(saveSuccess);
        } catch (e) {
            cells.forEach(function (c) { setCellStatus(c, oldStatus); });
            Kela.notify.error(saveError);
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
    const workspaceDd = document.getElementById('attendance-workspace-dd');
    const workspaceLabel = document.getElementById('attendance-workspace-label');
    const workspaceSearch = document.getElementById('attendance-workspace-search');
    const workspaceList = document.getElementById('attendance-workspace-list');
    let selectedWorkspaceId = workspaceId;

    function attendanceUrl(ws, month) {
        const params = new URLSearchParams();
        if (ws) params.set('workspaceId', ws);
        if (month) params.set('month', month);
        const query = params.toString();
        return location.pathname + (query ? '?' + query : '');
    }

    function renderWorkspaceList(items) {
        if (!workspaceList) return;
        if (!items.length) {
            workspaceList.innerHTML = '<li class="p-2 text-sm text-base-content/50">' + Kela.esc(Kela.t('workspaces.empty')) + '</li>';
            return;
        }
        workspaceList.innerHTML = items.map(function (w) {
            const active = w.id === selectedWorkspaceId ? ' active' : '';
            return '<li><a data-workspace-value="' + w.id + '" class="' + active + '">' + Kela.esc(w.name) + '</a></li>';
        }).join('');
    }

    if (workspaceSearch) {
        let wsDebounce;
        workspaceSearch.addEventListener('input', function () {
            clearTimeout(wsDebounce);
            const q = workspaceSearch.value.trim();
            wsDebounce = setTimeout(function () {
                Kela.axios.get('/teacher/workspaces/table', {
                    params: { search: q, page: 1 }
                }).then(function (res) {
                    renderWorkspaceList(res.data.items || []);
                }).catch(function () {
                });
            }, 300);
        });
    }

    Kela.onPageEvent('click', function (event) {
        const wsItem = event.target.closest('#attendance-workspace-list [data-workspace-value]');
        if (wsItem) {
            selectedWorkspaceId = parseInt(wsItem.dataset.workspaceValue, 10);
            if (workspaceLabel) workspaceLabel.textContent = wsItem.textContent;
            if (workspaceDd) workspaceDd.removeAttribute('open');
            Kela.navigate(attendanceUrl(selectedWorkspaceId, monthInput ? monthInput.value : ''));
        }
    });
    if (monthInput) {
        monthInput.addEventListener('change', function () {
            if (monthInput.value) {
                Kela.navigate(attendanceUrl(selectedWorkspaceId, monthInput.value));
            }
        });
    }

    Kela.onPageEvent('click', function (event) {
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

    if (grid) {
        Kela.axios.get('/teacher/attendance/data', {
            params: {
                workspaceId: String(workspaceId),
                month: dateStr(year, month, 1).slice(0, 7)
            }
        }).then(function (res) {
            renderGrid(res.data);
        }).catch(function () {
        });
    }
})();
