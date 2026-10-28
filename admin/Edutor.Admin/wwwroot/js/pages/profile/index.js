(function () {
    document.getElementById('profileForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const fd = new FormData();
        ['name', 'surname', 'email', 'phoneNumber'].forEach(id => fd.append(id, document.getElementById(id).value || ''));

        const res = await AdminUI.putForm('/profile', fd);
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); AdminUI.reloadAfter(600); }
        else AdminUI.error(AdminUI.errText(res));
    });

    document.getElementById('passwordForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const current = document.getElementById('currentPassword').value;
        const next = document.getElementById('newPassword').value;
        const confirm = document.getElementById('confirmPassword').value;

        if (next !== confirm) { AdminUI.notify(L["Profile_PasswordMismatch"], 'error'); return; }

        const fd = new FormData();
        fd.append('currentPassword', current);
        fd.append('newPassword', next);

        const res = await AdminUI.putForm('/profile/password', fd);
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); e.target.reset(); }
        else AdminUI.error(AdminUI.errText(res));
    });
})();
