// Ödənişlər — planlar, ödəniş qeydi və tarixçə.
let currentPage = 1;
const PAYMENTS = {};

const fmtDate = value => value ? new Date(value + 'T00:00:00').toLocaleDateString() : '—';
const fmtMoney = value => value == null ? '—' : Number(value).toFixed(2);
const STATUS = {
    upcoming: { label: 'Yaxınlaşır', tone: 'warning' },
    overdue:  { label: 'Gecikib',    tone: 'danger' },
    unpaid:   { label: 'Plan yoxdur', tone: 'neutral' },
    current:  { label: 'Aktiv plan',  tone: 'success' }
};

function esc(v) {
    const el = document.createElement('span');
    el.textContent = v ?? '';
    return el.innerHTML;
}

async function loadSummary() {
    const s = await AdminUI.get('/payment/summary');
    document.getElementById('count-upcoming').textContent = s.upcoming;
    document.getElementById('count-overdue').textContent = s.overdue;
    document.getElementById('count-unpaid').textContent = s.unpaid;
    document.getElementById('count-current').textContent = s.current;
}

async function loadPayments(page) {
    currentPage = page || 1;
    const search = document.getElementById('search')?.value ?? '';
    const status = document.getElementById('payment-status')?.value ?? '';

    AdminUI.loader(true);
    const data = await AdminUI.get(
        `/payment/list?page=${currentPage}&perPage=20&status=${encodeURIComponent(status)}&search=${encodeURIComponent(search)}`);
    AdminUI.loader(false);

    for (const row of data.data) PAYMENTS[row.studentId] = row;
    AdminUI.setInfo(data.total);

    const rows = data.data.map((x, i) => {
        const meta = STATUS[x.status] ?? STATUS.unpaid;
        const actions = AdminUI.rowActions([
            AdminUI.actionButton('Plan', `openPlan(${x.studentId})`, { tone: 'primary' }),
            x.amount != null ? AdminUI.actionButton('Ödəniş al', `recordPayment(${x.studentId})`) : '',
            AdminUI.actionLink('Tarixçə', `/payment/${x.studentId}/history`)
        ]);

        return [
            { html: AdminUI.rowNo(x, i, currentPage, data.perPage), cls: 'text-slate-400' },
            { html: `<strong>${esc(x.fullName)}</strong><div class="mt-0.5 text-xs text-slate-500">${esc(x.email)}</div>` },
            { html: fmtDate(x.joinedOn) },
            { html: fmtDate(x.nextDueOn) },
            { html: x.amount == null ? '—' : `${fmtMoney(x.amount)} ${esc(x.currency)}` },
            { html: x.frequency === 'custom' ? `${x.intervalDays} gündən bir` : x.frequency ? 'Aylıq' : '—' },
            { html: AdminUI.badge(meta.label, meta.tone) },
            { html: actions, cls: 'text-right' }
        ];
    });

    AdminUI.table('payment-rows', rows, 8, {});
    AdminUI.pagination('pagination', currentPage, data.lastPage, loadPayments);
}

// Filtr paneli dəyişdikcə avtomatik yenilə.
window.loadList = () => loadPayments(1);

function today() { return new Date().toISOString().slice(0, 10); }
function nextMonth() {
    const d = new Date();
    d.setMonth(d.getMonth() + 1);
    return d.toISOString().slice(0, 10);
}

window.openPlan = function (studentId) {
    const row = PAYMENTS[studentId];
    if (!row) return;

    document.getElementById('plan-student-id').value = studentId;
    document.getElementById('plan-joined').value = row.joinedOn || today();
    document.getElementById('plan-due').value = row.nextDueOn || nextMonth();
    document.getElementById('plan-amount').value = row.amount ?? '';
    document.getElementById('plan-currency').value = row.currency || 'AZN';
    document.getElementById('plan-frequency').value = row.frequency || 'monthly';
    document.getElementById('plan-interval').value = row.intervalDays || 30;
    document.getElementById('plan-note').value = row.note || '';
    document.getElementById('interval-wrap').classList.toggle('hidden', row.frequency !== 'custom');

    AdminUI.openModal('planModal');
};

window.openDefaultPlan = function () {
    document.getElementById('default-joined').value = today();
    document.getElementById('default-due').value = nextMonth();
    document.getElementById('default-amount').value = '';
    document.getElementById('default-currency').value = 'AZN';
    document.getElementById('default-frequency').value = 'monthly';
    document.getElementById('default-interval').value = 30;
    document.getElementById('default-note').value = '';
    document.getElementById('default-interval-wrap').classList.add('hidden');

    AdminUI.openModal('defaultPlanModal');
};

document.getElementById('plan-frequency').addEventListener('change', e =>
    document.getElementById('interval-wrap').classList.toggle('hidden', e.target.value !== 'custom'));

document.getElementById('default-frequency').addEventListener('change', e =>
    document.getElementById('default-interval-wrap').classList.toggle('hidden', e.target.value !== 'custom'));

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

    AdminUI.closeModal('planModal');
    AdminUI.notify(L["Js_Saved"]);
    await Promise.all([loadPayments(currentPage), loadSummary()]);
});

document.getElementById('default-plan-form').addEventListener('submit', async e => {
    e.preventDefault();
    const body = {
        joinedOn: document.getElementById('default-joined').value,
        nextDueOn: document.getElementById('default-due').value,
        amount: +document.getElementById('default-amount').value,
        currency: document.getElementById('default-currency').value,
        frequency: document.getElementById('default-frequency').value,
        intervalDays: +document.getElementById('default-interval').value,
        note: document.getElementById('default-note').value
    };

    const res = await AdminUI.postJson('/payment/default-plan', body);
    if (!res.ok) { AdminUI.error(res.data?.message || L["Js_ErrorOccured"]); return; }

    AdminUI.closeModal('defaultPlanModal');
    AdminUI.notify(`${res.data.created} şagird üçün standart plan yaradıldı`);
    await Promise.all([loadPayments(1), loadSummary()]);
});

window.recordPayment = async function (studentId) {
    const row = PAYMENTS[studentId];
    if (!row || row.amount == null) return;

    const answer = await AdminUI.swal({
        title: 'Ödəniş qeyd edilsin?',
        text: `${fmtMoney(row.amount)} ${row.currency} — bu gün üçün ödəniş alınacaq.`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Bəli, qeyd et',
        cancelButtonText: L["Common_Cancel"]
    });
    if (!answer.isConfirmed) return;

    const res = await AdminUI.postJson('/payment/record', { studentId, paidOn: today() });
    if (!res.ok) { AdminUI.error(res.data?.message || L["Js_ErrorOccured"]); return; }

    AdminUI.notify('Ödəniş qeyd edildi');
    await Promise.all([loadPayments(currentPage), loadSummary()]);
};

loadSummary();
loadPayments(1);
