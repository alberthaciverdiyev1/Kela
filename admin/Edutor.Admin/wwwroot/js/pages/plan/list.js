window.PlanPage = (function () {
    const limit = (value) => value > 0 ? value : L["Subscription_Unlimited"];

    function uploads(plan) {
        const allowed = [
            plan.allowVideoUpload && 'video',
            plan.allowFileUpload && 'PDF',
            plan.allowAudioUpload && L["Common_Audio"]
        ].filter(Boolean);

        return allowed.length
            ? allowed.map(x => AdminUI.badge(x, 'success')).join(' ')
            : AdminUI.badge(L["PlanCatalog_Off"], "danger");
    }

    function features(plan) {
        try { return JSON.parse(plan.features || '{}'); } catch { return {}; }
    }

    async function load() {
        const plans = await AdminUI.get('/plan/list');

        AdminUI.table('rows', (plans || []).map(p => [
            { html: `<span class="font-bold text-slate-900">${AdminUI.escapeHtml(p.name)}</span><br><code class="text-xs text-slate-400">${AdminUI.escapeHtml(p.code)}</code>` },
            { html: `<span class="font-semibold">${p.monthlyPrice} ${p.currency}</span>` },
            { html: p.trialDays > 0 ? L["Common_NDays"].replace('{0}', p.trialDays) : '—' },
            { html: limit(p.maxStudents) },
            { html: limit(p.maxGroups) },
            { html: p.monthlyVideoMinutes > 0 ? Math.round(p.monthlyVideoMinutes / 60) : L["Subscription_Unlimited"] },
            { html: uploads(p) },
            { html: features(p).paymentTemplates === true ? AdminUI.badge(L["PlanCatalog_On"], "success") : AdminUI.badge(L["PlanCatalog_Off"], "neutral") },
            { html: p.isActive ? AdminUI.badge(L["Common_Active"], "success") : AdminUI.badge(L["Common_Inactive"], "neutral") },
            {
                html: `<div class="flex justify-end gap-1">
                    <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-primary hover:bg-primary-light" onclick="PlanPage.edit(${p.id})">${L["Js_Edit"]}</button>
                    <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="PlanPage.remove(${p.id})">${L["Common_Delete"]}</button>
                </div>`, cls: 'text-right'
            }
        ]), 10, { empty: L["Plan_NoPlans"] });
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
