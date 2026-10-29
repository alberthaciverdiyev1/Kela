window.GroupRequestPage = (function () {
    let status = '';

    const statusBadge = (value) => ({
        pending: AdminUI.badge('Gözləyir', 'warning'),
        approved: AdminUI.badge('Təsdiqlənib', 'success'),
        rejected: AdminUI.badge('Rədd edilib', 'danger')
    }[value] || value);

    const date = (value) => value ? new Date(value).toLocaleDateString('az-AZ') : '—';

    async function load() {
        const query = status ? `?status=${encodeURIComponent(status)}` : '';
        const rows = await AdminUI.get(`/group-request/list${query}`) || [];

        AdminUI.table('groupRequestRows', rows.map(r => [
            { html: `<a href="/group/${r.groupId}/manage" class="font-semibold text-primary hover:underline">${AdminUI.escapeHtml(r.groupName || `#${r.groupId}`)}</a>` },
            { html: `<a href="/students/${r.studentId}/detail" class="text-slate-700 hover:text-primary hover:underline">${AdminUI.escapeHtml(r.fullName || '—')}</a>` },
            { html: AdminUI.escapeHtml(r.email || '—') },
            { html: date(r.createdAt) },
            { html: statusBadge(r.status) },
            { html: actions(r), cls: 'text-right' }
        ]), 6, { empty: 'Müraciət yoxdur' });
    }

    function actions(r) {
        if (r.status !== 'pending') {
            return `<span class="text-xs text-slate-400">${date(r.decidedAt)}</span>`;
        }

        return `<div class="flex justify-end gap-1">
            <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-primary hover:bg-primary-light" onclick="GroupRequestPage.approve(${r.groupId}, ${r.id})">Təsdiq</button>
            <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="GroupRequestPage.reject(${r.groupId}, ${r.id})">Rədd</button>
        </div>`;
    }

    async function decide(groupId, requestId, action) {
        const approve = action === 'approve';

        const result = await AdminUI.swal({
            title: approve ? 'Müraciət təsdiqlənsin?' : 'Müraciət rədd edilsin?',
            text: approve
                ? 'Şagird dərhal qrupa üzv olacaq.'
                : 'Şagird qrupa əlavə olunmayacaq.',
            icon: approve ? 'question' : 'warning',
            showCancelButton: true,
            confirmButtonText: approve ? 'Təsdiqlə' : 'Rədd et',
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
