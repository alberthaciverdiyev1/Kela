window.PlatformFinancePage = (function () {
    const data = window.PLATFORM_FINANCE || { currencies: [], monthly: [], stats: {}, i18n: {} };
    const i18n = data.i18n || {};
    const money = (value) => Number(value || 0).toLocaleString('az', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const amountText = (value, code) => `${money(value)} ${code}`;

    let currency = data.currencies[0]?.currency || 'AZN';

    const text = (id, value) => {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    };

    const statsFor = (code) => data.currencies.find(c => c.currency === code) || {};

    function renderStats() {
        const s = statsFor(currency);
        const stats = data.stats || {};

        text('pf-mrr', amountText(s.mrr, currency));
        text('pf-arr', amountText(s.arr, currency));
        text('pf-this', amountText(s.thisMonth, currency));
        text('pf-last', amountText(s.lastMonth, currency));
        text('pf-addons', amountText(s.addons, currency));
        text('pf-alltime', amountText(s.allTime, currency));
        text('pf-paying', s.paying ?? 0);
        text('pf-active', stats.active ?? 0);
        text('pf-pending', stats.pending ?? 0);
        text('pf-total', stats.total ?? 0);

        const delta = document.getElementById('pfDelta');
        if (delta) {
            if (!s.lastMonth) {
                delta.textContent = s.thisMonth ? (i18n.noLastMonth || '') : '';
                delta.className = 'text-sm font-semibold text-slate-500';
            } else {
                const pct = Math.round((s.thisMonth - s.lastMonth) * 100 / s.lastMonth);
                const signed = `${pct > 0 ? '+' : ''}${pct}`;
                delta.textContent = (i18n.growth || '{0}%').replace('{0}', signed);
                delta.className = `text-sm font-semibold ${pct >= 0 ? 'text-primary' : 'text-danger'}`;
            }
        }
    }

    function renderChart() {
        const box = document.getElementById('pfChart');
        const empty = document.getElementById('pfChartEmpty');
        if (!box) return;

        const rows = data.monthly.filter(m => m.currency === currency);
        const max = Math.max(...rows.map(r => r.total), 1);
        empty?.classList.toggle('hidden', rows.some(r => r.total > 0));

        box.innerHTML = rows.map(r => {
            const height = Math.max(2, Math.round(r.total * 100 / max));
            return `<div class="flex h-full flex-1 flex-col items-center justify-end gap-1" title="${r.period}: ${amountText(r.total, currency)}">
                <div class="w-full rounded-t-md ${r.total > 0 ? 'bg-primary' : 'bg-slate-200'}" style="height:${height}%"></div>
                <span class="text-[10px] font-medium text-slate-400">${r.period.slice(5)}</span>
            </div>`;
        }).join('');
    }

    function render() {
        renderStats();
        renderChart();
    }

    function init() {
        const select = document.getElementById('pfCurrency');
        if (select) {
            select.value = currency;
            select.addEventListener('change', () => {
                currency = select.value;
                render();
            });
        }

        render();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    return { init };
})();
