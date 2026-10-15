let policyData = [];

function preview(html) {
    const text = (html || '').replace(/<[^>]*>/g, ' ').replace(/\s+/g, ' ').trim();
    return text.length > 80 ? text.slice(0, 80) + '…' : (text || '-');
}

async function loadList() {
    AdminUI.loader(true);
    policyData = await AdminUI.get('/privacy-policy/list');

    const q = (document.getElementById('search')?.value ?? '').trim().toLowerCase();
    const items = q ? policyData.filter(p => (p.type + ' ' + (p.content || '')).toLowerCase().includes(q)) : policyData;

    AdminUI.table('rows', items.map(p => [
        { html: `<span class="rounded bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">${p.type}</span>` },
        { html: preview(p.content), cls: 'text-slate-500' },
        { html: `<div class="flex justify-end"><button type="button" class="px-2 py-1 text-primary hover:bg-primary/10 rounded-lg" onclick="openEdit('${p.type}')">${L["Js_Edit"]}</button></div>`, cls: 'text-right' }
    ]), 3);

    AdminUI.loader(false);
}

document.addEventListener('DOMContentLoaded', loadList);
