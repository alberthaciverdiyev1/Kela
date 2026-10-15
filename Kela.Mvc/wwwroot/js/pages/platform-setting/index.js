(function () {
    const FIELDS = ['description', 'email', 'address', 'whatsappNumber', 'contactNumber1', 'contactNumber2',
        'contactNumber3', 'contactNumber4', 'instagramUrl', 'facebookUrl'];

    if (window.LOGO_URL) AdminUI.fileSet('logo', window.LOGO_URL);

    document.getElementById('platformSettingForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();

        const fd = new FormData();
        FIELDS.forEach(id => fd.append(id, document.getElementById(id).value || ''));
        fd.append('RemoveLogo', document.getElementById('logo_remove').value === 'true' ? 'true' : 'false');
        const file = document.getElementById('logo').files[0];
        if (file) fd.append('Logo', file);

        const res = await AdminUI.putForm('/platform-setting', fd);
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); if (res.data.logo) AdminUI.fileSet('logo', res.data.logo); }
        else AdminUI.error(AdminUI.errText(res));
    });
})();
