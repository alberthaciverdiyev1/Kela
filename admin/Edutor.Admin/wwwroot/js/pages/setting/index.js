(function () {
    const FIELDS = ['description', 'email', 'address', 'whatsappNumber', 'contactNumber1', 'contactNumber2',
        'contactNumber3', 'contactNumber4', 'instagramUrl', 'facebookUrl', 'androidMinVersion', 'iosMinVersion'];

    if (window.LOGO_URL) AdminUI.fileSet('logo', window.LOGO_URL);

    document.getElementById('settingForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();

        // 1) Kurs (tenant) adı
        const name = document.getElementById('tenantName').value.trim();
        if (!name) { AdminUI.notify(L["Js_ErrorOccured"], 'warning'); return; }
        const profileRes = await AdminUI.putForm('/setting/profile', (() => {
            const p = new FormData();
            p.append('name', name);
            return p;
        })());
        if (!profileRes.ok) { AdminUI.error(AdminUI.errText(profileRes)); return; }

        // 2) Kurs parametrləri (loqo, əlaqə, info, versiya)
        const fd = new FormData();
        // Superadmin olmayanda versiya sahələri render olunmur — mövcud olanlar göndərilir.
        FIELDS.forEach(id => {
            const el = document.getElementById(id);
            if (el) fd.append(id, el.value || '');
        });
        fd.append('RemoveLogo', document.getElementById('logo_remove').value === 'true' ? 'true' : 'false');
        const file = document.getElementById('logo').files[0];
        if (file) fd.append('Logo', file);

        const res = await AdminUI.putForm('/setting', fd);
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); if (res.data.logo) AdminUI.fileSet('logo', res.data.logo); }
        else AdminUI.error(AdminUI.errText(res));
    });

    document.getElementById('clearCacheBtn')?.addEventListener('click', async () => {
        const res = await AdminUI.post('/setting/clear-cache', {});
        if (res.ok) AdminUI.notify(L["Js_CacheCleared"]);
        else AdminUI.error();
    });
})();
