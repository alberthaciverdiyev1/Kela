// Sidebar etiketləri — superadmin bölmə üçün etiket yazır, bütün tenant-lar görür.
window.SidebarLabelPage = (function () {
    async function save(row) {
        const key = row.getAttribute('data-sidebar-label-key');
        const label = row.querySelector('[data-label-input]').value.trim();
        const isActive = row.querySelector('[data-label-active]').checked;
        const btn = row.querySelector('[data-save]');

        btn.disabled = true;
        try {
            const res = await AdminUI.putJson('/sidebar-label/' + encodeURIComponent(key), { label, isActive });
            if (res.ok) {
                row.querySelector('[data-label-active]').checked = !!res.data.isActive;
                AdminUI.notify(L["Js_Saved"]);
            } else {
                AdminUI.error(AdminUI.errText(res));
            }
        } finally {
            btn.disabled = false;
        }
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('[data-sidebar-label-key]').forEach(row => {
            row.querySelector('[data-save]')?.addEventListener('click', () => save(row));
        });
    });

    return { save };
})();
