window.CashboxPage = (function () {
    const data = window.CASHBOX || { currencies: [], monthly: [], recent: [] };
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
            ? L["Common_NDays"].replace('{0}', template.intervalDays || 30)
            : L["Cashbox_Monthly"];
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

        const deltaBox = document.getElementById('cashboxDelta');
        if (!s.lastMonth) {
            deltaBox.textContent = s.thisMonth ? L["Cashbox_NoLastMonth"] : '';
            return;
        }

        const pct = Math.round((s.thisMonth - s.lastMonth) * 100 / s.lastMonth);
        deltaBox.textContent = L["Cashbox_VsLastMonth"].replace('{0}', `${pct > 0 ? '+' : ''}${pct}`);
        deltaBox.className = `text-sm font-semibold ${pct >= 0 ? 'text-primary' : 'text-danger'}`;
    }

    function renderMonths() {
        const rows = data.monthly.filter(m => m.currency === currency);
        const box = document.getElementById('cashboxMonths');
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
        const box = document.getElementById('cashboxRecent');

        box.innerHTML = rows.length
            ? rows.map(r => `<tr>
                    <td class="whitespace-nowrap text-slate-500">${r.paidOn}</td>
                    <td class="font-medium text-slate-800">${esc(r.student)}</td>
                    <td class="whitespace-nowrap text-right font-semibold text-slate-800">${amountText(r.amount, r.currency)}</td>
                    <td class="text-slate-500">${esc(r.note || '—')}</td>
                    <td class="text-right">
                        <button type="button" class="admin-row-action is-danger" data-action="CashboxPage.removePayment" data-args='[${r.id}]'>Sil</button>
                    </td>
                </tr>`).join('')
            : `<tr><td colspan="5" class="p-4 text-sm text-slate-400">${emptyText}</td></tr>`;
    }

    function renderRecent() {
        renderPayments(data.recent, L["Cashbox_NoPaymentsYet"]);
    }

    async function applyRange() {
        const from = document.getElementById('cashboxFrom').value;
        const to = document.getElementById('cashboxTo').value;

        const query = new URLSearchParams();
        if (from) query.set('from', from);
        if (to) query.set('to', to);

        const rows = await AdminUI.get('/cashbox/payments' + (query.toString() ? '?' + query : ''));
        renderPayments(rows, L["Cashbox_NoPaymentsRange"]);

        const totals = rows.reduce((acc, r) => {
            acc[r.currency] = (acc[r.currency] || 0) + Number(r.amount || 0);
            return acc;
        }, {});

        const text = Object.entries(totals).map(([code, total]) => amountText(total, code)).join(' · ');
        document.getElementById('cashboxRangeTotal').textContent = text ? L["Cashbox_RangeTotal"].replace('{0}', text) : '';
    }

    function clearRange() {
        ['cashboxFrom', 'cashboxTo'].forEach(id => {
            const el = document.getElementById(id);
            el.value = '';
            if (window.DatePicker) DatePicker.sync(el);
        });
        document.getElementById('cashboxRangeTotal').textContent = '';
        renderRecent();
    }

    function fillStudentSelect() {
        const select = document.getElementById('cashboxStudent');

        select.innerHTML = '<option value="">' + L["Cashbox_SelectStudent"] + '</option>' + students.map(s => {
            const plan = s.amount ? ` — ${money(s.amount)} ${s.currency}` : '';
            return `<option value="${s.id}">${esc(s.name)}${plan}</option>`;
        }).join('');

        AdminUI.syncSelect(select);
    }

    // Tələbənin mövcud planı varsa siyahının başında "Mövcud plan" kimi göstərilir
    // və avtomatik seçilir; yeni plan seçilsə, ödənişdən əvvəl təyin olunur.
    function refreshPlanOptions() {
        const student = selectedStudent();
        const select = document.getElementById('cashboxPlan');

        let html = '';

        if (student?.amount) {
            const period = student.frequency === 'custom'
                ? L["Common_NDays"].replace('{0}', student.intervalDays || 30)
                : L["Cashbox_Monthly"];
            html += '<option value="current">' + L["Cashbox_CurrentPlanOption"].replace('{0}', money(student.amount)).replace('{1}', student.currency).replace('{2}', period) + '</option>';
        }

        html += '<option value="">' + L["Cashbox_NoPlanOption"] + '</option>' + planTemplates.map(t =>
            `<option value="${t.id}">${esc(t.name)} — ${money(t.amount)} ${t.currency} (${periodLabel(t)})</option>`
        ).join('');

        select.innerHTML = html;
        select.value = student?.amount ? 'current' : '';

        AdminUI.syncSelect(select);
    }

    function selectedStudent() {
        const id = document.getElementById('cashboxStudent').value;
        return students?.find(s => String(s.id) === id);
    }

    function selectedPlan() {
        const id = document.getElementById('cashboxPlan').value;
        return planTemplates?.find(t => String(t.id) === id);
    }

    function keepsCurrentPlan() {
        return document.getElementById('cashboxPlan').value === 'current';
    }

    function syncStudentHint() {
        const student = selectedStudent();
        const plan = selectedPlan();
        const hint = document.getElementById('cashboxStudentHint');
        const amount = document.getElementById('cashboxAmount');

        if (plan) {
            amount.value = plan.amount;
            hint.textContent = L["Cashbox_SelectedPlanHint"].replace('{0}', plan.name).replace('{1}', money(plan.amount)).replace('{2}', plan.currency).replace('{3}', periodLabel(plan));
            return;
        }

        if (keepsCurrentPlan() && student) {
            amount.value = student.amount;
            hint.textContent = L["Cashbox_CurrentPlanHint"].replace('{0}', money(student.amount)).replace('{1}', student.currency) +
                (student.nextDueOn ? L["Cashbox_NextDueSuffix"].replace('{0}', student.nextDueOn) : '');
            return;
        }

        amount.value = student?.amount ?? '';

        if (!student) { hint.textContent = ''; return; }

        hint.textContent = student.amount
            ? L["Cashbox_CurrentPlanHint"].replace('{0}', money(student.amount)).replace('{1}', student.currency) +
              (student.nextDueOn ? L["Cashbox_NextDueSuffix"].replace('{0}', student.nextDueOn) : '')
            : L["Cashbox_NoPlanForStudent"];
    }

    async function openPayment() {
        document.getElementById('cashboxPaidOn').value = today();
        document.getElementById('cashboxNote').value = '';
        document.getElementById('cashboxStudentHint').textContent = '';

        if (!students || !planTemplates) {
            const [s, p] = await Promise.all([
                students ? Promise.resolve(students) : AdminUI.get('/cashbox/students'),
                planTemplates ? Promise.resolve(planTemplates) : AdminUI.get('/cashbox/plans')
            ]);
            students = s;
            planTemplates = p;
        }

        fillStudentSelect();
        refreshPlanOptions();

        document.getElementById('cashboxStudent').value = '';
        document.getElementById('cashboxAmount').value = '';

        AdminUI.syncSelect(document.getElementById('cashboxStudent'));
        AdminUI.syncSelect(document.getElementById('cashboxPlan'));

        AdminUI.openModal('cashboxPaymentModal');
    }

    async function removePayment(id) {
        if (!await AdminUI.confirmDelete(L["Cashbox_DeleteConfirm"])) return;

        const res = await AdminUI.del(`/cashbox/payments/${id}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); location.reload(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    document.getElementById('cashboxStudent')?.addEventListener('change', () => {
        refreshPlanOptions();
        syncStudentHint();
    });
    document.getElementById('cashboxPlan')?.addEventListener('change', syncStudentHint);

    document.getElementById('cashboxPaymentForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();

        const student = selectedStudent();
        const plan = selectedPlan();
        const amount = Number(document.getElementById('cashboxAmount').value);
        const paidOn = document.getElementById('cashboxPaidOn').value || today();

        if (!student) { AdminUI.error(L["Cashbox_StudentRequired"]); return; }
        if (!(amount > 0)) { AdminUI.error(L["Cashbox_AmountPositive"]); return; }
        if (!plan && !keepsCurrentPlan() && !student.amount) {
            AdminUI.error(L["Cashbox_NoPlanSelected"]);
            return;
        }

        if (plan) {
            const nextDueOn = plan.frequency === 'custom'
                ? addDays(paidOn, plan.intervalDays || 30)
                : addMonths(paidOn, 1);

            const planRes = await AdminUI.postJson('/cashbox/plan', {
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

        const res = await AdminUI.postJson('/cashbox/payments', {
            studentId: student.id,
            paidOn,
            amount,
            note: document.getElementById('cashboxNote').value.trim()
        });

        if (res.ok) {
            AdminUI.notify(L["Js_Saved"]);
            AdminUI.closeModal('cashboxPaymentModal');
            location.reload();
        } else {
            AdminUI.error(AdminUI.errText(res));
        }
    });

    document.getElementById('cashboxCurrency')?.addEventListener('change', (e) => {
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
