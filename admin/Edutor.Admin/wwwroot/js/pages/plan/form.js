(function () {
    // Plan şablonları modulu — digər modullar hələ UI-da idarə olunmur, JSON-da saxlanılır.
    const PAYMENT_TEMPLATES = 'paymentTemplates';

    const featureKeys = () => (document.getElementById('features').value || '')
        .split(',').map(k => k.trim()).filter(Boolean);

    function setFeature(key, on) {
        const keys = featureKeys().filter(k => k !== key);
        if (on) keys.push(key);
        document.getElementById('features').value = keys.join(',');
    }

    const fields = [
        'code', 'name', 'description', 'monthly_price', 'currency', 'yearly_free_months', 'trial_days', 'sort_order',
        'max_students', 'max_groups', 'monthly_video_hours',
        'price_per_student', 'price_per_group', 'price_per_video_hour',
        // Görünməyən, amma mövcud planı redaktə edəndə saxlanmalı olan sahələr.
        'max_teachers', 'max_subjects', 'max_lessons', 'storage_gb',
        'concurrent_conversions', 'max_video_height', 'original_retention_days', 'features'
    ];

    window.PlanPage.open = function () {
        AdminUI.openModal('planModal');
    };

    window.PlanPage.close = function () {
        AdminUI.closeModal('planModal');
    };

    window.PlanPage.create = function () {
        document.getElementById('planForm').reset();
        document.getElementById('planId').value = '';
        document.getElementById('planTitle').textContent = 'Yeni plan';
        document.getElementById('monthly_price').value = '0';
        document.getElementById('trial_days').value = '14';
        document.getElementById('monthly_video_minutes').value = '0';
        document.getElementById('is_active').value = 'true';
        document.getElementById('features').value = '';
        document.getElementById('feature_payment_templates').checked = false;
        window.PlanPage.open();
    };

    window.PlanPage.edit = async function (id) {
        const p = await AdminUI.get(`/plan/${id}`);

        document.getElementById('allow_video_upload').checked = p.allowVideoUpload;
        document.getElementById('allow_audio_upload').checked = p.allowAudioUpload;
        document.getElementById('allow_file_upload').checked = p.allowFileUpload;
        document.getElementById('bill_addons_with_plan').checked = p.billAddonsWithPlan;

        document.getElementById('planTitle').textContent = `Plan — ${p.name}`;
        document.getElementById('planId').value = p.id;
        document.getElementById('code').value = p.code;
        document.getElementById('name').value = p.name;
        document.getElementById('description').value = p.description || '';
        document.getElementById('monthly_price').value = p.monthlyPrice;
        document.getElementById('currency').value = p.currency;
        document.getElementById('yearly_free_months').value = p.yearlyFreeMonths;
        document.getElementById('trial_days').value = p.trialDays;
        document.getElementById('sort_order').value = p.sortOrder;
        document.getElementById('is_active').value = String(p.isActive);

        document.getElementById('max_students').value = p.maxStudents;
        document.getElementById('max_groups').value = p.maxGroups;
        document.getElementById('monthly_video_hours').value = p.monthlyVideoMinutes > 0 ? p.monthlyVideoMinutes / 60 : 0;

        document.getElementById('price_per_student').value = p.pricePerStudent;
        document.getElementById('price_per_group').value = p.pricePerGroup;
        document.getElementById('price_per_video_hour').value = p.pricePerVideoHour;

        document.getElementById('max_teachers').value = p.maxTeachers;
        document.getElementById('max_subjects').value = p.maxSubjects;
        document.getElementById('max_lessons').value = p.maxLessons;
        document.getElementById('storage_gb').value = Math.round(p.storageBytes / 1024 / 1024 / 1024);
        document.getElementById('concurrent_conversions').value = p.concurrentConversions;
        document.getElementById('max_video_height').value = p.maxVideoHeight;
        document.getElementById('original_retention_days').value = p.originalRetentionDays;
        document.getElementById('features').value = Object.entries(JSON.parse(p.features || '{}'))
            .filter(([, on]) => on).map(([key]) => key).join(',');
        document.getElementById('feature_payment_templates').checked = featureKeys().includes(PAYMENT_TEMPLATES);

        window.PlanPage.open();
    };

    document.addEventListener('DOMContentLoaded', () => {
        document.getElementById('planForm').addEventListener('submit', async (e) => {
            e.preventDefault();

            const hours = parseFloat(document.getElementById('monthly_video_hours').value) || 0;
            document.getElementById('monthly_video_minutes').value = Math.round(hours * 60);
            setFeature(PAYMENT_TEMPLATES, document.getElementById('feature_payment_templates').checked);

            const fd = new FormData();
            fields.forEach(id => fd.append(id, document.getElementById(id).value || ''));
            fd.append('monthly_video_minutes', document.getElementById('monthly_video_minutes').value);
            fd.append('is_active', document.getElementById('is_active').value);

            ['allow_video_upload', 'allow_audio_upload', 'allow_file_upload', 'bill_addons_with_plan'].forEach(key =>
                fd.append(key, document.getElementById(key).checked ? 'true' : 'false'));

            const id = document.getElementById('planId').value;
            const res = id
                ? await AdminUI.putForm(`/plan/${id}`, fd)
                : await AdminUI.postForm('/plan', fd);

            if (res.ok) {
                AdminUI.notify(id ? L["Js_Updated"] : L["Js_Created"]);
                window.PlanPage.close();
                window.PlanPage.load();
            } else {
                AdminUI.error(AdminUI.errText(res));
            }
        });
    });
})();
