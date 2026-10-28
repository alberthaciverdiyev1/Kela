// Dashboard qrafikləri — xarici kitabxana yoxdur, SVG/div-ləri özümüz qururuq.
// Razor hər qrafik üçün data-chart="[{label,value,tone}]" yazır.
(function () {
    const TONES = {
        primary: '#00b884',
        info: '#0ea5e9',
        violet: '#8b5cf6',
        warning: '#f59e0b',
        danger: '#ef4444',
        slate: '#cbd5e1'
    };

    const color = (tone) => TONES[tone] || TONES.primary;
    const esc = (value) => AdminUI.escapeHtml(String(value ?? ''));

    function donut(el, items) {
        const total = items.reduce((s, i) => s + i.value, 0);
        const size = 176, stroke = 22, radius = (size - stroke) / 2;
        const circumference = 2 * Math.PI * radius;

        let offset = 0;
        const arcs = items.map(i => {
            const ratio = total ? i.value / total : 0;
            const len = circumference * ratio;
            const arc = `<circle cx="${size / 2}" cy="${size / 2}" r="${radius}" fill="none"
                        stroke="${color(i.tone)}" stroke-width="${stroke}"
                        stroke-dasharray="${len} ${circumference - len}"
                        stroke-dashoffset="${-offset}" transform="rotate(-90 ${size / 2} ${size / 2})"
                        stroke-linecap="butt" />`;
            offset += len;
            return arc;
        }).join('');

        el.innerHTML = `
            <div class="admin-donut">
                <svg viewBox="0 0 ${size} ${size}" width="${size}" height="${size}">
                    <circle cx="${size / 2}" cy="${size / 2}" r="${radius}" fill="none" stroke="#eef2f7" stroke-width="${stroke}" />
                    ${arcs}
                </svg>
                <div class="admin-donut-center">
                    <span class="text-2xl font-extrabold text-slate-950">${total}</span>
                </div>
            </div>
            <div class="space-y-2">
                ${items.map(i => `
                    <div class="flex items-center gap-2 text-sm">
                        <span class="h-2.5 w-2.5 shrink-0 rounded-full" style="background:${color(i.tone)}"></span>
                        <span class="text-slate-500">${esc(i.label)}</span>
                        <span class="ml-auto font-bold text-slate-800">${i.value}</span>
                    </div>`).join('')}
            </div>`;
    }

    function bars(el, items) {
        const max = Math.max(1, ...items.map(i => i.value));
        el.innerHTML = items.map(i => `
            <div class="admin-bar-row">
                <span class="min-w-0 flex-1 truncate text-sm text-slate-600">${esc(i.label)}</span>
                <span class="w-full max-w-[45%]">
                    <span class="admin-bar-track">
                        <span class="admin-bar-fill" style="width:${Math.round((i.value / max) * 100)}%;background:${color(i.tone)}"></span>
                    </span>
                </span>
                <span class="w-10 shrink-0 text-right text-sm font-bold text-slate-800">${i.value}</span>
            </div>`).join('');
    }

    function columns(el, items) {
        const max = Math.max(1, ...items.map(i => i.value));
        el.innerHTML = items.map(i => `
            <div class="dash-col" title="${esc(i.title || i.label)}">
                <span class="dash-col-track">
                    <span class="dash-col-fill" style="height:${i.value > 0 ? Math.max(3, Math.round((i.value / max) * 100)) : 0}%"></span>
                </span>
                <span class="dash-col-label">${esc(i.label)}</span>
            </div>`).join('');
    }

    const money = (value) => Number(value || 0).toLocaleString('az', {
        minimumFractionDigits: 2, maximumFractionDigits: 2
    });

    function monthLabel(period) {
        const lang = document.documentElement.lang || 'az';
        const date = new Date(`${period}-01T00:00:00`);
        return date.toLocaleDateString(lang, { month: 'short' });
    }

    function initFinance() {
        const data = window.DASH_FINANCE;
        const chart = document.getElementById('dashFinanceChart');
        if (!data || !chart || !data.currencies.length) return;

        const select = document.getElementById('dashCurrency');
        const month = document.getElementById('dashMonth');
        const delta = document.getElementById('dashDelta');

        function render(code) {
            const current = data.currencies.find(c => c.currency === code) || data.currencies[0];

            month.textContent = `${money(current.thisMonth)} ${current.currency}`;

            if (current.lastMonth > 0) {
                const percent = Math.round((current.thisMonth - current.lastMonth) / current.lastMonth * 100);
                const sign = percent > 0 ? '+' : '';
                delta.textContent = L['Dashboard_VsLastMonth'].replace('{0}', `${sign}${percent}`);
                delta.className = 'mt-1 text-xs font-semibold ' +
                    (percent > 0 ? 'text-primary' : percent < 0 ? 'text-danger' : 'text-slate-400');
            } else {
                delta.textContent = '';
                delta.className = 'mt-1 text-xs font-semibold text-slate-400';
            }

            const set = (id, value) => {
                const el = document.getElementById(id);
                if (el) el.textContent = `${money(value)} ${current.currency}`;
            };
            set('dashToday', current.today);
            set('dashExpected', current.expected);
            set('dashOverdue', current.overdue);
            set('dashProjected', current.projected);

            const months = data.monthly.filter(m => m.currency === current.currency);
            columns(chart, months.map(m => ({
                label: monthLabel(m.period),
                value: m.amount,
                title: `${m.period} · ${money(m.amount)} ${current.currency}`
            })));
        }

        if (select) select.addEventListener('change', () => render(select.value));
        render(select ? select.value : data.currencies[0].currency);
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('[data-chart]').forEach(el => {
            let items = [];
            try { items = JSON.parse(el.dataset.chart); } catch (e) { return; }
            if (el.dataset.chartKind === 'donut') donut(el, items); else bars(el, items);
        });

        initFinance();
    });
})();
