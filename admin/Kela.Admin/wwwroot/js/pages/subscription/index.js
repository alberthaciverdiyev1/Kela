window.SubscriptionPage = (function () {
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

        const res = await AdminUI.post('/subscription/addons', {
            kind,
            packs: result.value || 1
        });

        if (res.ok) { AdminUI.notify(L["Js_Saved"]); location.reload(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    return { addAddon };
})();
