// Superadmin abunəlik idarəetməsi: ödəniş vaxtı yaxınlaşan tenantları göstərir,
// planı dəyişməyə, bugündən yeni dövr açmağa və müddəti uzatmağa imkan verir.
(function () {
    const data = window.PLATFORM_FINANCE || {};
    const tenants = data.tenants || [];

    const el = (id) => document.getElementById(id);
    const pad = (n) => String(n).padStart(2, '0');
    const fmt = (d) => `${pad(d.getDate())}.${pad(d.getMonth() + 1)}.${d.getFullYear()}`;

    let filter = 'all';
    let search = '';
    let plans = [];
    let current = null;

    const STATUS_TONE = { active: 'success', pending: 'warning', suspended: 'neutral', expired: 'danger' };
    const statusText = (status) => L['Revenue_Status' + status.charAt(0).toUpperCase() + status.slice(1)] || status;

    function isExpiring(t) {
        return t.planId != null && t.daysLeft != null && t.daysLeft >= 0 && t.daysLeft <= 30;
    }

    function filtered() {
        const needle = search.trim().toLowerCase();

        return tenants.filter(t => {
            if (filter === 'expiring' && !isExpiring(t)) return false;
            if (filter === 'expired' && !(t.planId != null && t.daysLeft != null && t.daysLeft < 0)) return false;
            if (filter === 'none' && t.planId != null) return false;

            if (!needle) return true;
            return (t.name || '').toLowerCase().includes(needle)
                || (t.plan || '').toLowerCase().includes(needle);
        }).sort((a, b) => {
            const ax = a.expiresAt ? new Date(a.expiresAt).getTime() : Number.MAX_SAFE_INTEGER;
            const bx = b.expiresAt ? new Date(b.expiresAt).getTime() : Number.MAX_SAFE_INTEGER;
            return ax - bx;
        });
    }

    function daysCell(t) {
        if (t.planId == null) return '<span class="text-slate-400">—</span>';
        if (t.daysLeft == null) return `<span class="text-slate-400">${L["SubMgmt_Unlimited"]}</span>`;

        const days = t.daysLeft;
        const tone = days < 0 ? 'text-danger' : days <= 7 ? 'text-danger' : days <= 30 ? 'text-amber-600' : 'text-slate-600';
        return `<span class="font-semibold ${tone}">${days} ${L["SubMgmt_DaysUnit"]}</span>`;
    }

    function render() {
        const body = el('pfSubscriptions');
        if (!body) return;

        const rows = filtered();
        if (rows.length === 0) {
            body.innerHTML = `<tr><td colspan="7" class="p-4 text-center text-sm text-slate-400">${L["SubMgmt_Empty"]}</td></tr>`;
            return;
        }

        body.innerHTML = rows.map(t => `
            <tr>
                <td class="font-semibold text-slate-800">${AdminUI.escapeHtml(t.name)}</td>
                <td class="text-slate-600">${t.plan ? AdminUI.escapeHtml(t.plan) : `<span class="text-slate-400">${L["Revenue_NoPlan"]}</span>`}</td>
                <td><span class="badge badge-${STATUS_TONE[t.status] || 'neutral'}">${AdminUI.escapeHtml(statusText(t.status))}</span></td>
                <td class="whitespace-nowrap text-slate-500">${t.expiresAt ? fmt(new Date(t.expiresAt)) : '—'}</td>
                <td class="whitespace-nowrap text-right">${daysCell(t)}</td>
                <td class="whitespace-nowrap text-right text-slate-700">${Number(t.price || 0).toFixed(2)} ${t.currency || ''}</td>
                <td class="text-right">
                    <button type="button" class="admin-button admin-button-secondary h-8 px-3 text-xs"
                            data-action="PlatformSubscriptions.openRenew" data-args='[${t.id}]'>${L["SubMgmt_Renew"]}</button>
                </td>
            </tr>`).join('');
    }

    async function loadPlans() {
        if (plans.length > 0) return plans;
        plans = await AdminUI.get('/tenant/plan-list') || [];

        el('renewPlan').innerHTML = plans
            .map(p => `<option value="${p.id}">${AdminUI.escapeHtml(p.name)} — ${p.monthlyPrice} ${p.currency}/ay</option>`)
            .join('');

        return plans;
    }

    function previewDate() {
        const mode = el('renewMode').value;
        const months = parseInt(el('renewMonths').value, 10) || 0;
        const days = parseInt(el('renewDays').value, 10) || 0;

        const now = new Date();
        let base = now;

        if (mode === 'extend' && current?.expiresAt) {
            const expires = new Date(current.expiresAt);
            if (expires > now) base = expires;
        }

        const result = new Date(base);
        result.setMonth(result.getMonth() + months);
        result.setDate(result.getDate() + days);

        return months === 0 && days === 0 ? null : result;
    }

    function updatePreview() {
        const target = previewDate();
        el('renewPreview').textContent = target ? fmt(target) : '—';
    }

    async function openRenew(tenantId) {
        current = tenants.find(t => t.id === tenantId);
        if (!current) return;

        await loadPlans();

        el('renewTenantId').value = tenantId;
        el('renewTenant').textContent = current.name;
        el('renewCurrent').textContent = [
            current.plan || L["Revenue_NoPlan"],
            current.expiresAt ? fmt(new Date(current.expiresAt)) : L["SubMgmt_Unlimited"]
        ].join(' · ');

        const select = el('renewPlan');
        select.value = current.planId ? String(current.planId) : (select.options[0]?.value || '');
        if (AdminUI.syncSelect) AdminUI.syncSelect(select);

        el('renewMode').value = 'extend';
        el('renewMonths').value = '1';
        el('renewDays').value = '0';
        el('renewError').classList.add('hidden');

        updatePreview();
        AdminUI.openModal('renewModal');
    }

    document.addEventListener('DOMContentLoaded', () => {
        render();

        const searchInput = el('subMgmtSearch');
        if (searchInput) {
            searchInput.addEventListener('input', () => { search = searchInput.value; render(); });
        }

        document.querySelectorAll('[data-sub-filter]').forEach(btn => {
            btn.addEventListener('click', () => {
                filter = btn.dataset.subFilter;

                document.querySelectorAll('[data-sub-filter]').forEach(b => {
                    const active = b === btn;
                    b.classList.toggle('bg-primary', active);
                    b.classList.toggle('text-white', active);
                    b.classList.toggle('text-slate-600', !active);
                    b.classList.toggle('hover:bg-slate-100', !active);
                });

                render();
            });
        });

        ['renewMode', 'renewMonths', 'renewDays'].forEach(id => {
            el(id)?.addEventListener('input', updatePreview);
            el(id)?.addEventListener('change', updatePreview);
        });

        el('renewForm')?.addEventListener('submit', async (e) => {
            e.preventDefault();

            const id = el('renewTenantId').value;
            const planId = el('renewPlan').value;
            const months = parseInt(el('renewMonths').value, 10) || 0;
            const days = parseInt(el('renewDays').value, 10) || 0;
            const box = el('renewError');

            if (!id || !planId) return;

            if (months === 0 && days === 0) {
                box.textContent = L["Common_RequiredFields"];
                box.classList.remove('hidden');
                return;
            }

            const fd = new FormData();
            fd.append('plan_id', planId);
            fd.append('mode', el('renewMode').value);
            fd.append('months', String(months));
            fd.append('days', String(days));

            const res = await AdminUI.putForm(`/platform-finance/${id}/subscription`, fd);

            if (!res.ok) {
                box.textContent = AdminUI.errText(res);
                box.classList.remove('hidden');
                return;
            }

            AdminUI.notify(L["Js_Saved"]);
            AdminUI.closeModal('renewModal');
            location.reload();
        });
    });

    window.PlatformSubscriptions = { openRenew };
})();
