window.PlanModal = (function () {
    const modal = () => document.getElementById('planModal');
    let plans = [];

    async function loadPlans() {
        if (plans.length > 0) return plans;

        plans = await AdminUI.get('/tenant/plan-list');

        const select = document.getElementById('planSelect');
        select.innerHTML = plans
            .map(p => `<option value="${p.id}">${p.name} — ${p.monthlyPrice} ${p.currency}/ay</option>`)
            .join('');

        return plans;
    }

    async function open(tenantId, planId, expiresAt) {
        await loadPlans();

        document.getElementById('planTenantId').value = tenantId;

        const select = document.getElementById('planSelect');
        select.value = planId || '';
        if (AdminUI.syncSelect) AdminUI.syncSelect(select);

        document.getElementById('planExpires').value = expiresAt ? expiresAt.substring(0, 10) : '';

        AdminUI.openModal('planModal');
    }

    function close() {
        AdminUI.closeModal('planModal');
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.getElementById('planForm').addEventListener('submit', async (e) => {
            e.preventDefault();

            const id = document.getElementById('planTenantId').value;
            const expires = document.getElementById('planExpires').value;

            const fd = new FormData();
            fd.append('plan_id', document.getElementById('planSelect').value);
            if (expires) fd.append('expires_at', expires);

            const res = await AdminUI.putForm(`/tenant/${id}/plan`, fd);

            if (res.ok) { AdminUI.notify(L["Js_Saved"]); close(); loadList(); }
            else AdminUI.error(AdminUI.errText(res));
        });
    });

    return { open, close };
})();
