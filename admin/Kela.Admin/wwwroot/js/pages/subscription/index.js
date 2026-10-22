window.SubscriptionPage = (function () {
    // Superadmin başqa tenant-ın abunəliyinə baxırsa seçim URL ilə daşınır.
    function switchTenant(el) {
        if (!el || !el.value) return;
        location.href = '/subscription?tenant_id=' + encodeURIComponent(el.value);
    }

    function selectedTenantId() {
        return document.getElementById('subscriptionTenant')?.value || '';
    }

    async function addAddon(kind) {
        const labels = { students: 'tələbə yeri', video: 'video vaxtı', storage: 'depolama' };

        const result = await Swal.fire({
            title: 'Əlavə paket',
            text: `Neçə paket ${labels[kind] || ''} əlavə edilsin?`,
            input: 'number',
            inputValue: 1,
            inputAttributes: { min: 1, max: 100 },
            showCancelButton: true,
            confirmButtonText: 'Əlavə et',
            cancelButtonText: L["Common_Cancel"]
        });

        if (!result.isConfirmed) return;

        const payload = { kind, packs: result.value || 1 };
        const tenantId = selectedTenantId();
        if (tenantId) payload.tenant_id = tenantId;

        const res = await AdminUI.post('/subscription/addons', payload);

        if (res.ok) { AdminUI.notify(L["Js_Saved"]); location.reload(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    return { switchTenant, addAddon };
})();
