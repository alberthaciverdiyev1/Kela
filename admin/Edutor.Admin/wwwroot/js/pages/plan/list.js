window.PlanPage = (function () {
    const limit = (value) => value > 0 ? value : 'limitsiz';

    function uploads(plan) {
        const allowed = [
            plan.allowVideoUpload && 'video',
            plan.allowFileUpload && 'PDF',
            plan.allowAudioUpload && 'səs'
        ].filter(Boolean);

        return allowed.length
            ? allowed.map(x => AdminUI.badge(x, 'success')).join(' ')
            : AdminUI.badge('bağlı', 'danger');
    }

    async function load() {
        const plans = await AdminUI.get('/plan/list');

        AdminUI.table('rows', (plans || []).map(p => [
            { html: `<span class="font-bold text-slate-900">${AdminUI.escapeHtml(p.name)}</span><br><code class="text-xs text-slate-400">${AdminUI.escapeHtml(p.code)}</code>` },
            { html: `<span class="font-semibold">${p.monthlyPrice} ${p.currency}</span>` },
            { html: limit(p.maxStudents) },
            { html: limit(p.maxGroups) },
            { html: p.monthlyVideoMinutes > 0 ? Math.round(p.monthlyVideoMinutes / 60) : 'limitsiz' },
            { html: uploads(p) },
            { html: p.isActive ? AdminUI.badge('Aktiv', 'success') : AdminUI.badge('Deaktiv', 'neutral') },
            {
                html: `<div class="flex justify-end gap-1">
                    <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-primary hover:bg-primary-light" onclick="PlanPage.edit(${p.id})">Redaktə</button>
                    <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="PlanPage.remove(${p.id})">Sil</button>
                </div>`, cls: 'text-right'
            }
        ]), 8, { empty: 'Plan yoxdur' });
    }

    async function remove(id) {
        if (!await AdminUI.confirmDelete()) return;

        const res = await AdminUI.del(`/plan/${id}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    document.addEventListener('DOMContentLoaded', load);

    return { load, remove };
})();
