let monthMap = {};
let current = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
let groupId = null;
let selected = null;

function ymd(d) {
    return d.getFullYear() + '-' +
        String(d.getMonth() + 1).padStart(2, '0') + '-' +
        String(d.getDate()).padStart(2, '0');
}

/* --- dil dəstəyi (ICU yoxdursa fallback) --- */
function fmt(options, fallback) {
    try { return new Intl.DateTimeFormat(window.ATT_LOCALE || undefined, options); }
    catch (e) { return null; }
}

function monthLabel() {
    const f = fmt({ month: 'long', year: 'numeric' });
    return f ? f.format(current) : (current.getMonth() + 1) + '/' + current.getFullYear();
}

function weekdayNames() {
    const f = fmt({ weekday: 'short' });
    if (!f) return ['B.e', 'Ç.a', 'Ç', 'C.a', 'C', 'Ş', 'B'];

    // Bazar ertəsi birinci: 2024-01-01 bazar ertəsidir.
    return Array.from({ length: 7 }, (_, i) => f.format(new Date(2024, 0, 1 + i)));
}

/* --- qrup çipləri --- */
function renderChips() {
    const el = document.getElementById('groupChips');
    const groups = window.ATT_GROUPS || [];

    if (!groups.length) {
        el.innerHTML = `<span class="px-1 text-sm text-slate-400">${AdminUI.escapeHtml(L["Attendance_NoGroups"])}</span>`;
        return;
    }

    el.innerHTML = groups.map(g => `
        <button type="button" data-id="${g.id}"
                class="att-chip shrink-0 rounded-full border px-4 py-1.5 text-[13px] font-semibold transition
                       ${g.id === groupId
                            ? 'border-primary bg-primary-light text-primary'
                            : 'border-slate-200 bg-white text-slate-500 hover:bg-slate-50'}">
            ${AdminUI.escapeHtml(g.name)}
        </button>`).join('');

    el.querySelectorAll('.att-chip').forEach(btn => {
        btn.addEventListener('click', () => {
            groupId = Number(btn.dataset.id);
            renderChips();
            loadMonth();
            // Seçilmiş gün qalır — yeni qrupun həmin günü yüklənir.
            if (selected) loadDay();
        });
    });
}

/* --- ay təqvimi --- */
function dayClass(day) {
    const row = monthMap[ymd(new Date(current.getFullYear(), current.getMonth(), day))];
    const today = new Date();
    const isToday = current.getFullYear() === today.getFullYear()
        && current.getMonth() === today.getMonth() && day === today.getDate();
    const ring = isToday ? ' ring-2 ring-primary/40 ring-offset-1' : '';
    const key = ymd(new Date(current.getFullYear(), current.getMonth(), day));
    const picked = key === selected ? ' att-selected' : '';
    const base = 'flex aspect-square items-center justify-center rounded-xl text-sm font-semibold cursor-pointer select-none ';

    if (!row || !row.totalCount) return base + 'text-slate-400 hover:bg-slate-100' + ring + picked;

    const ratio = row.presentCount / row.totalCount;
    return base + (ratio >= 1 ? 'att-full' : ratio >= 0.5 ? 'att-part' : 'att-low') + ring + picked;
}

function renderCalendar() {
    const first = new Date(current.getFullYear(), current.getMonth(), 1);
    const lead = (first.getDay() + 6) % 7;
    const days = new Date(current.getFullYear(), current.getMonth() + 1, 0).getDate();

    let cells = '';
    for (let i = 0; i < lead; i++) cells += '<span></span>';
    for (let d = 1; d <= days; d++) {
        cells += `<div class="${dayClass(d)}" data-day="${d}">${d}</div>`;
    }

    document.getElementById('calendar').innerHTML = `
        <div class="flex items-center justify-between">
            <button type="button" id="prevMonth" class="rounded-xl p-2 text-slate-400 hover:bg-slate-100" aria-label="previous">‹</button>
            <p class="font-bold capitalize text-slate-800">${AdminUI.escapeHtml(monthLabel())}</p>
            <button type="button" id="nextMonth" class="rounded-xl p-2 text-slate-400 hover:bg-slate-100" aria-label="next">›</button>
        </div>
        <div class="mt-3 grid grid-cols-7 gap-1">
            ${weekdayNames().map(w => `<span class="text-center text-[10px] font-bold uppercase tracking-wide text-slate-400">${AdminUI.escapeHtml(w)}</span>`).join('')}
        </div>
        <div class="mt-1 grid grid-cols-7 gap-1">${cells}</div>`;

    document.getElementById('prevMonth').onclick = () => { current = new Date(current.getFullYear(), current.getMonth() - 1, 1); loadMonth(); };
    document.getElementById('nextMonth').onclick = () => { current = new Date(current.getFullYear(), current.getMonth() + 1, 1); loadMonth(); };

    document.querySelectorAll('#calendar [data-day]').forEach(cell => {
        cell.addEventListener('click', () => {
            selected = ymd(new Date(current.getFullYear(), current.getMonth(), Number(cell.dataset.day)));
            renderCalendar();   // seçilmiş günün işarəsi dərhal görünsün
            loadDay();
        });
    });
}

function renderSummary(data) {
    const el = document.getElementById('monthSummary');
    if (!data || !data.days || !data.days.length) { el.textContent = ''; return; }

    const present = data.days.reduce((s, d) => s + d.presentCount, 0);
    const total = data.days.reduce((s, d) => s + d.totalCount, 0);
    el.textContent = L["Attendance_MonthSummary"].replace('{0}', present).replace('{1}', total);
}

