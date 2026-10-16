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

    AdminUI.setInfo(items.length);
    AdminUI.table('rows', items.map((p, index) => [
        { html: AdminUI.rowNo(p, index), cls: 'text-slate-400' },
        { html: AdminUI.chip(p.type, 'primary') },
        { html: preview(p.content), cls: 'text-slate-500' },
        { html: AdminUI.rowActions([AdminUI.actionButton(L["Js_Edit"], `openEdit('${p.type}')`, { tone: 'primary' })]), cls: 'text-right' }
    ]), 3);

    AdminUI.loader(false);
}

document.addEventListener('DOMContentLoaded', loadList);
