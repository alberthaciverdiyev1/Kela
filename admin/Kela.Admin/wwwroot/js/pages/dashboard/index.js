// Dashboard qrafikləri — xarici kitabxana yoxdur, SVG-ni özümüz qururuq.
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
                        <span class="text-slate-500">${i.label}</span>
                        <span class="ml-auto font-bold text-slate-800">${i.value}</span>
                    </div>`).join('')}
            </div>`;
    }

    function bars(el, items) {
        const max = Math.max(1, ...items.map(i => i.value));
        el.innerHTML = items.map(i => `
            <div class="admin-bar-row">
                <span class="min-w-0 flex-1 truncate text-sm text-slate-600">${i.label}</span>
                <span class="w-full max-w-[45%]">
                    <span class="admin-bar-track">
                        <span class="admin-bar-fill" style="width:${Math.round((i.value / max) * 100)}%;background:${color(i.tone)}"></span>
                    </span>
                </span>
                <span class="w-10 shrink-0 text-right text-sm font-bold text-slate-800">${i.value}</span>
            </div>`).join('');
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('[data-chart]').forEach(el => {
            let items = [];
            try { items = JSON.parse(el.dataset.chart); } catch (e) { return; }
            if (el.dataset.chartKind === 'donut') donut(el, items); else bars(el, items);
        });
    });
})();
