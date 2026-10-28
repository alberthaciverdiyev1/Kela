// Kela admin panel — tarix seçici komponenti.
// Sahədə gizli <input type="date|datetime-local"> qalır (dəyər formatı dəyişmir,
// mövcud JS onu oxuyub yazır); görünən hissə və təqvim burada qurulur.
(function () {
    const lang = (document.documentElement.lang || 'az').slice(0, 2);
    const pad = n => String(n).padStart(2, '0');
    const T = key => (window.L && window.L[key]) || '';

    const monthName = new Intl.DateTimeFormat(lang, { month: 'long' });
    const monthNameShort = new Intl.DateTimeFormat(lang, { month: 'short' });
    const weekdayName = new Intl.DateTimeFormat(lang, { weekday: 'short' });

    // 2024-01-01 bazar ertəsidir — həftə həmişə bazar ertəsindən başlayır.
    const weekdays = Array.from({ length: 7 },
        (_, i) => weekdayName.format(new Date(2024, 0, 1 + i)));

    let pop = null;
    const state = {
        field: null,
        input: null,
        mode: 'date',
        month: null,
        selected: null,
        hours: 0,
        minutes: 0,
        panel: 'days'
    };

    function parseValue(value) {
        const m = /^(\d{4})-(\d{2})-(\d{2})(?:[T ](\d{2}):(\d{2}))?/.exec(value || '');
        if (!m) return null;
        return new Date(+m[1], +m[2] - 1, +m[3], m[4] ? +m[4] : 0, m[5] ? +m[5] : 0);
    }

    function formatText(d, mode) {
        const date = `${pad(d.getDate())}.${pad(d.getMonth() + 1)}.${d.getFullYear()}`;
        return mode === 'datetime-local'
            ? `${date} ${pad(d.getHours())}:${pad(d.getMinutes())}`
            : date;
    }

    function formatValue(d) {
        const date = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
        return state.mode === 'datetime-local'
            ? `${date}T${pad(state.hours)}:${pad(state.minutes)}`
            : date;
    }

    function sameDay(a, b) {
        return !!a && !!b
            && a.getFullYear() === b.getFullYear()
            && a.getMonth() === b.getMonth()
            && a.getDate() === b.getDate();
    }

    function sync(input) {
        const field = input.closest('[data-datepicker]');
        if (!field) return;
        const text = field.querySelector('[data-dp-text]');
        const mode = field.dataset.mode || 'date';
        const value = parseValue(input.value);
        const shown = value ? formatText(value, mode) : '';
        text.textContent = shown || field.dataset.placeholder || '';
        text.classList.toggle('is-empty', !shown);
        field.setAttribute('aria-label', shown
            ? `${field.dataset.placeholder}: ${shown}`
            : field.dataset.placeholder || '');
    }

    function syncAll(root) {
        (root || document).querySelectorAll('[data-datepicker] input[data-dp-input]').forEach(sync);
    }

    function setValue(input, value) {
        input.value = value;
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new Event('change', { bubbles: true }));
        sync(input);
    }

    function ensurePop() {
        if (pop) return pop;
        pop = document.createElement('div');
        pop.className = 'dp-pop';
        pop.setAttribute('role', 'dialog');
        pop.hidden = true;
        pop.addEventListener('click', onPopClick);
        pop.addEventListener('change', onPopChange);
        document.body.appendChild(pop);
        return pop;
    }

    function isOpen() {
        return pop && !pop.hidden;
    }

    function open(field) {
        const input = field.querySelector('input[data-dp-input]');
        if (!input) return;

        const node = ensurePop();
        sync(input);

        state.field = field;
        state.input = input;
        state.mode = field.dataset.mode || 'date';
        state.panel = 'days';

        const current = parseValue(input.value);
        const base = current || new Date();
        state.selected = current;
        state.month = new Date(base.getFullYear(), base.getMonth(), 1);

        if (current) {
            state.hours = current.getHours();
            state.minutes = current.getMinutes();
        } else {
            state.hours = state.mode === 'datetime-local' ? 9 : 0;
            state.minutes = 0;
        }

        field.classList.add('is-open');
        node.hidden = false;
        render();
        position();

        const focusTarget = node.querySelector('.dp-day.is-selected')
            || node.querySelector('.dp-day.is-today')
            || node.querySelector('.dp-title');
        if (focusTarget) focusTarget.focus({ preventScroll: true });
    }

    function close(returnFocus) {
        if (!isOpen()) return;
        const field = state.field;
        pop.hidden = true;
        pop.classList.remove('is-open');
        if (field) {
            field.classList.remove('is-open');
            if (returnFocus) field.focus({ preventScroll: true });
        }
        state.field = null;
        state.input = null;
    }

    function position() {
        if (!isOpen() || !state.field) return;
        const rect = state.field.getBoundingClientRect();
        const width = pop.offsetWidth;
        const height = pop.offsetHeight;

        let left = Math.min(Math.max(8, rect.left), window.innerWidth - width - 8);
        let top = rect.bottom + 6;
        if (top + height > window.innerHeight - 8) {
            top = Math.max(8, rect.top - height - 6);
        }

        pop.style.left = `${Math.round(left)}px`;
        pop.style.top = `${Math.round(top)}px`;
    }

    function render() {
        const month = state.month;
        const today = new Date();

        const head = `
            <div class="dp-head">
                <button type="button" class="dp-nav" data-dp-action="prev" aria-label="${T('DatePicker_PrevMonth')}">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="m15 6-6 6 6 6"/></svg>
                </button>
                <button type="button" class="dp-title" data-dp-action="${state.panel === 'months' ? 'days' : 'months'}">
                    ${state.panel === 'months' ? month.getFullYear() : `${monthName.format(month)} ${month.getFullYear()}`}
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="m6 9 6 6 6-6"/></svg>
                </button>
                <button type="button" class="dp-nav" data-dp-action="next" aria-label="${T('DatePicker_NextMonth')}">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="m9 6 6 6-6 6"/></svg>
                </button>
            </div>`;

        let body;
        if (state.panel === 'months') {
            body = `<div class="dp-months">${Array.from({ length: 12 }, (_, i) => {
                const active = state.selected
                    && state.selected.getFullYear() === month.getFullYear()
                    && state.selected.getMonth() === i;
                return `<button type="button" class="dp-month${active ? ' is-selected' : ''}"
                    data-dp-action="pick-month" data-month="${i}">${monthNameShort.format(new Date(2024, i, 1))}</button>`;
            }).join('')}</div>`;
        } else {
            const first = new Date(month.getFullYear(), month.getMonth(), 1);
            const offset = (first.getDay() + 6) % 7; // bazar ertəsi = 0
            const start = new Date(first);
            start.setDate(first.getDate() - offset);

            const days = Array.from({ length: 42 }, (_, i) => {
                const d = new Date(start);
                d.setDate(start.getDate() + i);
                const classes = ['dp-day'];
                if (d.getMonth() !== month.getMonth()) classes.push('is-out');
                if (sameDay(d, today)) classes.push('is-today');
                if (sameDay(d, state.selected)) classes.push('is-selected');
                return `<button type="button" class="${classes.join(' ')}"
                    data-dp-day data-y="${d.getFullYear()}" data-m="${d.getMonth()}" data-d="${d.getDate()}"
                    >${d.getDate()}</button>`;
            }).join('');

            body = `
                <div class="dp-week">${weekdays.map(w => `<span>${w}</span>`).join('')}</div>
                <div class="dp-grid">${days}</div>`;

            if (state.mode === 'datetime-local') body += timeRow();
        }

        const foot = `
            <div class="dp-foot">
                <button type="button" class="dp-link" data-dp-action="clear">${T('DatePicker_Clear')}</button>
                <div class="dp-foot-actions">
                    <button type="button" class="dp-btn-ghost" data-dp-action="today">${T('DatePicker_Today')}</button>
                    ${state.mode === 'datetime-local'
                        ? `<button type="button" class="dp-btn" data-dp-action="apply">${T('DatePicker_Apply')}</button>`
                        : ''}
                </div>
            </div>`;

        pop.innerHTML = head + body + foot;
    }

    function timeRow() {
        const hours = Array.from({ length: 24 }, (_, h) =>
            `<option value="${h}"${h === state.hours ? ' selected' : ''}>${pad(h)}</option>`).join('');
        const minutes = Array.from({ length: 60 }, (_, m) =>
            `<option value="${m}"${m === state.minutes ? ' selected' : ''}>${pad(m)}</option>`).join('');

        return `
            <div class="dp-time">
                <span class="dp-time-label">${T('DatePicker_Time')}</span>
                <select data-dp-time="h" aria-label="${T('DatePicker_Time')}">${hours}</select>
                <span class="dp-time-sep">:</span>
                <select data-dp-time="m" aria-label="${T('DatePicker_Time')}">${minutes}</select>
            </div>`;
    }

    function commit(closeAfter) {
        if (!state.selected || !state.input) return;
        setValue(state.input, formatValue(state.selected));
        if (closeAfter) close(true);
    }

    function onPopClick(event) {
        const action = event.target.closest('[data-dp-action]');
        if (action) {
            handleAction(action.dataset.dpAction, action);
            return;
        }

        const day = event.target.closest('[data-dp-day]');
        if (!day) return;

        const picked = new Date(+day.dataset.y, +day.dataset.m, +day.dataset.d);
        state.selected = picked;
        state.month = new Date(picked.getFullYear(), picked.getMonth(), 1);

        if (state.mode === 'datetime-local') {
            commit(false);
            render();
            position();
        } else {
            commit(true);
        }
    }

    function onPopChange(event) {
        if (!event.target.matches('[data-dp-time]')) return;
        const value = Number(event.target.value);
        if (event.target.dataset.dpTime === 'h') state.hours = value;
        else state.minutes = value;

        if (state.selected) {
            commit(false);
            // Seçilmiş gün dəyişməz — yalnız saat yenilənir.
            const selected = pop.querySelector('.dp-day.is-selected');
            if (selected) selected.focus({ preventScroll: true });
        }
    }

    function handleAction(action, element) {
        const month = state.month;

        switch (action) {
            case 'prev':
                state.month = state.panel === 'months'
                    ? new Date(month.getFullYear() - 1, month.getMonth(), 1)
                    : new Date(month.getFullYear(), month.getMonth() - 1, 1);
                render();
                position();
                break;
            case 'next':
                state.month = state.panel === 'months'
                    ? new Date(month.getFullYear() + 1, month.getMonth(), 1)
                    : new Date(month.getFullYear(), month.getMonth() + 1, 1);
                render();
                position();
                break;
            case 'months':
                state.panel = 'months';
                render();
                position();
                break;
            case 'days':
                state.panel = 'days';
                render();
                position();
                break;
            case 'pick-month':
                state.month = new Date(month.getFullYear(), Number(element.dataset.month), 1);
                state.panel = 'days';
                render();
                position();
                break;
            case 'today': {
                const now = new Date();
                state.selected = now;
                if (state.mode === 'datetime-local') {
                    state.hours = now.getHours();
                    state.minutes = now.getMinutes();
                }
                state.month = new Date(now.getFullYear(), now.getMonth(), 1);
                commit(state.mode !== 'datetime-local');
                if (state.mode === 'datetime-local') {
                    render();
                    position();
                }
                break;
            }
            case 'clear':
                setValue(state.input, '');
                close(true);
                break;
            case 'apply':
                commit(true);
                break;
        }
    }

    // --- Qlobal hadisələr ---------------------------------------------------

    document.addEventListener('click', event => {
        const field = event.target.closest('[data-datepicker]');
        if (field) {
            event.preventDefault();
            if (isOpen() && state.field === field) close(true);
            else open(field);
            return;
        }

        // render() hadisə zamanı popup-ı yenidən qurur — hədəf düymə DOM-dan ayrılır,
        // ona görə "içəridə klik" yoxlaması composedPath ilə aparılır.
        if (event.composedPath().includes(pop)) return;
        if (isOpen()) close(false);
    });

    // Capture fazasında: Escape əvvəl təqvimi bağlasın, modalı yox.
    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && isOpen()) {
            event.preventDefault();
            event.stopPropagation();
            close(true);
            return;
        }

        const field = event.target.closest && event.target.closest('[data-datepicker]');
        if (field && !isOpen() && (event.key === 'Enter' || event.key === ' ' || event.key === 'ArrowDown')) {
            event.preventDefault();
            open(field);
        }
    }, true);

    document.addEventListener('input', event => {
        if (event.target.matches('input[data-dp-input]')) sync(event.target);
    });

    document.addEventListener('change', event => {
        if (event.target.matches('input[data-dp-input]')) sync(event.target);
    });

    // Forma sıfırlananda (modal bağlanır) göstəriş də sıfırlanmalıdır.
    document.addEventListener('reset', event => {
        if (event.target.matches('form')) {
            queueMicrotask(() => syncAll(event.target));
        }
    }, true);

    // Modal JS ilə doldurulur, sonra açılır — açılışda göstərişlər təzələnir.
    document.addEventListener('modal:open', event => syncAll(event.target));
    document.addEventListener('modal:closed', () => close(false));

    const reposition = () => { if (isOpen()) position(); };
    window.addEventListener('resize', reposition);
    window.addEventListener('scroll', reposition, true);

    document.addEventListener('DOMContentLoaded', () => syncAll(document));

    window.DatePicker = {
        open: field => open(field),
        sync: input => sync(input),
        syncAll: root => syncAll(root),
        set: (input, value) => setValue(input, value),
        close: () => close(false)
    };
})();