async function loadMonth() {
    monthMap = {};

    if (!groupId) { renderCalendar(); renderSummary(null); return; }

    const y = current.getFullYear(), m = current.getMonth() + 1;
    const data = await AdminUI.get(`/attendance/month?group_id=${groupId}&year=${y}&month=${m}`);

    (data.days || []).forEach(d => { monthMap[d.date] = d; });
    renderCalendar();
    renderSummary(data);
}

/* --- gün paneli --- */
async function loadDay() {
    if (!groupId || !selected) return;

    const panel = document.getElementById('dayPanel');
    panel.className = 'admin-empty';
    panel.textContent = '…';

    const data = await AdminUI.get(`/attendance/day?group_id=${groupId}&date=${selected}`);

    if (!data.students || !data.students.length) {
        panel.className = 'admin-empty';
        panel.textContent = L["Js_NoStudents"];
        return;
    }

    panel.className = '';
    const recorded = data.students.some(s => s.isRecorded);

    panel.innerHTML = `
        <div class="flex flex-wrap items-center justify-between gap-3 border-b border-border pb-4">
            <div>
                <div class="font-bold text-slate-800">${AdminUI.escapeHtml(selected)}</div>
                <div class="mt-0.5 flex items-center gap-2 text-sm">
                    <span id="dayCount" class="text-slate-500">${data.presentCount} / ${data.totalCount}</span>
                    ${recorded
                        ? AdminUI.badge(AdminUI.escapeHtml(L["Attendance_Saved"]), 'success')
                        : AdminUI.badge(AdminUI.escapeHtml(L["Attendance_LegendNone"]), 'neutral')}
                </div>
            </div>
            <div class="flex gap-2">
                <button type="button" class="admin-button-secondary" ${AdminUI.actionAttrs('markAll', [true])}>${AdminUI.escapeHtml(L["Attendance_AllPresent"])}</button>
                <button type="button" class="admin-button-secondary" ${AdminUI.actionAttrs('markAll', [false])}>${AdminUI.escapeHtml(L["Attendance_AllAbsent"])}</button>
                <button type="button" id="saveBtn" class="admin-button-primary" ${AdminUI.actionAttrs('saveDay', [])}>${AdminUI.escapeHtml(L["Common_Save"])}</button>
            </div>
        </div>
        <div class="mt-4 space-y-2">
            ${data.students.map(s => `
                <label class="att-row flex items-center gap-3 rounded-xl border px-3 py-2 transition"
                       data-student="${s.studentId}">
                    <input type="checkbox" class="att-check size-4" data-student="${s.studentId}"
                           data-recorded="${s.isRecorded ? '1' : '0'}" ${s.isPresent ? 'checked' : ''} />
                    <span class="min-w-0 flex-1">
                        <span class="block truncate text-sm font-medium text-slate-800">${AdminUI.escapeHtml(s.fullName)}</span>
                        <span class="block truncate text-xs text-slate-400">${AdminUI.escapeHtml(s.email)}</span>
                    </span>
                    <span class="att-state shrink-0 text-xs font-semibold"></span>
                </label>`).join('')}
        </div>`;

    // İşarə "qayıb" deməkdir, ona görə vəziyyət canlı göstərilir ki,
    // "qayıb" ilə "qeyd olunmayıb" qarışmasın.
    panel.querySelectorAll('.att-row').forEach(row => {
        const box = row.querySelector('.att-check');
        // Sətir yalnız açıq şəkildə "qayıb" olanda qırmızıdır: ya əvvəl qeyd
        // olunub, ya da istifadəçi indi işarəni götürüb.
        const paint = (explicit) => {
            const absent = !box.checked && (explicit || box.dataset.recorded === '1');
            row.classList.toggle('att-present', box.checked);
            row.classList.toggle('att-absent', absent);
            row.querySelector('.att-state').textContent =
                box.checked ? L["Attendance_Present"] : (absent ? L["Attendance_Absent"] : '');

            const el = document.getElementById('dayCount');
            if (el) {
                const checked = panel.querySelectorAll('.att-check:checked').length;
                const total = panel.querySelectorAll('.att-check').length;
                el.textContent = checked + ' / ' + total;
            }
        };
        box.addEventListener('change', () => paint(true));
        paint(false);
    });
}

function markAll(present) {
    document.querySelectorAll('.att-check').forEach(c => {
        c.checked = present;
        c.dispatchEvent(new Event('change', { bubbles: true }));
    });
}

async function saveDay() {
    if (!groupId || !selected) return;

    const students = Array.from(document.querySelectorAll('.att-check')).map(c => ({
        studentId: Number(c.dataset.student),
        isPresent: c.checked
    }));

    const btn = document.getElementById('saveBtn');
    if (btn) btn.disabled = true;

    const res = await AdminUI.postJson('/attendance/save', {
        groupId: Number(groupId), date: selected, students
    });

    if (btn) btn.disabled = false;

    if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

    AdminUI.notify(L["Js_Saved"]);
    await loadMonth();
    await loadDay();
}

document.addEventListener('DOMContentLoaded', () => {
    const groups = window.ATT_GROUPS || [];
    if (groups.length) groupId = groups[0].id;

    renderChips();
    loadMonth();
});

window.markAll = markAll;
window.saveDay = saveDay;
