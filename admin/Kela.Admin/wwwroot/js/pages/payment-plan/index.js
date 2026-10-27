// Plan şablonları — tələbəyə plan təyin edərkən seçilən hazır planlar.
const TEMPLATES = [];

function escT(v) {
    const el = document.createElement('span');
    el.textContent = v ?? '';
    return el.innerHTML;
}

async function loadTemplates() {
    AdminUI.loader(true);
    const items = await AdminUI.get('/payment/templates/list');
    AdminUI.loader(false);

    TEMPLATES.length = 0;
    items.forEach(t => TEMPLATES.push(t));
    AdminUI.setInfo(items.length);

    AdminUI.table('template-rows', items.map((t, i) => [
        { html: AdminUI.rowNo(t, i, 1, items.length), cls: 'text-slate-400' },
        { html: `<strong>${escT(t.name)}</strong>` },
        { html: Number(t.amount).toFixed(2) },
        { html: escT(t.currency) },
        { html: t.frequency === 'custom' ? `${t.intervalDays} gündən bir` : 'Aylıq' },
        {
            html: AdminUI.rowActions([
                AdminUI.actionButton(L["Js_Edit"], `openTemplate(${t.id})`, { tone: 'primary' }),
                AdminUI.actionButton(L["Common_Delete"], `removeTemplate(${t.id})`, { tone: 'danger' })
            ]), cls: 'text-right'
        }
    ]), 6, { empty: 'Hələ plan şablonu yoxdur.' });
}

window.openTemplate = function (id) {
    const t = TEMPLATES.find(x => x.id === id);
    document.getElementById('templateModalTitle').textContent = t ? 'Şablonu redaktə et' : 'Yeni plan şablonu';
    document.getElementById('template-id').value = t?.id ?? '';
    document.getElementById('template-name').value = t?.name ?? '';
    document.getElementById('template-amount').value = t?.amount ?? '';
    document.getElementById('template-currency').value = t?.currency ?? 'AZN';
    document.getElementById('template-frequency').value = t?.frequency ?? 'monthly';
    document.getElementById('template-interval').value = t?.intervalDays || 30;
    document.getElementById('template-note').value = t?.note ?? '';
    document.getElementById('template-interval-wrap').classList.toggle('hidden', (t?.frequency ?? 'monthly') !== 'custom');
    AdminUI.openModal('templateModal');
};

window.removeTemplate = async function (id) {
    const ok = await AdminUI.confirmDelete('Bu plan şablonu silinsin?');
    if (!ok) return;

    const res = await AdminUI.del(`/payment/templates/${id}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadTemplates(); }
    else AdminUI.error(AdminUI.errText(res));
};

document.getElementById('template-frequency').addEventListener('change', e =>
    document.getElementById('template-interval-wrap').classList.toggle('hidden', e.target.value !== 'custom'));

document.getElementById('template-form').addEventListener('submit', async e => {
    e.preventDefault();
    const id = document.getElementById('template-id').value;
    const body = {
        id: id ? +id : null,
        name: document.getElementById('template-name').value,
        amount: +document.getElementById('template-amount').value,
        currency: document.getElementById('template-currency').value,
        frequency: document.getElementById('template-frequency').value,
        intervalDays: +document.getElementById('template-interval').value,
        note: document.getElementById('template-note').value
    };

    const res = await AdminUI.postJson('/payment/templates', body);
    if (!res.ok) { AdminUI.error(res.data?.message || L["Js_ErrorOccured"]); return; }

    AdminUI.closeModal('templateModal');
    AdminUI.notify(L["Js_Saved"]);
    loadTemplates();
});

loadTemplates();
