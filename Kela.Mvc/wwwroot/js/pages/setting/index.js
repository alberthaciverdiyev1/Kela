(function () {
    const FIELDS = ['whatsappNumber', 'contactNumber1', 'contactNumber2', 'contactNumber3', 'contactNumber4',
        'instagramUrl', 'facebookUrl', 'androidMinVersion', 'iosMinVersion'];

    if (window.LOGO_URL) AdminUI.fileSet('logo', window.LOGO_URL);

    document.getElementById('settingForm').addEventListener('submit', async (e) => {
        e.preventDefault();

        const fd = new FormData();
        FIELDS.forEach(id => fd.append(id, document.getElementById(id).value || ''));
        fd.append('RemoveLogo', document.getElementById('logo_remove').value === 'true' ? 'true' : 'false');
        const file = document.getElementById('logo').files[0];
        if (file) fd.append('Logo', file);

        const res = await AdminUI.putForm('/setting', fd);
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); if (res.data.logo) AdminUI.fileSet('logo', res.data.logo); }
        else AdminUI.error();
    });

    document.getElementById('clearCacheBtn')?.addEventListener('click', async () => {
        const res = await AdminUI.post('/setting/clear-cache', {});
        if (res.ok) AdminUI.notify(L["Js_CacheCleared"]);
        else AdminUI.error();
    });
})();
