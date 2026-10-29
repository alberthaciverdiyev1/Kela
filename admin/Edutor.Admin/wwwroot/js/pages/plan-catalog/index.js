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
        return value > 0 ? String(value) : 'limitsiz';
    }

    function storageLabel(bytes) {
        return bytes > 0 ? Math.round(bytes / 1073741824) + ' GB' : 'limitsiz';
    }

    // Plan seçimi hazırda WhatsApp müraciəti kimi göndərilir (aktivləşdirmə əl ilə).
    async function choosePlan(planId) {
        const ctx = window.PLAN_CATALOG_CONTEXT;
        const plan = ctx?.plans?.find(p => p.id === Number(planId));
        if (!plan) return;

        const result = await AdminUI.swal({
            title: 'Plan seçimi',
            text: `«${plan.name}» planı üçün WhatsApp ilə müraciət göndərilsin?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Göndər',
            cancelButtonText: L["Common_Cancel"]
        });

        if (!result.isConfirmed) return;

        const lines = [
            'Salam! Abunəlik planı dəyişikliyi üçün müraciət:',
            '',
            `Müəssisə: ${ctx.tenantName || '—'}`,
            `Müraciət edən: ${ctx.userName || '—'}`,
            `E-poçt: ${ctx.userEmail || '—'}`,
            `Cari plan: ${ctx.currentPlanName || '—'}`,
            `Seçilmiş plan: ${plan.name} (${plan.code})`,
            `Qiymət: ${plan.price} ${plan.currency}/ay`,
            `Tələbə limiti: ${limitLabel(plan.maxStudents)}`,
            `Qrup limiti: ${limitLabel(plan.maxGroups)}`,
            `Video (aylıq): ${plan.videoMinutes > 0 ? Math.round(plan.videoMinutes / 60) + ' saat' : 'limitsiz'}`,
            `Yaddaş: ${storageLabel(plan.storageBytes)}`,
            `Maks. video keyfiyyəti: ${plan.maxVideoHeight > 0 ? plan.maxVideoHeight + 'p' : 'limitsiz'}`,
            `Tarix: ${new Date().toLocaleString('az')}`
        ];

        window.open(`https://wa.me/${ctx.whatsapp}?text=${encodeURIComponent(lines.join('\n'))}`, '_blank', 'noopener');
    }

    return { switchTenant, choosePlan };
})();
