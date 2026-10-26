window.KassaPage = (function () {
    const data = window.KASSA || { currencies: [], monthly: [], recent: [] };
    const esc = (v) => AdminUI.escapeHtml(v);

    let currency = data.currencies[0]?.currency || 'AZN';
    let students = null;

    const money = (value) => Number(value || 0).toLocaleString('az', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const amountText = (value, code) => `${money(value)} ${code}`;
    const today = () => new Date().toLocaleDateString('sv-SE'); // yyyy-MM-dd

    function statsFor(code) {
        return data.currencies.find(c => c.currency === code) || {};
    }

    function renderStats() {
        const s = statsFor(currency);

        document.getElementById('k-today').textContent = amountText(s.today, currency);
        document.getElementById('k-this').textContent = amountText(s.thisMonth, currency);
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

    function renderRecent() {
        const box = document.getElementById('kassaRecent');

        box.innerHTML = data.recent.length
            ? data.recent.map(r => `<tr>
                    <td class="whitespace-nowrap text-slate-500">${r.paidOn}</td>
                    <td class="font-medium text-slate-800">${esc(r.student)}</td>
                    <td class="whitespace-nowrap text-right font-semibold text-slate-800">${amountText(r.amount, r.currency)}</td>
                    <td class="text-slate-500">${esc(r.note || '—')}</td>
                    <td class="text-right">
                        <button type="button" class="admin-row-action is-danger" data-action="KassaPage.removePayment" data-args='[${r.id}]'>Sil</button>
                    </td>
                </tr>`).join('')
            : '<tr><td colspan="5" class="p-4 text-sm text-slate-400">Hələ ödəniş qeydə alınmayıb.</td></tr>';
    }

    async function openPayment() {
        document.getElementById('kassaPaidOn').value = today();
        document.getElementById('kassaAmount').value = '';
        document.getElementById('kassaNote').value = '';
        document.getElementById('kassaStudentHint').textContent = '';

        const select = document.getElementById('kassaStudent');
        AdminUI.openModal('kassaPaymentModal');

        if (!students) {
            select.innerHTML = '<option value="">Yüklənir...</option>';
            students = await AdminUI.get('/kassa/students');
        }

        select.innerHTML = '<option value="">Tələbə seçin</option>' + students.map(s => {
            const plan = s.amount ? ` — ${money(s.amount)} ${s.currency}` : '';
            return `<option value="${s.id}" data-amount="${s.amount ?? ''}">${esc(s.name)}${plan}</option>`;
        }).join('');
    }

    async function removePayment(id) {
        if (!await AdminUI.confirmDelete('Bu ödəniş silinsin? Planın vade tarixi yenidən hesablanacaq.')) return;

        const res = await AdminUI.del(`/kassa/payments/${id}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); location.reload(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    document.getElementById('kassaStudent')?.addEventListener('change', (e) => {
        const option = e.target.selectedOptions[0];
        const amount = option?.dataset.amount;

        if (amount) document.getElementById('kassaAmount').value = amount;

        const student = students?.find(s => String(s.id) === e.target.value);
        document.getElementById('kassaStudentHint').textContent = student?.nextDueOn
            ? `Növbəti ödəniş tarixi: ${student.nextDueOn}`
            : '';
    });

    document.getElementById('kassaPaymentForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();

        const studentId = Number(document.getElementById('kassaStudent').value);
        const amount = Number(document.getElementById('kassaAmount').value);

        if (!studentId || !(amount > 0)) { AdminUI.error('Tələbə və məbləğ tələb olunur'); return; }

        const res = await AdminUI.postJson('/kassa/payments', {
            studentId,
            paidOn: document.getElementById('kassaPaidOn').value || today(),
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

    return { openPayment, removePayment };
})();
