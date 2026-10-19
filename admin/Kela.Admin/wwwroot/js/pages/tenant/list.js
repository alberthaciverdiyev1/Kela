async function loadList() {
    AdminUI.loader(true);
    const data = await AdminUI.get('/tenant/list');

    const q = (document.getElementById('search')?.value ?? '').trim().toLowerCase();
    const items = q ? data.filter(t => (t.name + ' ' + t.slug).toLowerCase().includes(q)) : data;

    AdminUI.setInfo(items.length);
    AdminUI.table('rows', items.map((t, i) => [
        { html: AdminUI.rowNo(t, i, 1, items.length), cls: 'text-slate-400' },
        { html: t.name, cls: 'font-medium text-slate-800' },
        { html: `<code class="text-sm text-slate-500">${t.slug}</code>` },
        { html: t.isPending
            ? AdminUI.badge(L["Tenant_Pending"], 'warning')
            : AdminUI.statusSwitch(t.isActive, `setTenantStatus(${t.id}, ${!t.isActive})`) },
        { html: t.isPending
            ? AdminUI.rowActions([
                AdminUI.actionButton(L["Tenant_Approve"], `approveTenant(${t.id})`, { tone: 'primary' }),
                AdminUI.actionButton(L["Tenant_Reject"], `rejectTenant(${t.id})`, { tone: 'danger' })
            ])
            : AdminUI.rowActions([AdminUI.actionButton(L["Tenant_Switch"], `switchTenant(${t.id})`, { tone: 'primary' })]), cls: 'text-right' }
    ]), 5);

    AdminUI.loader(false);
}

window.approveTenant = async function (id) {
    const res = await AdminUI.putJson(`/tenant/${id}/status`, { isActive: true });
    if (res.ok) { AdminUI.notify(L["Tenant_Approve"]); loadList(); if (window.refreshTenantPendingBadge) window.refreshTenantPendingBadge(); }
    else AdminUI.error(AdminUI.errText(res));
};

window.rejectTenant = async function (id) {
    const ok = await AdminUI.swal({
        title: L["Tenant_Reject"],
        text: L["Tenant_RejectConfirm"],
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: L["Tenant_Reject"],
        cancelButtonText: L["Common_Cancel"]
    });
    if (!ok.isConfirmed) return;

    const res = await AdminUI.del(`/tenant/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadList(); if (window.refreshTenantPendingBadge) window.refreshTenantPendingBadge(); }
    else AdminUI.error(AdminUI.errText(res));
};

window.setTenantStatus = async function (id, isActive) {
    const res = await AdminUI.putJson(`/tenant/${id}/status`, { isActive });
    if (res.ok) { AdminUI.notify(L["Js_StatusChanged"]); loadList(); }
    else AdminUI.error(AdminUI.errText(res));
};

window.switchTenant = async function (id) {
    const res = await AdminUI.postJson(`/tenant/switch/${id}`, {});
    if (res.ok) location.href = '/dashboard';
    else AdminUI.error(AdminUI.errText(res));
};

document.addEventListener('DOMContentLoaded', loadList);
