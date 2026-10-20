const paymentHeaders = () => ({ 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || '' });
const fmtDate = value => value ? new Date(value + 'T00:00:00').toLocaleDateString() : '—';
const statusLabel = { upcoming: 'Yaxınlaşır', overdue: 'Gecikib', unpaid: 'Plan yoxdur', current: 'Aktiv' };
async function loadPayments() {
    const query = new URLSearchParams({ status: document.getElementById('payment-status').value, search: document.getElementById('payment-search').value });
    const rows = await (await fetch(`/payment/list?${query}`)).json();
    for (const key of ['upcoming','overdue','unpaid','current']) document.getElementById(`count-${key}`).textContent = rows.filter(x => x.status === key).length;
    document.getElementById('payment-rows').innerHTML = rows.map(x => `<tr><td><strong>${escapePayment(x.fullName)}</strong><div class="text-xs text-slate-500">${escapePayment(x.email)}</div></td><td>${fmtDate(x.joinedOn)}</td><td>${fmtDate(x.nextDueOn)}</td><td>${x.amount == null ? '—' : `${x.amount.toFixed(2)} ${escapePayment(x.currency)}`}</td><td>${x.frequency === 'custom' ? `${x.intervalDays} gündən bir` : x.frequency ? 'Aylıq' : '—'}</td><td>${statusLabel[x.status]}</td><td><div class="flex gap-2"><button class="admin-button-secondary" onclick="editPlanEncoded('${encodeURIComponent(JSON.stringify(x))}')">Plan</button>${x.amount != null ? `<button class="admin-button-primary" onclick="recordPayment(${x.studentId},${x.amount})">Ödəniş al</button>` : ''}<button class="admin-button-secondary" onclick="paymentHistory(${x.studentId})">Tarixçə</button></div></td></tr>`).join('');
}
function escapePayment(value) { const el = document.createElement('span'); el.textContent = value || ''; return el.innerHTML; }
function editPlanEncoded(value) { editPlan(JSON.parse(decodeURIComponent(value))); }
function editPlan(row) {
    document.getElementById('plan-student-id').value = row.studentId;
    document.getElementById('plan-joined').value = row.joinedOn || new Date().toISOString().slice(0,10);
    document.getElementById('plan-due').value = row.nextDueOn || new Date().toISOString().slice(0,10);
    document.getElementById('plan-amount').value = row.amount || '';
    document.getElementById('plan-currency').value = row.currency || 'AZN';
    document.getElementById('plan-frequency').value = row.frequency || 'monthly';
    document.getElementById('plan-interval').value = row.intervalDays || 30;
    document.getElementById('plan-note').value = row.note || '';
    document.getElementById('interval-wrap').classList.toggle('hidden', row.frequency !== 'custom');
    document.getElementById('plan-dialog').showModal();
}
document.getElementById('plan-frequency').addEventListener('change', e => document.getElementById('interval-wrap').classList.toggle('hidden', e.target.value !== 'custom'));
document.getElementById('plan-form').addEventListener('submit', async e => {
    e.preventDefault();
    const body = { studentId: +document.getElementById('plan-student-id').value, joinedOn: document.getElementById('plan-joined').value, nextDueOn: document.getElementById('plan-due').value, amount: +document.getElementById('plan-amount').value, currency: document.getElementById('plan-currency').value, frequency: document.getElementById('plan-frequency').value, intervalDays: +document.getElementById('plan-interval').value, note: document.getElementById('plan-note').value };
    const response = await fetch('/payment/plan', { method: 'POST', headers: paymentHeaders(), body: JSON.stringify(body) });
    if (!response.ok) { const err = await response.json(); alert(err.message || 'Xəta baş verdi'); return; }
    document.getElementById('plan-dialog').close(); await loadPayments();
});
async function recordPayment(studentId, amount) {
    if (!confirm(`${amount.toFixed(2)} məbləğində ödənişi bu gün üçün qeyd edək?`)) return;
    const response = await fetch('/payment/record', { method: 'POST', headers: paymentHeaders(), body: JSON.stringify({ studentId, paidOn: new Date().toISOString().slice(0,10) }) });
    if (!response.ok) { const err = await response.json(); alert(err.message || 'Xəta baş verdi'); return; }
    await loadPayments();
}
async function paymentHistory(id) { const history = await (await fetch(`/payment/${id}/history`)).json(); alert(history.length ? history.map(x => `${fmtDate(x.paidOn)} — ${x.amount.toFixed(2)} ${x.currency}${x.note ? ` (${x.note})` : ''}`).join('\n') : 'Ödəniş tarixçəsi boşdur.'); }
document.getElementById('payment-search').addEventListener('keydown', e => { if (e.key === 'Enter') loadPayments(); });
loadPayments();
