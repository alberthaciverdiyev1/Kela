window.PlanCatalogPage = (function () {
    // Superadmin başqa tenant-ın planlarına baxırsa seçim URL ilə daşınır.
    function switchTenant(el) {
        if (!el || !el.value) return;
        location.href = '/plan-catalog?tenant_id=' + encodeURIComponent(el.value);
    }

    // Ortaq select komponenti açılışda boş placeholder seçir — serverdən gələn
    // tenant-ı geri qaytarırıq ki, səhifə məlumatı ilə seçim uyğun olsun.
    document.addEventListener('DOMContentLoaded', () => {
        const select = document.getElementById('planCatalogTenant');
        if (!select) return;

        const marked = select.querySelector('option[selected]');
        if (marked) select.value = marked.value;
        if (window.AdminUI && AdminUI.syncSelect) AdminUI.syncSelect(select);
    });

    function limitLabel(value) {
        return value > 0 ? String(value) : L["Subscription_Unlimited"];
    }

    function storageLabel(bytes) {
        return bytes > 0 ? Math.round(bytes / 1073741824) + ' GB' : L["Subscription_Unlimited"];
    }

    // Plan seçimi hazırda WhatsApp müraciəti kimi göndərilir (aktivləşdirmə əl ilə).
    async function choosePlan(planId) {
        const ctx = window.PLAN_CATALOG_CONTEXT;
        const plan = ctx?.plans?.find(p => p.id === Number(planId));
        if (!plan) return;

        const result = await AdminUI.swal({
            title: L["PlanChoose_Title"],
            text: L["PlanCatalog_WaConfirmText"].replace('{0}', plan.name),
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: L["PlanCatalog_Send"],
            cancelButtonText: L["Common_Cancel"]
        });

        if (!result.isConfirmed) return;

        const lines = [
            L["PlanCatalog_WaGreeting"],
            '',
            L["PlanCatalog_WaOrg"].replace('{0}', ctx.tenantName || '—'),
            L["PlanCatalog_WaRequester"].replace('{0}', ctx.userName || '—'),
            L["PlanCatalog_WaEmail"].replace('{0}', ctx.userEmail || '—'),
            L["PlanCatalog_WaCurrent"].replace('{0}', ctx.currentPlanName || '—'),
            L["PlanCatalog_WaSelected"].replace('{0}', plan.name).replace('{1}', plan.code),
            L["PlanCatalog_WaPrice"].replace('{0}', plan.price).replace('{1}', plan.currency),
            L["PlanCatalog_WaStudents"].replace('{0}', limitLabel(plan.maxStudents)),
            L["PlanCatalog_WaGroups"].replace('{0}', limitLabel(plan.maxGroups)),
            L["PlanCatalog_WaVideo"].replace('{0}', plan.videoMinutes > 0 ? L["PlanCatalog_Hours"].replace('{0}', Math.round(plan.videoMinutes / 60)) : L["Subscription_Unlimited"]),
            L["PlanCatalog_WaStorage"].replace('{0}', storageLabel(plan.storageBytes)),
            L["PlanCatalog_WaMaxQuality"].replace('{0}', plan.maxVideoHeight > 0 ? plan.maxVideoHeight + 'p' : L["Subscription_Unlimited"]),
            L["PlanCatalog_WaDate"].replace('{0}', new Date().toLocaleString(window.LANG || 'az'))
        ];

        window.open(`https://wa.me/${ctx.whatsapp}?text=${encodeURIComponent(lines.join('\n'))}`, '_blank', 'noopener');
    }

    return { switchTenant, choosePlan };
})();
