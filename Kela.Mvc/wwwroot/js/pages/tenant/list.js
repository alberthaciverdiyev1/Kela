async function loadList() {
    AdminUI.loader(true);
    const data = await AdminUI.get('/tenant/list');

    const q = (document.getElementById('search')?.value ?? '').trim().toLowerCase();
    const items = q ? data.filter(t => (t.name + ' ' + t.slug).toLowerCase().includes(q)) : data;

    AdminUI.table('rows', items.map((t, i) => [
        { html: AdminUI.rowNo(t, i, 1, items.length), cls: 'text-slate-400' },
        { html: t.name, cls: 'font-medium text-slate-800' },
        { html: `<code class="text-sm text-slate-500">${t.slug}</code>` },
        { html: t.isActive ? AdminUI.badge('Aktiv', 'success') : AdminUI.badge('Deaktiv', 'neutral') },
        { html: `<div class="flex justify-end"><button type="button" class="inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-sm font-semibold text-primary hover:bg-primary-light" onclick="switchTenant(${t.id})">${L["Tenant_Switch"]}</button></div>`, cls: 'text-right' }
    ]), 5);

    AdminUI.loader(false);
}

window.switchTenant = async function (id) {
    const res = await AdminUI.postJson(`/tenant/switch/${id}`, {});
    if (res.ok) location.href = '/dashboard';
    else AdminUI.error(AdminUI.errText(res));
};

document.addEventListener('DOMContentLoaded', loadList);
