async function loadList() {
    AdminUI.loader(true);
    const data = await AdminUI.get('/tenant/list');

    AdminUI.table('rows', data.map(t => [
        { html: t.id, cls: 'text-slate-400' },
        { html: t.name, cls: 'font-medium text-slate-800' },
        { html: `<code class="text-sm text-slate-500">${t.slug}</code>` },
        { html: t.isActive ? AdminUI.badge('Aktiv', 'success') : AdminUI.badge('Deaktiv', 'neutral') }
    ]), 4);

    AdminUI.loader(false);
}

document.addEventListener('DOMContentLoaded', loadList);
