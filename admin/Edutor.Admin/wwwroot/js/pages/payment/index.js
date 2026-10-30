// Ödənişlər — planlar, ödəniş qeydi və tarixçə.
var pager = AdminUI.createPager('pagination', loadPayments);
const PAYMENTS = {};
const TEMPLATES = [];

const fmtDate = value => value ? new Date(value + 'T00:00:00').toLocaleDateString() : '—';
const fmtMoney = value => value == null ? '—' : Number(value).toFixed(2);
// Normalda daha tünd, hover-də açıq çalar.
const ROW_TONE = {
    current:  'bg-emerald-100 hover:bg-emerald-50',
    upcoming: 'bg-amber-100 hover:bg-amber-50',
    overdue:  'bg-red-100 hover:bg-red-50',
    unpaid:   'bg-rose-100 hover:bg-rose-50',
    _default: 'bg-slate-100 hover:bg-slate-50'
};

const STATUS = {
    upcoming: { label: 'Yaxınlaşır', tone: 'warning' },
    overdue:  { label: 'Gecikib',    tone: 'danger' },
    unpaid:   { label: 'Plan yoxdur', tone: 'neutral' },
    current:  { label: 'Aktiv plan',  tone: 'success' }
};


async function loadTemplateOptions() {
    const sel = document.getElementById('plan-template');
    if (!sel) return; // Plan şablonları abunəlik planında yoxdur — yalnız əl ilə daxil etmə.

    try {
        const items = await AdminUI.get('/payment-plan/list');
        TEMPLATES.length = 0;
        items.forEach(t => TEMPLATES.push(t));

        sel.innerHTML = '<option value="">— Əl ilə daxil et —</option>' +
            items.map(t => `<option value="${t.id}">${AdminUI.escapeHtml(t.name)}</option>`).join('');
    } catch { /* şablon yoxdursa əl ilə */ }
}

async function loadSummary() {
    const s = await AdminUI.get('/payment/summary');
    document.getElementById('count-upcoming').textContent = s.upcoming;
    document.getElementById('count-overdue').textContent = s.overdue;
    document.getElementById('count-unpaid').textContent = s.unpaid;
    document.getElementById('count-current').textContent = s.current;
}

async function loadPayments(page) {
    const pageNo = page || pager.page;
    const search = document.getElementById('search')?.value ?? '';
    const status = document.getElementById('payment-status')?.value ?? '';

    AdminUI.loader(true);
    const data = await AdminUI.get(
        `/payment/list?page=${pageNo}&perPage=20&status=${encodeURIComponent(status)}&search=${encodeURIComponent(search)}`);
    AdminUI.loader(false);

    for (const row of data.data) PAYMENTS[row.studentId] = row;
    pager.apply(data);

    const rows = data.data.map((x, i) => {
        const meta = STATUS[x.status] ?? STATUS.unpaid;
        const actions = AdminUI.rowActions([
            AdminUI.actionButton('Plan', `openPlan(${x.studentId})`, { tone: 'primary' }),
            x.amount != null ? AdminUI.actionButton('Ödəniş al', `recordPayment(${x.studentId})`) : '',
            AdminUI.actionLink('Tarixçə', `/payment/${x.studentId}/history`)
        ]);

        return [
            { html: AdminUI.rowNo(x, i, pager.page, pager.perPage), cls: 'text-slate-400' },
            { html: `<strong>${AdminUI.escapeHtml(x.fullName)}</strong><div class="mt-0.5 text-xs text-slate-500">${AdminUI.escapeHtml(x.email)}</div>` },
            { html: fmtDate(x.joinedOn) },
            { html: fmtDate(x.nextDueOn) },
            { html: x.amount == null ? '—' : `${fmtMoney(x.amount)} ${AdminUI.escapeHtml(x.currency)}` },
            { html: x.frequency === 'custom' ? `${x.intervalDays} gündən bir` : x.frequency ? 'Aylıq' : '—' },
            { html: AdminUI.badge(meta.label, meta.tone) },
            { html: actions, cls: 'text-right' }
        ];
    });

    AdminUI.table('payment-rows', rows, 8, {});

    // Sətir rəngi statusa görə: ödənilib → yaşıl, yaxınlaşır → sarı, gecikib/ödenməyib → qırmızı.
    document.querySelectorAll('#payment-rows tr').forEach((tr, i) => {
        const status = data.data[i]?.status;
        if (!status) return;
        tr.classList.add(...(ROW_TONE[status] || ROW_TONE._default).split(' '));
    });
}

// Filtr paneli dəyişdikcə avtomatik yenilə.
window.loadList = () => loadPayments(1);

