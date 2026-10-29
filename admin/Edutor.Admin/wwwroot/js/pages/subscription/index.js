window.SubscriptionPage = (function () {
    // Superadmin başqa tenant-ın abunəliyinə baxırsa seçim URL ilə daşınır.
    function switchTenant(el) {
        if (!el || !el.value) return;
        location.href = '/subscription?tenant_id=' + encodeURIComponent(el.value);
    }

    // Ortaq select komponenti açılışda boş placeholder seçir — serverdən gələn
    // tenant-ı geri qaytarırıq ki, səhifə məlumatı ilə seçim uyğun olsun.
    document.addEventListener('DOMContentLoaded', () => {
        const select = document.getElementById('subscriptionTenant');
        if (!select) return;

        const marked = select.querySelector('option[selected]');
        if (marked) select.value = marked.value;
        if (window.AdminUI && AdminUI.syncSelect) AdminUI.syncSelect(select);
    });

    function selectedTenantId() {
        return document.getElementById('subscriptionTenant')?.value || '';
    }

    async function addAddon(kind) {
        const labels = { students: 'tələbə', groups: 'qrup', video: 'saat' };

        const ctx = window.SUBSCRIPTION_CONTEXT;
        const result = await AdminUI.swal({
            title: 'Limit artırımı',
            text: `Neçə ${labels[kind] || 'vahid'} əlavə edilsin?`,
            input: 'number',
            inputValue: 1,
            inputAttributes: { min: 1, max: 10000 },
            showCancelButton: true,
            confirmButtonText: 'Davam et',
            cancelButtonText: L["Common_Cancel"]
        });

        if (!result.isConfirmed) return;

        // Aylıq paket planla birlikdə ödənilir — plan icazə vermirsə seçim yoxdur.
        const modeOptions = ctx?.billAddonsWithPlan
            ? { one_time: 'Birdəfəlik', monthly: 'Aylıq (planla birlikdə)' }
            : { one_time: 'Birdəfəlik' };

        const modeResult = await AdminUI.swal({
            title: 'Ödəniş forması',
            input: 'select',
            inputOptions: modeOptions,
            inputValue: 'one_time',
            showCancelButton: true,
            confirmButtonText: 'Əlavə et',
            cancelButtonText: L["Common_Cancel"]
        });

        if (!modeResult.isConfirmed) return;

        const payload = { kind, quantity: result.value || 1, mode: modeResult.value || 'one_time' };
        payload.payment_timing = payload.mode === 'monthly' ? 'with_plan' : 'now';

        // Plan limit ödənişini planla birlikdə qəbul edirsə, superadmin vaxtı seçir.
        if (payload.mode === 'one_time' && ctx?.billAddonsWithPlan) {
            const timing = await AdminUI.swal({
                title: 'Ödəniş vaxtı',
                input: 'select',
                inputOptions: { now: 'İndi ödəniş', with_plan: 'Ay sonunda planla birlikdə' },
                inputValue: 'now',
                showCancelButton: true,
                confirmButtonText: 'Əlavə et',
                cancelButtonText: L["Common_Cancel"]
            });

            if (!timing.isConfirmed) return;
            payload.payment_timing = timing.value || 'now';
        }

        const tenantId = selectedTenantId();
        if (tenantId) payload.tenant_id = tenantId;

        const res = await AdminUI.post('/subscription/addons', payload);

        if (res.ok) { AdminUI.notify(L["Js_Saved"]); loadAddons(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    // ---- Limit artırma müraciətləri ----

    const kindLabels = { students: 'Tələbə', groups: 'Qrup', video: 'Video saatı' };
    const modeLabels = { one_time: 'Birdəfəlik', monthly: 'Aylıq' };
    const timingLabels = { now: 'İndi', with_plan: 'Planla birlikdə' };
    const paymentBadge = (a) => a.paidAt
        ? AdminUI.badge('Ödənilib', 'success')
        : AdminUI.badge('Gözləyir', 'warning');
    const requestBadge = (value) => ({
        pending: AdminUI.badge('Gözləyir', 'warning'),
        approved: AdminUI.badge('Təsdiqlənib', 'success'),
        rejected: AdminUI.badge('Rədd edilib', 'danger')
    }[value] || value);

    const shortDate = (value) => value ? new Date(value).toLocaleDateString('az-AZ') : '—';

    // Superadmin başqa tenant-a baxırsa sorğulara tenant_id əlavə olunur.
    function tenantQuery() {
        const id = selectedTenantId();
        return id ? `?tenant_id=${encodeURIComponent(id)}` : '';
    }

    async function loadRequests() {
        const rows = await AdminUI.get(`/subscription/addon-requests${tenantQuery()}`) || [];

        AdminUI.table('subscriptionAddonRequests', rows.map(r => [
            { html: shortDate(r.createdAt) },
            { html: kindLabels[r.kind] || r.kind },
            { html: r.quantity },
            { html: timingLabels[r.paymentTiming] || r.paymentTiming },
            { html: requestBadge(r.status) },
            { html: AdminUI.escapeHtml(r.reviewNote || r.note || '—') }
        ]), 6, { empty: 'Müraciət yoxdur' });
    }

    async function loadAddons() {
        const rows = await AdminUI.get(`/subscription/addons${tenantQuery()}`) || [];

        AdminUI.table('subscriptionAddons', rows.map(a => [
            { html: a.period },
            { html: kindLabels[a.kind] || a.kind },
            { html: a.quantity },
            { html: modeLabels[a.mode] || a.mode },
            { html: `${timingLabels[a.paymentTiming] || a.paymentTiming}<br>${paymentBadge(a)}` },
            { html: `<span class="font-semibold">${a.amount} ${a.currency}</span>` },
            {
                html: actions(a),
                cls: 'text-right'
            }
        ]), 7, { empty: 'Əlavə paket yoxdur' });
    }

    function actions(a) {
        const cancel = a.mode === 'monthly'
            ? `<button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="SubscriptionPage.cancelAddon(${a.id})">Dayandır</button>`
            : '';
        const paid = !a.paidAt
            ? `<button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-primary hover:bg-primary-light" onclick="SubscriptionPage.markPaid(${a.id})">Ödənişi qeyd et</button>`
            : '';

        return `<div class="flex justify-end gap-1">${paid}${cancel}</div>`;
    }

    async function markPaid(id) {
        const result = await AdminUI.swal({
            title: 'Ödəniş qeydə alınsın?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Qeyd et',
            cancelButtonText: L["Common_Cancel"]
        });

        if (!result.isConfirmed) return;

        const res = await AdminUI.post(`/addon-request/addons/${id}/paid`, {});
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); loadAddons(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    async function cancelAddon(id) {
        if (!await AdminUI.confirmDelete('Aylıq paket dayandırılsın? Növbəti aylarda limitə əlavə olunmayacaq.')) return;

        const res = await AdminUI.del(`/addon-request/addons/${id}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); loadAddons(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    async function sendRequest(form) {
        const button = form.querySelector('button[type="submit"]');
        if (button) { button.disabled = true; button.classList.add('is-loading'); }

        const fd = new FormData(form);
        const res = await AdminUI.postForm('/subscription/addon-requests', fd);

        if (button) { button.disabled = false; button.classList.remove('is-loading'); }

        if (res.ok) {
            AdminUI.notify('Müraciət göndərildi. Administrator təsdiqindən sonra limit artırılacaq.');
            loadRequests();
        } else {
            AdminUI.error(AdminUI.errText(res));
        }
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('form[data-addon-request]').forEach(form => {
            form.addEventListener('submit', (e) => {
                e.preventDefault();
                sendRequest(form);
            });
        });

        if (document.getElementById('subscriptionAddonRequests')) loadRequests();
        if (document.getElementById('subscriptionAddons')) loadAddons();
    });

    return { switchTenant, addAddon, cancelAddon, markPaid, sendRequest };
})();
