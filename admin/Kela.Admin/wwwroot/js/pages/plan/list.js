window.PlanPage = (function () {
    const gb = (bytes) => bytes > 0 ? `${Math.round(bytes / 1024 / 1024 / 1024)} GB` : 'limitsiz';
    const limit = (value) => value > 0 ? value : 'limitsiz';

    async function load() {
        const plans = await AdminUI.get('/plan/list');

        AdminUI.table('rows', (plans || []).map(p => [
            { html: `<span class="font-bold text-slate-900">${p.name}</span><br><code class="text-xs text-slate-400">${p.code}</code>` },
            { html: `<span class="font-semibold">${p.monthlyPrice} ${p.currency}</span>` },
            { html: limit(p.maxStudents) },
            { html: limit(p.maxTeachers) },
            { html: Math.round(p.monthlyVideoMinutes / 60) },
            { html: gb(p.storageBytes) },
            { html: limit(p.concurrentConversions) },
            { html: p.isActive ? AdminUI.badge('Aktiv', 'success') : AdminUI.badge('Deaktiv', 'neutral') },
            {
                html: `<div class="flex justify-end gap-1">
                    <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-primary hover:bg-primary-light" onclick="PlanPage.edit(${p.id})">Redaktə</button>
                    <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="PlanPage.remove(${p.id})">Sil</button>
                </div>`, cls: 'text-right'
            }
        ]), 9, { empty: 'Plan yoxdur' });
    }

    async function remove(id) {
        if (!await AdminUI.confirmDelete()) return;

        const res = await AdminUI.del(`/plan/${id}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    return { load, remove };
})();