// Lokal tarixi yyyy-MM-dd kimi qaytarır (UTC sürüşməsi olmadan).
function toDateInput(d) {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function today() { return toDateInput(new Date()); }

// JS ilə yazılan tarix sahəsinin göstərişi də yenilənsin (tarix seçici komponenti).
function setDateValue(id, value) {
    const el = document.getElementById(id);
    if (!el) return;
    el.value = value;
    if (window.DatePicker) DatePicker.sync(el);
}
function nextMonth() {
    const d = new Date();
    d.setMonth(d.getMonth() + 1);
    return toDateInput(d);
}

// Növbəti ödəniş tarixi perioda görə avtomatik hesablanır.
function recalcDueDate() {
    const joined = document.getElementById('plan-joined').value;
    if (!joined) return;

    const d = new Date(joined + 'T00:00:00');
    if (document.getElementById('plan-frequency').value === 'custom') {
        const days = Number(document.getElementById('plan-interval').value || 0);
        if (days > 0) d.setDate(d.getDate() + days);
    } else {
        d.setMonth(d.getMonth() + 1);
    }

    setDateValue('plan-due', toDateInput(d));
}

window.openPlan = function (studentId) {
    const row = PAYMENTS[studentId];
    if (!row) return;

    const template = document.getElementById('plan-template');
    if (template) { template.value = ''; template.dispatchEvent(new Event('change')); }

    document.getElementById('plan-student-id').value = studentId;
    document.getElementById('plan-joined').value = row.joinedOn || today();
    document.getElementById('plan-amount').value = row.amount ?? '';
    document.getElementById('plan-currency').value = row.currency || 'AZN';
    document.getElementById('plan-frequency').value = row.frequency || 'monthly';
    document.getElementById('plan-interval').value = row.intervalDays || 30;
    document.getElementById('plan-note').value = row.note || '';
    document.getElementById('interval-wrap').classList.toggle('hidden', row.frequency !== 'custom');

    recalcDueDate();
    if (row.amount != null && row.nextDueOn) setDateValue('plan-due', row.nextDueOn);

    // İlk ödəniş yalnız planı olmayan (yeni) tələbə üçün təklif olunur.
    document.getElementById('plan-first-paid-wrap').classList.toggle('hidden', row.amount != null);
    document.getElementById('plan-first-paid').checked = false;

    AdminUI.openModal('planModal');
};

document.getElementById('plan-frequency').addEventListener('change', e => {
    document.getElementById('interval-wrap').classList.toggle('hidden', e.target.value !== 'custom');
    recalcDueDate();
});

document.getElementById('plan-joined').addEventListener('change', recalcDueDate);
document.getElementById('plan-interval').addEventListener('input', recalcDueDate);

document.getElementById('plan-template')?.addEventListener('change', e => {
    const t = TEMPLATES.find(x => String(x.id) === e.target.value);
    if (!t) return;

    document.getElementById('plan-amount').value = t.amount;
    document.getElementById('plan-currency').value = t.currency;
    document.getElementById('plan-frequency').value = t.frequency;
    document.getElementById('plan-interval').value = t.intervalDays || 30;
    document.getElementById('plan-note').value = t.note || '';
    document.getElementById('interval-wrap').classList.toggle('hidden', t.frequency !== 'custom');
    document.getElementById('plan-frequency').dispatchEvent(new Event('change'));
    recalcDueDate();
});

document.getElementById('plan-form').addEventListener('submit', async e => {
    e.preventDefault();

    const body = {
        studentId: +document.getElementById('plan-student-id').value,
        joinedOn: document.getElementById('plan-joined').value,
        nextDueOn: document.getElementById('plan-due').value,
        amount: +document.getElementById('plan-amount').value,
        currency: document.getElementById('plan-currency').value,
        frequency: document.getElementById('plan-frequency').value,
        intervalDays: +document.getElementById('plan-interval').value,
        note: document.getElementById('plan-note').value
    };

    const res = await AdminUI.postJson('/payment/plan', body);
    if (!res.ok) { AdminUI.error(res.data?.message || L["Js_ErrorOccured"]); return; }

    const firstPaid = document.getElementById('plan-first-paid');
    const wantsFirst = firstPaid.checked && !firstPaid.closest('label').classList.contains('hidden');
    if (wantsFirst) {
        const pay = await AdminUI.postJson('/payment/record', { studentId: body.studentId, paidOn: today() });
        if (!pay.ok) { AdminUI.error(pay.data?.message || L["Js_ErrorOccured"]); return; }
    }

    AdminUI.closeModal('planModal');
    AdminUI.notify(wantsFirst ? 'Plan yaradıldı və ilk ödəniş qeyd edildi' : L["Js_Saved"]);
    await Promise.all([loadPayments(pager.page), loadSummary()]);
});

window.recordPayment = function (studentId) {
    const row = PAYMENTS[studentId];
    if (!row || row.amount == null) return;

    document.getElementById('record-student-id').value = studentId;
    document.getElementById('record-summary').textContent =
        `${row.fullName} — ${fmtMoney(row.amount)} ${row.currency}`;
    document.getElementById('record-paid-on').value = today();
    document.getElementById('record-amount').value = row.amount;
    document.getElementById('record-note').value = '';

    AdminUI.openModal('recordModal');
};

document.getElementById('record-payment-form').addEventListener('submit', async e => {
    e.preventDefault();

    const studentId = +document.getElementById('record-student-id').value;
    const paidOn = document.getElementById('record-paid-on').value;
    const amount = +document.getElementById('record-amount').value;
    const note = document.getElementById('record-note').value;

    const answer = await AdminUI.swal({
        title: 'Ödəniş qeyd edilsin?',
        text: `${fmtMoney(amount)} — ${paidOn}`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Bəli, qeyd et',
        cancelButtonText: L["Common_Cancel"]
    });
    if (!answer.isConfirmed) return;

    const res = await AdminUI.postJson('/payment/record', { studentId, paidOn, amount, note });
    if (!res.ok) { AdminUI.error(res.data?.message || L["Js_ErrorOccured"]); return; }

    AdminUI.closeModal('recordModal');
    AdminUI.notify('Ödəniş qeyd edildi');
    await Promise.all([loadPayments(pager.page), loadSummary()]);
});

loadSummary();
loadTemplateOptions();
loadPayments(1);
