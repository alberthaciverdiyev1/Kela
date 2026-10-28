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

    // Tab dəyişəndə yalnız bu səhifəyə aid yan effektlər qalır.
    document.addEventListener('tab:change', (e) => {
        const name = e.detail.tab;
        const createBtn = document.getElementById('createCodeBtn');
        const isCodes = name === 'codes';
        if (createBtn) createBtn.classList.toggle('hidden', !isCodes);
        const note = document.getElementById('refreshNote');
        if (note) note.classList.toggle('hidden', !isCodes);

        if (name === 'jobs') JobsPage.reload();
        if (name === 'codes') CodesPage.reload();
    });

    function tab(name) { AdminUI.selectTab('nodeTabs', name); }

    async function load() {
        AdminUI.loader(true);
        const data = await AdminUI.get('/node/list?perPage=500');
        // API "data" qaytarır (PagedResponse) — əvvəl "items" oxunduğu üçün cədvəl boş qalırdı.
        const nodes = data.data || [];
        AdminUI.setInfo(nodes.length);

        AdminUI.table('rows', nodes.map((node, index) => {
            const device = node.device || {};
            const benchmark = node.benchmark;
            const encoders = (device.encoders || []).join(', ') || '—';
            const platform = [device.operatingSystem, device.cpuCores ? `${device.cpuCores} CPU` : null,
                device.totalRamMb ? `${Math.round(device.totalRamMb / 1024)} GB` : null]
                .filter(Boolean).join(' · ');

            const actions = AdminUI.rowActions([
                AdminUI.actionButton(node.enabled ? L["Node_Disable"] : L["Node_Enable"], `NodesPage.toggle(${node.id}, ${!node.enabled})`),
                AdminUI.actionButton(L["Node_Revoke"], `NodesPage.revoke(${node.id})`, { tone: 'danger' })
            ]);

            return [
                { html: AdminUI.rowNo(node, index), cls: 'text-slate-400' },
                { html: `<span class="font-semibold text-slate-800">${AdminUI.escapeHtml(node.name)}</span>` },
                { html: statusBadge(node.status) },
                { html: `<span class="text-sm text-slate-500">${AdminUI.escapeHtml(platform || '—')}</span>` },
                { html: `<span class="text-sm text-slate-500">${AdminUI.escapeHtml(encoders)}</span>` },
                { html: benchmark
                    ? `<span class="text-sm font-semibold text-slate-700">${benchmark.fps} fps</span> <span class="text-xs text-slate-400">(bal ${node.performanceScore})</span>`
                    : '<span class="text-sm text-slate-400">—</span>' },
                { html: `<span class="text-sm font-semibold text-slate-700">${node.activeJobs} / ${node.maxConcurrentJobs}</span>` },
                { html: `<span class="text-sm text-slate-500" title="${AdminUI.escapeHtml(node.lastSeenAt || '')}">${secondsAgo(node.lastSeenAt)}</span>` },
                { html: actions, cls: 'text-right' }
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
