window.GroupRequestPage = (function () {
    let status = '';

    const statusBadge = (value) => ({
        pending: AdminUI.badge(L["Sub_StatusWaiting"], "warning"),
        approved: AdminUI.badge(L["Sub_StatusApproved"], "success"),
        rejected: AdminUI.badge(L["Sub_StatusRejected"], "danger")
    }[value] || value);

    const date = (value) => value ? new Date(value).toLocaleDateString(window.LANG || 'az') : '—';

    async function load() {
        const query = status ? `?status=${encodeURIComponent(status)}` : '';
        const rows = await AdminUI.get(`/group-request/list${query}`) || [];

        AdminUI.table('groupRequestRows', rows.map(r => [
            { html: `<a href="/group/${r.groupId}/manage" class="font-semibold text-primary hover:underline">${AdminUI.escapeHtml(r.groupName || `#${r.groupId}`)}</a>` },
            { html: `<a href="/student-panel/${r.studentId}" class="text-slate-700 hover:text-primary hover:underline">${AdminUI.escapeHtml(r.fullName || '—')}</a>` },
            { html: AdminUI.escapeHtml(r.email || '—') },
            { html: date(r.createdAt) },
            { html: statusBadge(r.status) },
            { html: actions(r), cls: 'text-right' }
        ]), 6, { empty: L["Requests_NoRequests"] });
    }

    function actions(r) {
        if (r.status !== 'pending') {
            return `<span class="text-xs text-slate-400">${date(r.decidedAt)}</span>`;
        }

        return `<div class="flex justify-end gap-1">
            <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-primary hover:bg-primary-light" onclick="GroupRequestPage.approve(${r.groupId}, ${r.id})">${L["Requests_Approve"]}</button>
            <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="GroupRequestPage.reject(${r.groupId}, ${r.id})">${L["Requests_Reject"]}</button>
        </div>`;
    }

    async function decide(groupId, requestId, action) {
        const approve = action === 'approve';

        const result = await AdminUI.swal({
            title: approve ? L["GroupRequest_ApproveTitle"] : L["GroupRequest_RejectTitle"],
            text: approve
                ? L["GroupRequest_ApproveText"]
                : L["GroupRequest_RejectText"],
            icon: approve ? 'question' : 'warning',
            showCancelButton: true,
            confirmButtonText: approve ? L["Group_JoinApprove"] : L["Group_JoinReject"],
            cancelButtonText: L["Common_Cancel"],
            customClass: approve
                ? undefined
                : Object.assign({}, AdminUI.SWAL_BASE.customClass, { confirmButton: 'admin-button-danger' })
        });

        if (!result.isConfirmed) return;

        const res = await AdminUI.post(`/group-request/${groupId}/${requestId}/${action}`, {});
        if (res.ok) {
            AdminUI.notify(L["Js_Saved"]);
            load();
            if (window.refreshGroupJoinBadge) window.refreshGroupJoinBadge();
        } else {
            AdminUI.error(AdminUI.errText(res));
        }
    }

    const approve = (groupId, requestId) => decide(groupId, requestId, 'approve');
    const reject = (groupId, requestId) => decide(groupId, requestId, 'reject');

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

    return { load, approve, reject };
})();
