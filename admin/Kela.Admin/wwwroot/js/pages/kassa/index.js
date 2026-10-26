window.KassaPage = (function () {
    const data = window.KASSA || { currencies: [], monthly: [], recent: [] };
    const esc = (v) => AdminUI.escapeHtml(v);

    let currency = data.currencies[0]?.currency || 'AZN';
    let students = null;
    let planTemplates = null;

    const money = (value) => Number(value || 0).toLocaleString('az', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const amountText = (value, code) => `${money(value)} ${code}`;
    const today = () => new Date().toLocaleDateString('sv-SE'); // yyyy-MM-dd

    function addDays(iso, days) {
        const date = new Date(iso + 'T00:00:00');
        date.setDate(date.getDate() + days);
        return date.toLocaleDateString('sv-SE');
    }

    function addMonths(iso, months) {
        const date = new Date(iso + 'T00:00:00');
        date.setMonth(date.getMonth() + months);
        return date.toLocaleDateString('sv-SE');
    }

    function periodLabel(template) {
        return template.frequency === 'custom'
            ? `${template.intervalDays || 30} gün`
            : 'aylıq';
    }

    function statsFor(code) {
        return data.currencies.find(c => c.currency === code) || {};
    }

    function renderStats() {
        const s = statsFor(currency);

        document.getElementById('k-today').textContent = amountText(s.today, currency);
        document.getElementById('k-this').textContent = amountText(s.thisMonth, currency);
        document.getElementById('k-expected').textContent = amountText(s.expectedThisMonth, currency);
        document.getElementById('k-last').textContent = amountText(s.lastMonth, currency);
        document.getElementById('k-projection').textContent = amountText(s.projected, currency);
        document.getElementById('k-overdue').textContent = amountText(s.overdue, currency);

        const deltaBox = document.getElementById('kassaDelta');
        if (!s.lastMonth) {
            deltaBox.textContent = s.thisMonth ? 'Keçən ay ödəniş olmayıb' : '';
            return;
        }

        const pct = Math.round((s.thisMonth - s.lastMonth) * 100 / s.lastMonth);
        deltaBox.textContent = `${pct > 0 ? '+' : ''}${pct}% keçən aya nisbətən`;
        deltaBox.className = `text-sm font-semibold ${pct >= 0 ? 'text-primary' : 'text-danger'}`;
    }

    function renderMonths() {
        const rows = data.monthly.filter(m => m.currency === currency);
        const box = document.getElementById('kassaMonths');
        const max = Math.max(...rows.map(r => r.amount), 1);

        box.innerHTML = rows.map(r => {
            const height = Math.max(2, Math.round(r.amount * 100 / max));
            return `<div class="flex h-full flex-1 flex-col items-center justify-end gap-1" title="${r.period}: ${amountText(r.amount, currency)}">
                <div class="w-full rounded-t-md ${r.amount > 0 ? 'bg-primary' : 'bg-slate-200'}" style="height:${height}%"></div>
                <span class="text-[10px] font-medium text-slate-400">${r.period.slice(5)}</span>
            </div>`;
        }).join('');
    }

    function renderPayments(rows, emptyText) {
        const box = document.getElementById('kassaRecent');

        box.innerHTML = rows.length
            ? rows.map(r => `<tr>
                    <td class="whitespace-nowrap text-slate-500">${r.paidOn}</td>
                    <td class="font-medium text-slate-800">${esc(r.student)}</td>
                    <td class="whitespace-nowrap text-right font-semibold text-slate-800">${amountText(r.amount, r.currency)}</td>
                    <td class="text-slate-500">${esc(r.note || '—')}</td>
                    <td class="text-right">
                        <button type="button" class="admin-row-action is-danger" data-action="KassaPage.removePayment" data-args='[${r.id}]'>Sil</button>
                    </td>
                </tr>`).join('')
            : `<tr><td colspan="5" class="p-4 text-sm text-slate-400">${emptyText}</td></tr>`;
    }

    function renderRecent() {
        renderPayments(data.recent, 'Hələ ödəniş qeydə alınmayıb.');
    }

    async function applyRange() {
        const from = document.getElementById('kassaFrom').value;
        const to = document.getElementById('kassaTo').value;

        const query = new URLSearchParams();
        if (from) query.set('from', from);
        if (to) query.set('to', to);

        const rows = await AdminUI.get('/kassa/payments' + (query.toString() ? '?' + query : ''));
        renderPayments(rows, 'Seçilmiş aralıqda ödəniş yoxdur.');

        const totals = rows.reduce((acc, r) => {
            acc[r.currency] = (acc[r.currency] || 0) + Number(r.amount || 0);
            return acc;
        }, {});

        const text = Object.entries(totals).map(([code, total]) => amountText(total, code)).join(' · ');
        document.getElementById('kassaRangeTotal').textContent = text ? `Aralıq mədaxili: ${text}` : '';
    }

    function clearRange() {
        document.getElementById('kassaFrom').value = '';
        document.getElementById('kassaTo').value = '';
        document.getElementById('kassaRangeTotal').textContent = '';
        renderRecent();
    }

    function fillStudentSelect() {
        const select = document.getElementById('kassaStudent');

        select.innerHTML = '<option value="">Tələbə seçin</option>' + students.map(s => {
            const plan = s.amount ? ` — ${money(s.amount)} ${s.currency}` : '';
            return `<option value="${s.id}">${esc(s.name)}${plan}</option>`;
        }).join('');

        AdminUI.syncSelect(select);
    }

    function fillPlanSelect() {
        const select = document.getElementById('kassaPlan');

        select.innerHTML = '<option value="">Plan seçilməyib</option>' + planTemplates.map(t =>
            `<option value="${t.id}">${esc(t.name)} — ${money(t.amount)} ${t.currency} (${periodLabel(t)})</option>`
        ).join('');

        AdminUI.syncSelect(select);
    }

    function selectedStudent() {
        const id = document.getElementById('kassaStudent').value;
        return students?.find(s => String(s.id) === id);
    }

    function selectedPlan() {
        const id = document.getElementById('kassaPlan').value;
        return planTemplates?.find(t => String(t.id) === id);
    }

    function syncStudentHint() {
        const student = selectedStudent();
        const plan = selectedPlan();
        const hint = document.getElementById('kassaStudentHint');
        const amount = document.getElementById('kassaAmount');

        if (plan) {
            amount.value = plan.amount;
            hint.textContent = `Seçilmiş plan: ${plan.name} · ${money(plan.amount)} ${plan.currency} · ${periodLabel(plan)}`;
            return;
        }

        amount.value = student?.amount ?? '';

        if (!student) { hint.textContent = ''; return; }

        hint.textContent = student.amount
            ? `Mövcud plan: ${money(student.amount)} ${student.currency}` +
              (student.nextDueOn ? ` · növbəti ödəniş: ${student.nextDueOn}` : '')
            : 'Bu tələbənin ödəniş planı yoxdur — yuxarıdan plan seçin.';
    }

    async function openPayment() {
        document.getElementById('kassaPaidOn').value = today();
        document.getElementById('kassaNote').value = '';
        document.getElementById('kassaStudentHint').textContent = '';

        if (!students || !planTemplates) {
            const [s, p] = await Promise.all([
                students ? Promise.resolve(students) : AdminUI.get('/kassa/students'),
                planTemplates ? Promise.resolve(planTemplates) : AdminUI.get('/kassa/plans')
            ]);
            students = s;
            planTemplates = p;
        }

        fillStudentSelect();
        fillPlanSelect();

        document.getElementById('kassaStudent').value = '';
        document.getElementById('kassaPlan').value = '';
        document.getElementById('kassaAmount').value = '';

        AdminUI.syncSelect(document.getElementById('kassaStudent'));
        AdminUI.syncSelect(document.getElementById('kassaPlan'));

        AdminUI.openModal('kassaPaymentModal');
    }

    async function removePayment(id) {
        if (!await AdminUI.confirmDelete('Bu ödəniş silinsin? Planın vade tarixi yenidən hesablanacaq.')) return;

        const res = await AdminUI.del(`/kassa/payments/${id}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); location.reload(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    document.getElementById('kassaStudent')?.addEventListener('change', syncStudentHint);
    document.getElementById('kassaPlan')?.addEventListener('change', syncStudentHint);

    document.getElementById('kassaPaymentForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();

        const student = selectedStudent();
        const plan = selectedPlan();
        const amount = Number(document.getElementById('kassaAmount').value);
        const paidOn = document.getElementById('kassaPaidOn').value || today();

        if (!student) { AdminUI.error('Tələbə seçin'); return; }
        if (!(amount > 0)) { AdminUI.error('Məbləğ sıfırdan böyük olmalıdır'); return; }
        if (!plan && !student.amount) { AdminUI.error('Bu tələbənin ödəniş planı yoxdur — plan seçin'); return; }

        if (plan) {
            const nextDueOn = plan.frequency === 'custom'
                ? addDays(paidOn, plan.intervalDays || 30)
                : addMonths(paidOn, 1);

            const planRes = await AdminUI.postJson('/kassa/plan', {
                studentId: student.id,
                joinedOn: paidOn,
                nextDueOn,
                amount: plan.amount,
                currency: plan.currency,
                frequency: plan.frequency,
                intervalDays: plan.intervalDays,
                note: plan.note
            });

            if (!planRes.ok) { AdminUI.error(AdminUI.errText(planRes)); return; }
        }

        const res = await AdminUI.postJson('/kassa/payments', {
            studentId: student.id,
            paidOn,
            amount,
            note: document.getElementById('kassaNote').value.trim()
        });

        if (res.ok) {
            AdminUI.notify(L["Js_Saved"]);
            AdminUI.closeModal('kassaPaymentModal');
            location.reload();
        } else {
            AdminUI.error(AdminUI.errText(res));
        }
    });

    document.getElementById('kassaCurrency')?.addEventListener('change', (e) => {
        currency = e.target.value;
        renderStats();
        renderMonths();
    });

    document.addEventListener('DOMContentLoaded', () => {
        renderStats();
        renderMonths();
        renderRecent();
    });

    return { openPayment, removePayment, applyRange, clearRange };
})();
