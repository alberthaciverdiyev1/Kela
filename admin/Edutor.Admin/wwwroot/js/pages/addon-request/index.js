window.AddonRequestPage = (function () {
    const kindLabels = { students: 'Tələbə', groups: 'Qrup', video: 'Video saatı' };
    const modeLabels = { one_time: 'Birdəfəlik', monthly: 'Aylıq' };

    let status = '';

    const statusBadge = (value) => ({
        pending: AdminUI.badge('Gözləyir', 'warning'),
        approved: AdminUI.badge('Təsdiqlənib', 'success'),
        rejected: AdminUI.badge('Rədd edilib', 'danger')
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
                { html: modeLabels[r.mode] || r.mode },
                { html: `<span class="font-semibold">${r.amount} ${r.currency}</span>` },
                { html: AdminUI.escapeHtml(r.requestedByName || '—') },
                { html: date(r.createdAt) },
                { html: statusBadge(r.status) },
                { html: actions(r), cls: 'text-right' }
            );

            return cells;
        }), 9, { empty: 'Müraciət yoxdur' });
    }

    function actions(r) {
        if (r.status === 'pending') {
            return `<div class="flex justify-end gap-1">
                <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-primary hover:bg-primary-light" onclick="AddonRequestPage.approve(${r.id})">Təsdiq</button>
                <button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="AddonRequestPage.reject(${r.id})">Rədd</button>
            </div>`;
        }

        if (r.status === 'approved' && r.mode === 'monthly' && r.addonId) {
            return `<button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="AddonRequestPage.cancelAddon(${r.addonId})">Dayandır</button>`;
        }

        return `<span class="text-xs text-slate-400">${AdminUI.escapeHtml(r.reviewedByName || '')}</span>`;
    }

    async function approve(id) {
        const result = await AdminUI.swal({
            title: 'Müraciəti təsdiqlə',
            text: 'Təsdiqdən sonra paket dərhal tətbiq olunacaq.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Təsdiqlə',
            cancelButtonText: L["Common_Cancel"]
        });

        if (!result.isConfirmed) return;

        const res = await AdminUI.post(`/addon-request/${id}/approve`, {});
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    async function reject(id) {
        const result = await AdminUI.swal({
            title: 'Müraciəti rədd et',
            input: 'text',
            inputPlaceholder: 'Səbəb (istəyə bağlı)',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Rədd et',
            cancelButtonText: L["Common_Cancel"],
            customClass: Object.assign({}, AdminUI.SWAL_BASE.customClass, { confirmButton: 'admin-button-danger' })
        });

        if (!result.isConfirmed) return;

        const res = await AdminUI.post(`/addon-request/${id}/reject`, { note: result.value || '' });
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); load(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    async function cancelAddon(addonId) {
        if (!await AdminUI.confirmDelete('Aylıq paket dayandırılsın? Növbəti aylarda limitə əlavə olunmayacaq.')) return;

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
