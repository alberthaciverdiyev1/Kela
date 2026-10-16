// Konversiya cihazları: siyahı, işə salma/söndürmə, token ləğvi.
window.NodesPage = (function () {
    const statusBadge = (status) => {
        switch (status) {
            case 'online': return AdminUI.badge(L["Node_Online"], 'success');
            case 'offline': return AdminUI.badge(L["Node_Offline"], 'neutral');
            default: return AdminUI.badge(L["Node_Disabled"], 'warning');
        }
    };

    const secondsAgo = (value) => {
        if (!value) return '—';
        const diff = Math.floor((Date.now() - new Date(value).getTime()) / 1000);
        if (diff < 60) return `${diff}s`;
        if (diff < 3600) return `${Math.floor(diff / 60)}m`;
        if (diff < 86400) return `${Math.floor(diff / 3600)}h`;
        return `${Math.floor(diff / 86400)}d`;
    };

    function tab(name) {
        document.querySelectorAll('.node-tab').forEach(btn => {
            const active = btn.dataset.tab === name;
            btn.classList.toggle('bg-primary', active);
            btn.classList.toggle('text-white', active);
            btn.classList.toggle('text-slate-600', !active);
            btn.classList.toggle('hover:bg-surface-secondary', !active);
        });

        document.getElementById('tab-devices').classList.toggle('hidden', name !== 'devices');
        document.getElementById('tab-jobs').classList.toggle('hidden', name !== 'jobs');
        document.getElementById('tab-codes').classList.toggle('hidden', name !== 'codes');

        const createBtn = document.getElementById('createCodeBtn');
        const isCodes = name === 'codes';
        createBtn.classList.toggle('hidden', !isCodes);
        document.getElementById('refreshNote').classList.toggle('hidden', !isCodes);

        if (name === 'jobs') JobsPage.reload();
        if (name === 'codes') CodesPage.reload();
    }

    async function load() {
        AdminUI.loader(true);
        const data = await AdminUI.get('/node/list?perPage=500');

        AdminUI.table('rows', (data.items || []).map(node => {
            const device = node.device || {};
            const benchmark = node.benchmark;
            const encoders = (device.encoders || []).join(', ') || '—';
            const platform = [device.operatingSystem, device.cpuCores ? `${device.cpuCores} CPU` : null,
                device.totalRamMb ? `${Math.round(device.totalRamMb / 1024)} GB` : null]
                .filter(Boolean).join(' · ');

            const actions = [
                `<button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-slate-600 hover:bg-surface-secondary" onclick="NodesPage.toggle(${node.id}, ${!node.enabled})">${node.enabled ? L["Node_Disable"] : L["Node_Enable"]}</button>`,
                `<button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="NodesPage.revoke(${node.id})">${L["Node_Revoke"]}</button>`
            ].join('');

            return [
                { html: `<span class="font-semibold text-slate-800">${node.name}</span>` },
                { html: statusBadge(node.status) },
                { html: `<span class="text-sm text-slate-500">${platform || '—'}</span>` },
                { html: `<span class="text-sm text-slate-500">${encoders}</span>` },
                { html: benchmark
                    ? `<span class="text-sm font-semibold text-slate-700">${benchmark.fps} fps</span> <span class="text-xs text-slate-400">(bal ${node.performanceScore})</span>`
                    : '<span class="text-sm text-slate-400">—</span>' },
                { html: `<span class="text-sm font-semibold text-slate-700">${node.activeJobs} / ${node.maxConcurrentJobs}</span>` },
                { html: `<span class="text-sm text-slate-500" title="${node.lastSeenAt || ''}">${secondsAgo(node.lastSeenAt)}</span>` },
                { html: `<div class="flex justify-end gap-1">${actions}</div>`, cls: 'text-right' }
            ];
        }), 8, { empty: L["Node_NoDevices"] });

        AdminUI.loader(false);
    }

    async function toggle(id, enabled) {
        const res = await AdminUI.putJson(`/node/${id}/enabled?enabled=${enabled}`, {});
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    async function revoke(id) {
        if (!await AdminUI.confirmDelete(L["Node_ConfirmRevoke"])) return;

        const res = await AdminUI.del(`/node/${id}/token`);
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    document.addEventListener('DOMContentLoaded', () => { tab('devices'); load(); });

    return { tab, load, toggle, revoke };
})();
