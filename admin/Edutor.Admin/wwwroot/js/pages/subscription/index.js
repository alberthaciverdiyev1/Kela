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

        const modeResult = await AdminUI.swal({
            title: 'Ödəniş forması',
            input: 'select',
            inputOptions: { one_time: 'Birdəfəlik', monthly: 'Aylıq' },
            inputValue: 'one_time',
            showCancelButton: true,
            confirmButtonText: 'Əlavə et',
            cancelButtonText: L["Common_Cancel"]
        });

        if (!modeResult.isConfirmed) return;

        const payload = { kind, quantity: result.value || 1, mode: modeResult.value || 'one_time' };
        const tenantId = selectedTenantId();
        if (tenantId) payload.tenant_id = tenantId;

        const res = await AdminUI.post('/subscription/addons', payload);

        if (res.ok) { AdminUI.notify(L["Js_Saved"]); loadAddons(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    function limitLabel(value) {
        return value > 0 ? String(value) : 'limitsiz';
    }

    function storageLabel(bytes) {
        return bytes > 0 ? Math.round(bytes / 1073741824) + ' GB' : 'limitsiz';
    }

    // Plan seçimi hazırda WhatsApp müraciəti kimi göndərilir (aktivləşdirmə əl ilə).
    async function choosePlan(planId) {
        const ctx = window.SUBSCRIPTION_CONTEXT;
        const plan = ctx?.plans?.find(p => p.id === Number(planId));
        if (!plan) return;

        const result = await AdminUI.swal({
            title: 'Plan seçimi',
            text: `«${plan.name}» planı üçün WhatsApp ilə müraciət göndərilsin?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Göndər',
            cancelButtonText: L["Common_Cancel"]
        });

        if (!result.isConfirmed) return;

        const lines = [
            'Salam! Abunəlik planı dəyişikliyi üçün müraciət:',
            '',
            `Müəssisə: ${ctx.tenantName || '—'}`,
            `Müraciət edən: ${ctx.userName || '—'}`,
            `E-poçt: ${ctx.userEmail || '—'}`,
            `Cari plan: ${ctx.currentPlanName || '—'}`,
            `Seçilmiş plan: ${plan.name} (${plan.code})`,
            `Qiymət: ${plan.price} ${plan.currency}/ay`,
            `Tələbə limiti: ${limitLabel(plan.maxStudents)}`,
            `Qrup limiti: ${limitLabel(plan.maxGroups)}`,
            `Video (aylıq): ${plan.videoMinutes > 0 ? Math.round(plan.videoMinutes / 60) + ' saat' : 'limitsiz'}`,
            `Yaddaş: ${storageLabel(plan.storageBytes)}`,
            `Maks. video keyfiyyəti: ${plan.maxVideoHeight > 0 ? plan.maxVideoHeight + 'p' : 'limitsiz'}`,
            `Tarix: ${new Date().toLocaleString('az')}`
        ];

        window.open(`https://wa.me/${ctx.whatsapp}?text=${encodeURIComponent(lines.join('\n'))}`, '_blank', 'noopener');
    }

    // ---- Limit artırma müraciətləri ----

    const kindLabels = { students: 'Tələbə', groups: 'Qrup', video: 'Video saatı' };
    const modeLabels = { one_time: 'Birdəfəlik', monthly: 'Aylıq' };
    const requestBadge = (value) => ({
        pending: AdminUI.badge('Gözləyir', 'warning'),
        approved: AdminUI.badge('Təsdiqlənib', 'success'),
        rejected: AdminUI.badge('Rədd edilib', 'danger')
    }[value] || value);

    const shortDate = (value) => value ? new Date(value).toLocaleDateString('az-AZ') : '—';

    async function loadRequests() {
        const rows = await AdminUI.get('/subscription/addon-requests') || [];

        AdminUI.table('subscriptionAddonRequests', rows.map(r => [
            { html: shortDate(r.createdAt) },
            { html: kindLabels[r.kind] || r.kind },
            { html: r.quantity },
            { html: modeLabels[r.mode] || r.mode },
            { html: requestBadge(r.status) },
            { html: AdminUI.escapeHtml(r.reviewNote || r.note || '—') }
        ]), 6, { empty: 'Müraciət yoxdur' });
    }

    async function loadAddons() {
        const rows = await AdminUI.get('/subscription/addons') || [];

        AdminUI.table('subscriptionAddons', rows.map(a => [
            { html: a.period },
            { html: kindLabels[a.kind] || a.kind },
            { html: a.quantity },
            { html: modeLabels[a.mode] || a.mode },
            { html: `<span class="font-semibold">${a.amount} ${a.currency}</span>` },
            {
                html: a.mode === 'monthly'
                    ? `<button type="button" class="inline-flex items-center rounded-lg px-2.5 py-1.5 text-sm font-semibold text-danger hover:bg-danger-light" onclick="SubscriptionPage.cancelAddon(${a.id})">Dayandır</button>`
                    : '',
                cls: 'text-right'
            }
        ]), 6, { empty: 'Əlavə paket yoxdur' });
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

    return { switchTenant, addAddon, choosePlan, cancelAddon, sendRequest };
})();
