let dp = null;
let monthMap = {};
let current = new Date();
let ready = false;
let selected = null;

function gid() { return document.getElementById('group_id').value; }

function ymd(d) {
    return d.getFullYear() + '-' +
        String(d.getMonth() + 1).padStart(2, '0') + '-' +
        String(d.getDate()).padStart(2, '0');
}

function esc(v) {
    return String(v ?? '').replace(/[&<>"']/g, c =>
        ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

/* --- ay takvimi --- */

function dayClass(date) {
    const row = monthMap[ymd(date)];
    if (!row || !row.totalCount) return {};

    const ratio = row.presentCount / row.totalCount;
    return { classes: ratio >= 1 ? 'att-full' : ratio >= 0.5 ? 'att-part' : 'att-low' };
}

function renderMonthSummary(data) {
    const el = document.getElementById('monthSummary');
    if (!data) { el.textContent = ''; return; }

    const days = data.days || [];
    const present = days.reduce((s, d) => s + d.presentCount, 0);
    const total = days.reduce((s, d) => s + d.totalCount, 0);

    el.textContent = total
        ? L["Attendance_MonthSummary"].replace('{0}', present).replace('{1}', total)
        : '';
}

async function loadMonth() {
    const id = gid();
    if (!id) {
        monthMap = {};
        renderMonthSummary(null);
        if (dp) dp.update();
        return;
    }

    const year = current.getFullYear();
    const month = current.getMonth() + 1;

    const data = await AdminUI.get(`/attendance/month?group_id=${id}&year=${year}&month=${month}`);
    monthMap = {};
    (data.days || []).forEach(d => { monthMap[d.date] = d; });

    renderMonthSummary(data);
    if (dp) dp.update();
}

function afterMonthSwitch() {
    if (!dp) return;
    const v = dp.viewDate;
    if (v) current = new Date(v.getFullYear(), v.getMonth(), 1);
    loadMonth();
}

function initCalendar() {
    const el = document.getElementById('calendar');
    el.innerHTML = '';
    monthMap = {};
    ready = false;

    dp = new Datepicker(el, {
        inline: true,
        language: 'az',
        todayHighlight: true,
        beforeShowDay: dayClass,
        changeMonth: afterMonthSwitch,
        changeYear: afterMonthSwitch
    });

    // Gün seçimi: kitabxananın callback-inə güvənmək yerinə klikləri özümüz tuturuq.
    el.addEventListener('click', ev => {
        const cell = ev.target.closest('.datepicker-cell');
        if (!cell || !cell.dataset.date) return;

        const grid = cell.closest('.datepicker-grid');
        if (grid && (grid.classList.contains('months') || grid.classList.contains('years'))) return;

        const date = new Date(cell.dataset.date);
        if (isNaN(date)) return;

        selected = ymd(date);
        loadDay(selected);
    });

    dp.setDate(current);
    ready = true;
}

/* --- gün paneli --- */

async function loadDay(date) {
    const id = gid();
    if (!id) { AdminUI.error(L["Common_SelectGroup"]); return; }

    const panel = document.getElementById('dayPanel');
    panel.className = 'admin-empty';
    panel.textContent = '…';

    const data = await AdminUI.get(`/attendance/day?group_id=${id}&date=${date}`);

    if (!data.students || !data.students.length) {
        panel.className = 'admin-empty';
        panel.textContent = L["Js_NoStudents"];
        return;
    }

    panel.className = '';
    panel.innerHTML = `
        <div class="flex flex-wrap items-center justify-between gap-3 border-b border-border pb-4">
            <div>
                <div class="font-bold text-slate-800">${esc(date)}</div>
                <div id="dayCount" class="text-sm text-slate-500">${data.presentCount} / ${data.totalCount}</div>
            </div>
            <div class="flex gap-2">
                <button type="button" class="admin-button-secondary" onclick="markAll(true)">${L["Attendance_AllPresent"]}</button>
                <button type="button" class="admin-button-secondary" onclick="markAll(false)">${L["Attendance_AllAbsent"]}</button>
                <button type="button" id="saveBtn" class="admin-button-primary" onclick="saveDay()">${L["Common_Save"]}</button>
            </div>
        </div>
        <div class="mt-4 space-y-2">
            ${data.students.map(s => `
                <label class="flex items-center gap-3 rounded-xl border border-border px-3 py-2 hover:bg-slate-50">
                    <input type="checkbox" class="att-check size-4" data-student="${s.studentId}" ${s.isPresent ? 'checked' : ''} />
                    <span class="min-w-0 flex-1">
                        <span class="block truncate text-sm font-medium text-slate-800">${esc(s.fullName)}</span>
                        <span class="block truncate text-xs text-slate-400">${esc(s.email)}</span>
                    </span>
                </label>`).join('')}
        </div>`;
}

function markAll(present) {
    document.querySelectorAll('.att-check').forEach(c => { c.checked = present; });
}

async function saveDay() {
    const id = gid();
    if (!id || !selected) return;

    const students = Array.from(document.querySelectorAll('.att-check')).map(c => ({
        studentId: Number(c.dataset.student),
        isPresent: c.checked
    }));

    const btn = document.getElementById('saveBtn');
    if (btn) btn.disabled = true;

    const res = await AdminUI.postJson('/attendance', { groupId: Number(id), date: selected, students });

    if (btn) btn.disabled = false;

    if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

    AdminUI.notify(L["Js_Saved"]);
    await loadMonth();
    loadDay(selected);
}

/* --- başlanğıc --- */

document.addEventListener('DOMContentLoaded', async () => {
    initCalendar();
    document.getElementById('group_id').addEventListener('change', async () => {
        document.getElementById('dayPanel').className = 'admin-empty';
        document.getElementById('dayPanel').textContent = L["Attendance_PickDay"];
        await loadMonth();
    });
});

// Səhifə JS-dən çağırılan funksiyalar qlobal olmalıdır (onclick).
window.markAll = markAll;
window.saveDay = saveDay;
