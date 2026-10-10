async function loadList() {
    AdminUI.loader(true);
    const data = await AdminUI.get('/tenant/list');

    AdminUI.table('rows', data.map(t => [
        { html: t.id, cls: 'text-slate-400' },
        { html: t.name, cls: 'font-medium text-slate-800' },
        { html: `<code class="text-sm text-slate-500">${t.slug}</code>` },
        { html: t.isActive ? '<span class="rounded-full bg-emerald-100 px-2 py-0.5 text-xs text-emerald-700">Aktiv</span>' : '<span class="rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-500">Deaktiv</span>' }
    ]), 4);

    AdminUI.loader(false);
}

document.addEventListener('DOMContentLoaded', loadList);
