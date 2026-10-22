(function () {
    const fields = [
        'code', 'name', 'description', 'monthly_price', 'currency', 'yearly_free_months', 'sort_order',
        'max_teachers', 'max_students', 'max_groups', 'max_subjects', 'max_lessons',
        'monthly_video_minutes', 'storage_gb', 'concurrent_conversions',
        'addon_student_pack_size', 'addon_student_pack_price',
        'addon_video_pack_minutes', 'addon_video_pack_price',
        'addon_storage_pack_gb', 'addon_storage_pack_price',
        'max_addon_percent', 'original_retention_days', 'features'
    ];

    const modal = () => document.getElementById('planModal');

    window.PlanPage.open = function () {
        modal().classList.remove('hidden');
        modal().classList.add('flex');
    };

    window.PlanPage.close = function () {
        modal().classList.add('hidden');
        modal().classList.remove('flex');
    };

    window.PlanPage.edit = async function (id) {
        const p = await AdminUI.get(`/plan/${id}`);

        document.getElementById('planTitle').textContent = `Plan — ${p.name}`;
        document.getElementById('planId').value = p.id;
        document.getElementById('code').value = p.code;
        document.getElementById('name').value = p.name;
        document.getElementById('description').value = p.description || '';
        document.getElementById('monthly_price').value = p.monthlyPrice;
        document.getElementById('currency').value = p.currency;
        document.getElementById('yearly_free_months').value = p.yearlyFreeMonths;
        document.getElementById('sort_order').value = p.sortOrder;
        document.getElementById('is_active').value = String(p.isActive);

        document.getElementById('max_teachers').value = p.maxTeachers;
        document.getElementById('max_students').value = p.maxStudents;
        document.getElementById('max_groups').value = p.maxGroups;
        document.getElementById('max_subjects').value = p.maxSubjects;
        document.getElementById('max_lessons').value = p.maxLessons;
        document.getElementById('monthly_video_minutes').value = p.monthlyVideoMinutes;
        document.getElementById('storage_gb').value = Math.round(p.storageBytes / 1024 / 1024 / 1024);
        document.getElementById('concurrent_conversions').value = p.concurrentConversions;
        document.getElementById('max_video_height').value = p.maxVideoHeight;

        document.getElementById('addon_student_pack_size').value = p.addonStudentPackSize;
        document.getElementById('addon_student_pack_price').value = p.addonStudentPackPrice;
        document.getElementById('addon_video_pack_minutes').value = p.addonVideoPackMinutes;
        document.getElementById('addon_video_pack_price').value = p.addonVideoPackPrice;
        document.getElementById('addon_storage_pack_gb').value = Math.round(p.addonStoragePackBytes / 1024 / 1024 / 1024);
        document.getElementById('addon_storage_pack_price').value = p.addonStoragePackPrice;
        document.getElementById('max_addon_percent').value = p.maxAddonPercent;
        document.getElementById('original_retention_days').value = p.originalRetentionDays;

        document.getElementById('features').value = Object.entries(JSON.parse(p.features || '{}'))
            .filter(([, on]) => on).map(([key]) => key).join(',');

        window.PlanPage.open();
    };

    document.addEventListener('DOMContentLoaded', () => {
        document.getElementById('planForm').addEventListener('submit', async (e) => {
            e.preventDefault();

            const fd = new FormData();
            fields.forEach(id => fd.append(id, document.getElementById(id).value || ''));
            fd.append('is_active', document.getElementById('is_active').value);

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
