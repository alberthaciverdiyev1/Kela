window.SubscriptionPage = (function () {
    // Superadmin başqa tenant-ın abunəliyinə baxırsa seçim URL ilə daşınır.
    function switchTenant(el) {
        if (!el || !el.value) return;
        location.href = '/subscription?tenant_id=' + encodeURIComponent(el.value);
    }

    // Ortaq select komponenti açılışda boş placeholder seçir — serverdən gələn
    // tenant-ı geri qaytarırıq ki, səhifə məlumatı ilə seçim uyğun olsun.
    document.addEventListener('DOMContentLoaded', () => {
        const select = document.getElementById('subscriptionTenant');
        if (!select) return;

        const marked = select.querySelector('option[selected]');
        if (marked) select.value = marked.value;
        if (window.AdminUI && AdminUI.syncSelect) AdminUI.syncSelect(select);
    });

    function selectedTenantId() {
        return document.getElementById('subscriptionTenant')?.value || '';
    }

    async function addAddon(kind) {
        const labels = { students: 'tələbə', groups: 'qrup', video: 'saat' };

        const result = await Swal.fire({
            title: 'Limit artırımı',
            text: `Neçə ${labels[kind] || 'vahid'} əlavə edilsin?`,
            input: 'number',
            inputValue: 1,
            inputAttributes: { min: 1, max: 10000 },
            showCancelButton: true,
            confirmButtonText: 'Əlavə et',
            cancelButtonText: L["Common_Cancel"]
        });

        if (!result.isConfirmed) return;

        const payload = { kind, quantity: result.value || 1 };
        const tenantId = selectedTenantId();
        if (tenantId) payload.tenant_id = tenantId;

        const res = await AdminUI.post('/subscription/addons', payload);

        if (res.ok) { AdminUI.notify(L["Js_Saved"]); location.reload(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    return { switchTenant, addAddon };
})();
