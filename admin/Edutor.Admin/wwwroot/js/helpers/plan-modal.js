// Superadmin tenant-a plan təyin edir, vaxtı bitmiş abunəliyi yeniləyir və ya
// başqa paketə keçirir. Tenant səhifəsi və Limitlər səhifəsi eyni modaldan istifadə edir.
window.PlanModal = (function () {
    const byId = (id) => document.getElementById(id);
    let plans = [];

    async function loadPlans() {
        if (plans.length > 0) return plans;

        plans = await AdminUI.get('/tenant/plan-list');

        byId('planSelect').innerHTML = plans
            .map(p => `<option value="${p.id}">${p.name} — ${p.monthlyPrice} ${p.currency}/ay</option>`)
            .join('');

        return plans;
    }

    async function open(tenantId, planId, expiresAt) {
        await loadPlans();

        byId('planTenantId').value = tenantId;

        const select = byId('planSelect');
        // Planı olmayan tenant üçün select boş qalmasın — ilk plan seçili gəlir.
        select.value = planId ? String(planId) : (select.options[0]?.value || '');
        if (AdminUI.syncSelect) AdminUI.syncSelect(select);

        byId('planExpires').value = expiresAt ? expiresAt.substring(0, 10) : '';

        AdminUI.openModal('planModal');
    }

    function close() {
        AdminUI.closeModal('planModal');
    }

    function refresh() {
        // Tenant siyahısı JS ilə yenilənir; digər səhifələrdə plan məlumatı serverdən gəlir.
        if (typeof window.loadList === 'function') window.loadList();
        else location.reload();
    }

    document.addEventListener('DOMContentLoaded', () => {
        const form = byId('planForm');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            const id = byId('planTenantId').value;
            const planId = byId('planSelect').value;
            const expires = byId('planExpires').value;

            if (!planId) { AdminUI.error(L["Plan_SelectPrompt"]); return; }

            const fd = new FormData();
            fd.append('plan_id', planId);
            if (expires) fd.append('expires_at', expires);

            const res = await AdminUI.putForm(`/tenant/${id}/plan`, fd);

            if (res.ok) { AdminUI.notify(L["Js_Saved"]); close(); refresh(); }
            else AdminUI.error(AdminUI.errText(res));
        });
    });

    return { open, close };
})();
