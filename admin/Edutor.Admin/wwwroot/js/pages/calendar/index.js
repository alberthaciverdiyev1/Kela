let current = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
let events = [];
let selected = null;

function ymd(d) {
    return d.getFullYear() + '-' +
        String(d.getMonth() + 1).padStart(2, '0') + '-' +
        String(d.getDate()).padStart(2, '0');
}

// Chrome az paketində ay adı "M10" kimi gəlir; az/tr/ru üçün adlar sabit verilir.
const CAL_NAMES = {
    az: {
        months: ['Yanvar', 'Fevral', 'Mart', 'Aprel', 'May', 'İyun', 'İyul', 'Avqust', 'Sentyabr', 'Oktyabr', 'Noyabr', 'Dekabr'],
        days: ['B.e', 'Ç.a', 'Ç', 'C.a', 'C', 'Ş', 'B']
    },
    tr: {
        months: ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'],
        days: ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz']
    },
    ru: {
        months: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь', 'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'],
        days: ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс']
    }
};

function names() {
    return CAL_NAMES[(window.CAL_LOCALE || '').slice(0, 2)] || null;
}

function monthLabel() {
    const n = names();
    return n ? `${n.months[current.getMonth()]} ${current.getFullYear()}`
        : `${current.getMonth() + 1}/${current.getFullYear()}`;
}

function weekdayNames() {
    const n = names();
    if (n) return n.days;
    const f = new Intl.DateTimeFormat(window.CAL_LOCALE || 'en', { weekday: 'short' });
    return Array.from({ length: 7 }, (_, i) => f.format(new Date(2024, 0, 1 + i)));
}

function dayLabel(key) {
    const [y, m, d] = key.split('-').map(Number);
    const n = names();
    return n ? `${d} ${n.months[m - 1]} ${y}` : key;
}

function byDate(date) {
    return events.filter(e => e.date === date);
}

function renderCalendar() {
    const first = new Date(current.getFullYear(), current.getMonth(), 1);
    const lead = (first.getDay() + 6) % 7;
    const days = new Date(current.getFullYear(), current.getMonth() + 1, 0).getDate();
    const today = ymd(new Date());

    let cells = '';
    for (let i = 0; i < lead; i++) cells += '<span></span>';
    for (let d = 1; d <= days; d++) {
        const key = ymd(new Date(current.getFullYear(), current.getMonth(), d));
        const dayEvents = byDate(key);
        const dots = ['note', 'homework']
            .filter(type => dayEvents.some(e => e.type === type))
            .map(type => `<i class="cal-dot cal-${type}"></i>`).join('');

        cells += `
            <div class="cal-day flex aspect-square cursor-pointer select-none flex-col items-center justify-center rounded-xl text-sm font-semibold text-slate-600 hover:bg-slate-100 ${key === today ? 'ring-2 ring-primary/40' : ''} ${key === selected ? 'cal-selected bg-primary-light text-primary' : ''}"
                 data-day="${key}">
                <span>${d}</span>
                <span class="mt-0.5 flex h-1.5 gap-0.5">${dots}</span>
            </div>`;
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
            selected = cell.dataset.day;
            renderCalendar();
            renderDay();
        });
    });
}

function eventRow(e) {
    const badge = e.type === 'homework'
        ? AdminUI.badge(AdminUI.escapeHtml(L["Calendar_Homework"]), 'primary')
        : AdminUI.badge(AdminUI.escapeHtml(L["Calendar_Note"]), e.isDone ? 'success' : 'warning');

    const title = e.url
        ? `<a href="${AdminUI.escapeHtml(e.url)}" class="font-semibold text-slate-800 hover:text-primary">${AdminUI.escapeHtml(e.title)}</a>`
        : `<span class="font-semibold text-slate-800">${AdminUI.escapeHtml(e.title)}</span>`;

    return `<div class="flex items-start justify-between gap-3 rounded-xl border border-border px-3 py-2">
        <div class="min-w-0">
            ${title}
            ${e.subtitle ? `<span class="mt-0.5 block truncate text-xs text-slate-400">${AdminUI.escapeHtml(e.subtitle)}</span>` : ''}
        </div>
        <span class="shrink-0">${badge}</span>
    </div>`;
}

function renderDay() {
    const panel = document.getElementById('dayPanel');
    if (!selected) {
        panel.className = 'admin-card admin-empty p-5';
        panel.textContent = L["Calendar_SelectDay"];
        return;
    }

    const list = byDate(selected);
    panel.className = 'admin-card p-5';

    panel.innerHTML = `
        <p class="font-bold text-slate-800">${AdminUI.escapeHtml(dayLabel(selected))}</p>
        <div class="mt-3 space-y-2">
            ${list.length
                ? list.map(eventRow).join('')
                : `<p class="text-sm text-slate-400">${AdminUI.escapeHtml(L["Calendar_NoDayEvents"])}</p>`}
        </div>`;
}

async function loadMonth() {
    const y = current.getFullYear(), m = current.getMonth() + 1;
    events = await AdminUI.get(`/calendar/month?year=${y}&month=${m}`) || [];

    const summary = document.getElementById('monthSummary');
    summary.textContent = events.length ? L["Calendar_Summary"].replace('{0}', events.length) : L["Calendar_NoEvents"];

    renderCalendar();
    renderDay();
}

document.addEventListener('DOMContentLoaded', () => loadMonth());
