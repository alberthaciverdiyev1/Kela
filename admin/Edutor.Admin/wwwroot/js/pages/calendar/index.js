// Google Calendar üslubunda tam səhifə təqvim: ay / həftə / gün görünüşləri,
// müəllim öz hadisələrini saatla yaradır. Ev tapşırıqlarının son tarixləri
// yalnız oxunur (all-day) kimi göstərilir.
(function () {
    const COLORS = {
        default: '#ffffff', red: '#f8b4b4', orange: '#fdd9a0', yellow: '#fff3a3',
        green: '#c9f2b2', teal: '#a9efe4', blue: '#bfe3fb', purple: '#dcc9f7',
        pink: '#f9c9e0', gray: '#e2e5e9', brown: '#e3cbb0'
    };
    const HOMEWORK_COLOR = '#f59e0b';
    const HOUR_HEIGHT = 48;

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

    let view = 'month';
    let cursor = startOfDay(new Date());
    let events = [];
    let draftColor = 'green';

    const el = (id) => document.getElementById(id);
    const pad = (n) => String(n).padStart(2, '0');

    function names() {
        return CAL_NAMES[(window.CAL_LOCALE || '').slice(0, 2)] || null;
    }

    function monthName(month) {
        const n = names();
        return n ? n.months[month] : new Intl.DateTimeFormat(window.CAL_LOCALE || 'en', { month: 'long' }).format(new Date(2024, month, 1));
    }

    function weekdayNames() {
        const n = names();
        if (n) return n.days;
        const f = new Intl.DateTimeFormat(window.CAL_LOCALE || 'en', { weekday: 'short' });
        return Array.from({ length: 7 }, (_, i) => f.format(new Date(2024, 0, 1 + i)));
    }

    function startOfDay(d) { const x = new Date(d); x.setHours(0, 0, 0, 0); return x; }
    function addDays(d, n) { const x = new Date(d); x.setDate(x.getDate() + n); return x; }
    function startOfWeek(d) { return addDays(startOfDay(d), -((d.getDay() + 6) % 7)); }
    function ymd(d) { return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`; }
    function hm(d) { return `${pad(d.getHours())}:${pad(d.getMinutes())}`; }
    function sameDay(a, b) { return ymd(a) === ymd(b); }

    function eventStart(e) {
        return e.isAllDay ? new Date(e.date + 'T00:00:00') : new Date(e.startAt);
    }

    function eventEnd(e) {
        return e.isAllDay ? addDays(eventStart(e), 1) : new Date(e.endAt);
    }

    function eventsOn(day) {
        return events
            .filter(e => sameDay(eventStart(e), day))
            .sort((a, b) => eventStart(a) - eventStart(b));
    }

    function range() {
        if (view === 'month') {
            const first = new Date(cursor.getFullYear(), cursor.getMonth(), 1);
            const start = startOfWeek(first);
            return { start, end: addDays(start, 42) };
        }
        if (view === 'week') {
            const start = startOfWeek(cursor);
            return { start, end: addDays(start, 7) };
        }
        const start = startOfDay(cursor);
        return { start, end: addDays(start, 1) };
    }

    function label() {
        if (view === 'month') return `${monthName(cursor.getMonth())} ${cursor.getFullYear()}`;
        if (view === 'week') {
            const s = startOfWeek(cursor), e = addDays(s, 6);
            return `${s.getDate()} ${monthName(s.getMonth()).slice(0, 3)} – ${e.getDate()} ${monthName(e.getMonth()).slice(0, 3)} ${e.getFullYear()}`;
        }
        return `${cursor.getDate()} ${monthName(cursor.getMonth())} ${cursor.getFullYear()}`;
    }

    function chipStyle(color, homework) {
        const base = homework ? HOMEWORK_COLOR : (COLORS[color] || COLORS.default);
        return `background:${base};border-left-color:${homework ? '#b45309' : '#00000022'}`;
    }

    // ---------------------------------------------------------------- month
    function renderMonth() {
        const { start } = range();
        const today = new Date();
        const wd = weekdayNames();

        let cells = '';
        for (let i = 0; i < 42; i++) {
            const day = addDays(start, i);
            const list = eventsOn(day);
            const muted = day.getMonth() !== cursor.getMonth();
            const isToday = sameDay(day, today);

            const shown = list.slice(0, 3).map(e => {
                const time = e.isAllDay ? '' : `<span class="mr-1 font-bold">${hm(eventStart(e))}</span>`;
                const action = e.id
                    ? `data-action="Calendar.openEdit" data-args='[${e.id}]'`
                    : (e.url ? `data-action="Calendar.openUrl" data-args='["${e.url}"]'` : '');
                return `<button type="button" ${action}
                            class="mb-0.5 block w-full truncate rounded-md border-l-[3px] px-1.5 py-0.5 text-left text-[11px] font-semibold text-slate-800 hover:opacity-80"
                            style="${chipStyle(e.color, e.type === 'homework')}">${time}${AdminUI.escapeHtml(e.title)}</button>`;
            }).join('');

            const more = list.length > 3
                ? `<button type="button" class="mt-0.5 text-[11px] font-bold text-slate-500 hover:underline"
                       data-action="Calendar.more" data-args='["${ymd(day)}"]'>${L["Calendar_More"].replace('{0}', list.length - 3)}</button>`
                : '';

            cells += `
                <div class="cal-month-cell flex min-h-[108px] flex-col border-b border-r border-border p-1.5 ${muted ? 'bg-slate-50/60' : ''} ${isToday ? 'bg-primary-light/40' : ''}"
                     data-day="${ymd(day)}">
                    <div class="mb-1 flex items-center justify-between">
                        <span class="grid h-6 w-6 place-items-center rounded-full text-xs font-bold ${isToday ? 'bg-primary text-white' : (muted ? 'text-slate-300' : 'text-slate-600')}">${day.getDate()}</span>
                    </div>
                    ${shown}
                    ${more}
                </div>`;
        }

        el('calendarGrid').innerHTML = `
            <div class="min-w-[720px]">
                <div class="grid grid-cols-7 border-b border-border bg-surface-secondary">
                    ${wd.map(w => `<div class="px-2 py-2 text-center text-[10px] font-bold uppercase tracking-wide text-slate-400">${AdminUI.escapeHtml(w)}</div>`).join('')}
                </div>
                <div class="grid grid-cols-7">${cells}</div>
            </div>`;

        el('calendarGrid').querySelectorAll('.cal-month-cell').forEach(cell => {
            cell.addEventListener('click', (e) => {
                if (e.target.closest('button')) return;
                Calendar.openCreate(cell.dataset.day, null);
            });
        });
    }

    // ---------------------------------------------------------------- time grid
    function renderTimeGrid(days) {
        const today = new Date();
        const hours = Array.from({ length: 24 }, (_, h) => h);

        const gutter = `<div class="w-14 shrink-0">
            ${hours.map(h => `<div class="relative" style="height:${HOUR_HEIGHT}px">
                <span class="absolute -top-2 right-2 text-[10px] font-semibold text-slate-400">${pad(h)}:00</span>
            </div>`).join('')}
        </div>`;

        const columns = days.map(day => {
            const isToday = sameDay(day, today);
            const list = eventsOn(day);

            const items = list.map(e => {
                const start = eventStart(e);
                const end = eventEnd(e);
                const top = e.isAllDay ? 0 : (start.getHours() * 60 + start.getMinutes()) / 60 * HOUR_HEIGHT;
                const height = e.isAllDay ? 24 : Math.max(24, (end - start) / 60000 / 60 * HOUR_HEIGHT);
                const action = e.id
                    ? `data-action="Calendar.openEdit" data-args='[${e.id}]'`
                    : (e.url ? `data-action="Calendar.openUrl" data-args='["${e.url}"]'` : '');
                const time = e.isAllDay ? '' : `<div class="text-[10px] font-bold opacity-70">${hm(start)} – ${hm(end)}</div>`;

                return `<button type="button" ${action}
                            class="absolute left-1 right-1 overflow-hidden rounded-lg border-l-[3px] px-1.5 py-1 text-left text-[11px] font-semibold text-slate-800 shadow-sm hover:opacity-90"
                            style="top:${top}px;height:${height}px;${chipStyle(e.color, e.type === 'homework')}">
                            ${time}<div class="truncate">${AdminUI.escapeHtml(e.title)}</div>
                        </button>`;
            }).join('');

            return `<div class="cal-day-col relative flex-1 border-r border-border ${isToday ? 'bg-primary-light/30' : ''}" data-date="${ymd(day)}">
                <div class="absolute inset-0 pointer-events-none"
                     style="background-image:repeating-linear-gradient(to bottom, #eef2f6 0 1px, transparent 1px ${HOUR_HEIGHT}px)"></div>
                ${items}
            </div>`;
        }).join('');

        el('calendarGrid').innerHTML = `
            <div class="min-w-[760px]">
                <div class="flex border-b border-border bg-surface-secondary">
                    <div class="w-14 shrink-0"></div>
                    ${days.map(d => {
                        const isToday = sameDay(d, today);
                        return `<div class="flex-1 border-r border-border px-2 py-2 text-center">
                            <p class="text-[10px] font-bold uppercase tracking-wide text-slate-400">${AdminUI.escapeHtml(weekdayNames()[(d.getDay() + 6) % 7])}</p>
                            <p class="text-sm font-bold ${isToday ? 'text-primary' : 'text-slate-700'}">${d.getDate()}</p>
                        </div>`;
                    }).join('')}
                </div>
                <div class="flex" style="height:${24 * HOUR_HEIGHT}px">
                    ${gutter}${columns}
                </div>
            </div>`;

        el('calendarGrid').querySelectorAll('.cal-day-col').forEach(col => {
            col.addEventListener('click', (e) => {
                if (e.target.closest('button')) return;
                const rect = col.getBoundingClientRect();
                const minutes = Math.max(0, Math.round((e.clientY - rect.top) / HOUR_HEIGHT * 60 / 30) * 30);
                const h = Math.min(23, Math.floor(minutes / 60));
                const m = minutes % 60 === 60 ? 0 : minutes % 60;
                Calendar.openCreate(col.dataset.date, `${pad(h)}:${pad(m)}`);
            });
        });
    }

    function render() {
        el('calLabel').textContent = label();
        el('calRange').textContent = '';

        document.querySelectorAll('.cal-view-btn').forEach(btn => {
            const active = btn.dataset.view === view;
            btn.classList.toggle('bg-primary', active);
            btn.classList.toggle('text-white', active);
            btn.classList.toggle('text-slate-600', !active);
            btn.classList.toggle('hover:bg-slate-100', !active);
        });

        if (view === 'month') renderMonth();
        else if (view === 'week') {
            const start = startOfWeek(cursor);
            renderTimeGrid(Array.from({ length: 7 }, (_, i) => addDays(start, i)));
        } else {
            renderTimeGrid([startOfDay(cursor)]);
        }
    }

    async function load() {
        const { start, end } = range();
        events = await AdminUI.get(
            `/calendar/events?from=${encodeURIComponent(start.toISOString())}&to=${encodeURIComponent(end.toISOString())}`) || [];
        render();
    }

    // ---------------------------------------------------------------- editor
    function renderPalette() {
        el('eventPalette').innerHTML = Object.keys(COLORS).map(color => {
            const ring = color === draftColor ? ' ring-2 ring-primary ring-offset-2' : ' hover:scale-110';
            return `<button type="button" class="h-6 w-6 rounded-full border border-black/10 transition${ring}"
                        style="background:${COLORS[color]}" data-action="Calendar.setDraftColor" data-args='["${color}"]'
                        aria-label="${color}" aria-pressed="${color === draftColor}"></button>`;
        }).join('');
    }

    function setDraftColor(color) {
        draftColor = COLORS[color] ? color : 'default';
        renderPalette();
    }

    function openCreate(date, time) {
        const day = date || ymd(cursor);

        let startHour = new Date().getHours();
        let startMinute = 0;
        if (time) { const [h, m] = time.split(':').map(Number); startHour = h; startMinute = m; }

        el('eventModalTitle').textContent = L["Calendar_NewEvent"];
        el('eventId').value = '';
        el('eventTitle').value = '';
        el('eventDescription').value = '';
        el('eventDate').value = day;
        el('eventStart').value = `${pad(startHour)}:${pad(startMinute)}`;
        el('eventEnd').value = `${pad(Math.min(23, startHour + 1))}:${pad(startMinute)}`;
        el('eventDelete').classList.add('hidden');
        el('eventError').classList.add('hidden');

        setDraftColor('green');
        AdminUI.openModal('eventModal');
    }

    function openEdit(id) {
        const e = events.find(x => x.id === id);
        if (!e) return;

        const start = new Date(e.startAt);
        const end = new Date(e.endAt);

        el('eventModalTitle').textContent = L["Calendar_EditEvent"];
        el('eventId').value = e.id;
        el('eventTitle').value = e.title;
        el('eventDescription').value = e.subtitle || '';
        el('eventDate').value = ymd(start);
        el('eventStart').value = hm(start);
        el('eventEnd').value = hm(end);
        el('eventDelete').classList.remove('hidden');
        el('eventError').classList.add('hidden');

        setDraftColor(e.color || 'default');
        AdminUI.openModal('eventModal');
    }

    function openUrl(url) {
        if (url) location.href = url;
    }

    function more(date) {
        const [y, m, d] = date.split('-').map(Number);
        cursor = new Date(y, m - 1, d);
        view = 'day';
        load();
    }

    async function deleteEvent() {
        const id = el('eventId').value;
        if (!id) return;
        if (!await AdminUI.confirmDelete(L["Js_AreYouSure"])) return;

        const res = await AdminUI.del(`/calendar/events/${id}`);
        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

        AdminUI.notify(L["Js_Deleted"]);
        AdminUI.closeModal('eventModal');
        await load();
    }

    function combine(date, time) {
        const [y, m, d] = date.split('-').map(Number);
        const [h, min] = time.split(':').map(Number);
        return new Date(y, m - 1, d, h, min, 0, 0);
    }

    const Calendar = {
        openCreate, openEdit, openUrl, more, deleteEvent, setDraftColor,

        previous() {
            if (view === 'month') cursor = new Date(cursor.getFullYear(), cursor.getMonth() - 1, 1);
            else if (view === 'week') cursor = addDays(cursor, -7);
            else cursor = addDays(cursor, -1);
            load();
        },

        next() {
            if (view === 'month') cursor = new Date(cursor.getFullYear(), cursor.getMonth() + 1, 1);
            else if (view === 'week') cursor = addDays(cursor, 7);
            else cursor = addDays(cursor, 1);
            load();
        },

        today() {
            cursor = startOfDay(new Date());
            load();
        },

        setView(name) {
            view = name;
            load();
        }
    };

    window.Calendar = Calendar;

    document.addEventListener('DOMContentLoaded', () => {
        renderPalette();

        el('calPrev').addEventListener('click', () => Calendar.previous());
        el('calNext').addEventListener('click', () => Calendar.next());
        el('calToday').addEventListener('click', () => Calendar.today());
        el('calNew').addEventListener('click', () => openCreate(ymd(cursor), null));

        document.querySelectorAll('.cal-view-btn').forEach(btn => {
            btn.addEventListener('click', () => Calendar.setView(btn.dataset.view));
        });

        el('eventForm').addEventListener('submit', async (e) => {
            e.preventDefault();

            const date = el('eventDate').value;
            const start = el('eventStart').value;
            const end = el('eventEnd').value;
            const title = el('eventTitle').value.trim();
            const box = el('eventError');

            const fail = (msg) => { box.textContent = msg; box.classList.remove('hidden'); };

            if (!title || !date || !start || !end) return fail(L["Common_RequiredFields"]);

            const startAt = combine(date, start);
            const endAt = combine(date, end);
            if (endAt <= startAt) return fail(L["Calendar_InvalidRange"]);

            const payload = {
                title,
                description: el('eventDescription').value,
                startAt: startAt.toISOString(),
                endAt: endAt.toISOString(),
                color: draftColor
            };

            const id = el('eventId').value;
            const res = id
                ? await AdminUI.putJson(`/calendar/events/${id}`, payload)
                : await AdminUI.postJson('/calendar/events', payload);

            if (!res.ok) {
                AdminUI.error(AdminUI.errText(res));
                return;
            }

            AdminUI.notify(id ? L["Js_Updated"] : L["Js_Added"]);
            AdminUI.closeModal('eventModal');
            await load();
        });

        load();
    });
})();
