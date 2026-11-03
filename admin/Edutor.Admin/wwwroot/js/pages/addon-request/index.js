window.AddonRequestPage = (function () {
    const kindLabels = { students: L["Common_Student"], groups: L["Common_Group"], video: L["Sub_KindVideoHours"] };
    const timingLabels = { now: L["Sub_TimingNow"], with_plan: L["Sub_WithPlanShort"] };
    const isSuperAdmin = !!(window.ADMIN && ADMIN.isSuperAdmin);

    let status = '';

    const statusBadge = (value) => ({
        pending: AdminUI.badge(L["Sub_StatusWaiting"], "warning"),
        approved: AdminUI.badge(L["Sub_StatusApproved"], "success"),
        rejected: AdminUI.badge(L["Sub_StatusRejected"], "danger")
    }[value] || value);

    const date = (value) => value ? new Date(value).toLocaleDateString('az-AZ') : '—';

    async function load() {
        const query = status ? `?status=${encodeURIComponent(status)}` : '';
        const rows = await AdminUI.get(`/addon-request/list${query}`) || [];

        const hasTenant = document.querySelector('#addonRequestRows')
            && document.querySelectorAll('thead th').length > 8;

        AdminUI.table('addonRequestRows', rows.map(r => {
            const cells = [];

            if (hasTenant) cells.push({ html: AdminUI.escapeHtml(r.tenantName || `#${r.tenantId}`) });

            cells.push(
                { html: kindLabels[r.kind] || r.kind },
                { html: r.quantity },
                { html: timingLabels[r.paymentTiming] || r.paymentTiming },
                { html: `<span class="font-semibold">${r.amount} ${r.currency}</span>` },
                { html: AdminUI.escapeHtml(r.requestedByName || '—') },
                { html: date(r.createdAt) },
                { html: statusBadge(r.status) },
                { html: actions(r), cls: 'text-right' }
            );

            return cells;
        }), 9, { empty: L["Requests_NoRequests"] });
    }

    function actions(r) {
        if (!isSuperAdmin) {
            return `<span class="text-xs text-slate-400">${AdminUI.escapeHtml(r.reviewedByName || '')}</span>`;
        }

        if (r.status === 'pending') {
            return `<div class="flex justify-end gap-1">
                <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-primary hover:bg-primary-light" onclick="AddonRequestPage.approve(${r.id})">${L["Requests_Approve"]}</button>
                <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="AddonRequestPage.reject(${r.id})">${L["Requests_Reject"]}</button>
            </div>`;
        }

        if (r.status === 'approved' && r.mode === 'monthly' && r.addonId) {
            return `<button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="AddonRequestPage.cancelAddon(${r.addonId})">${L["Sub_Stop"]}</button>`;
        }

        return `<span class="text-xs text-slate-400">${AdminUI.escapeHtml(r.reviewedByName || '')}</span>`;
    }

    async function approve(id) {
        // Paketin formasını (birdəfəlik/aylıq) superadmin müəyyən edir — ödəniş
        // də buna görə hesablanır. Aylıq paket planla birlikdə ödənilir.
        const result = await AdminUI.swal({
            title: L["Addon_ApproveTitle"],
            text: L["Addon_ApproveText"],
            input: 'select',
            inputOptions: { one_time: L["Limit_OneTime"], monthly: L["Sub_ModeMonthlyWithPlan"] },
            inputValue: 'one_time',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: L["Group_JoinApprove"],
            cancelButtonText: L["Common_Cancel"]
        });

        if (!result.isConfirmed) return;

        const res = await AdminUI.post(`/addon-request/${id}/approve`, { mode: result.value || 'one_time' });
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    async function reject(id) {
        const result = await AdminUI.swal({
            title: L["Addon_RejectTitle"],
            input: 'text',
            inputPlaceholder: L["Addon_RejectReason"],
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: L["Group_JoinReject"],
            cancelButtonText: L["Common_Cancel"],
            customClass: Object.assign({}, AdminUI.SWAL_BASE.customClass, { confirmButton: 'admin-button-danger' })
        });

        if (!result.isConfirmed) return;

        const res = await AdminUI.post(`/addon-request/${id}/reject`, { note: result.value || '' });
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    async function cancelAddon(addonId) {
        if (!await AdminUI.confirmDelete(L["Sub_CancelAddonConfirm"])) return;

        const res = await AdminUI.del(`/addon-request/addons/${addonId}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('[data-filter]').forEach(btn => {
            btn.addEventListener('click', () => {
                status = btn.getAttribute('data-filter');
                document.querySelectorAll('[data-filter]').forEach(b => b.classList.remove('ring-2', 'ring-primary/40'));
                btn.classList.add('ring-2', 'ring-primary/40');
                load();
            });
        });

        load();
    });

    return { load, approve, reject, cancelAddon };
})();
