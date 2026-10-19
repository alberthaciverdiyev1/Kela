// Tək şagird yaratma + şifrə sıfırlama (teacher/admin).
(function () {
    const form = document.getElementById('studentForm');

    window.openStudentCreate = function () {
        form.reset();
        AdminUI.openModal('studentModal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const fd = new FormData();
        fd.append('fullName', document.getElementById('s_fullName').value.trim());
        fd.append('email', document.getElementById('s_email').value.trim());
        fd.append('phone', document.getElementById('s_phone').value.trim());
        fd.append('password', document.getElementById('s_password').value);
        fd.append('group_id', document.getElementById('s_group').value || '');
        fd.append('is_active', 'true');

        const res = await AdminUI.postForm('/students', fd);
        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

        AdminUI.closeModal('studentModal');
        AdminUI.notify(L["Js_Created"]);
        if (res.data?.password) {
            // Təsadüfi şifrə — istifadəçiyə çatdırılmalıdır.
            await AdminUI.swal({ title: res.data.email, text: res.data.password, icon: 'success' });
        }
        loadList(1);
    });

    // Şifrə unudulubsa: yeni şifrə yaz və ya boş burax (təsadüfi yaradılsın).
    window.resetStudentPassword = async function (id) {
        const r = await AdminUI.swal({
            title: L["Students_ResetPassword"],
            input: 'text',
            inputPlaceholder: L["Students_NewPasswordHint"],
            showCancelButton: true,
            confirmButtonText: L["Common_Save"],
            cancelButtonText: L["Common_Cancel"]
        });
        if (!r.isConfirmed) return;

        const res = await AdminUI.postJson(`/students/${id}/password`, { password: r.value || '' });
        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

        AdminUI.notify(L["Students_PasswordReset"]);
        if (res.data?.password) {
            await AdminUI.swal({ title: res.data.email, text: res.data.password, icon: 'success' });
        }
    };
})();
