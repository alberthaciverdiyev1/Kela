// Superadmin müştəriyə plan limitindən fərqli limitlər verir (fərdi override).
window.LimitOverridesModal = (function () {
    const byId = (id) => document.getElementById(id);

    async function open(tenantId) {
        const data = await AdminUI.get(`/subscription/${tenantId}/limits`);

        byId('limitsTenantId').value = tenantId;
        byId('limitsStudents').value = data.maxStudents ?? '';
        byId('limitsGroups').value = data.maxGroups ?? '';
        byId('limitsVideoMinutes').value = data.maxVideoMinutes ?? '';
        byId('limitsStorageGb').value = data.maxStorageGb ?? '';

        AdminUI.openModal('limitsModal');
    }

    function close() {
        AdminUI.closeModal('limitsModal');
    }

    document.addEventListener('DOMContentLoaded', () => {
        const form = byId('limitsForm');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            const fd = new FormData(form);
            // Boş sahə "plan limiti qüvvədədir" deməkdir — göndərilmir.
            for (const [key, value] of [...fd.entries()]) {
                if (String(value).trim() === '') fd.delete(key);
            }

            const res = await AdminUI.putForm(`/subscription/${byId('limitsTenantId').value}/limits`, fd);

            if (res.ok) { AdminUI.notify(L["Js_Saved"]); close(); location.reload(); }
            else AdminUI.error(AdminUI.errText(res));
        });
    });

    return { open, close };
})();
