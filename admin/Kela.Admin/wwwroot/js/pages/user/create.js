(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');


    window.buildUserForm = function () {
        const fd = new FormData();
        fd.append('name', document.getElementById('name').value || '');
        fd.append('surname', document.getElementById('surname').value || '');
        fd.append('email', document.getElementById('email').value);
        fd.append('phoneNumber', document.getElementById('phoneNumber').value || '');
        fd.append('type', document.getElementById('type').value);
        fd.append('password', document.getElementById('password').value || '');
        return fd;
    };

    // Şagird yalnız yaradılarkən seçilə bilər — mövcud istifadəçi tipi dəyişmir.
    window.setStudentOptionVisible = function (visible) {
        const option = [...document.getElementById('type').options].find(o => o.value === 'student');
        if (option) option.hidden = !visible;
    };

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["User_Create"];
        document.getElementById('entityId').value = '';
        ['name', 'surname', 'email', 'phoneNumber', 'password'].forEach(id => document.getElementById(id).value = '');
        document.getElementById('type').value = 'teacher';
        window.setStudentOptionVisible(true);
        document.getElementById('passwordHint').classList.add('hidden');
        setPasswordVisible(false);
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const res = await AdminUI.postForm('/user', window.buildUserForm());
        if (res.ok) {
            AdminUI.closeModal('modal');
            loadList(1);

            // Təsadüfi şifrə yalnız bir dəfə göstərilir.
            if (res.data?.password) {
                await AdminUI.swal({ title: res.data.email, text: res.data.password, icon: 'success' });
            } else {
                AdminUI.notify(L["Js_Added"]);
            }
            return;
        }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });

    // --- Şifrə: göstər/gizlət və yarat ---
    const passwordInput = document.getElementById('password');
    const togglePassword = document.getElementById('togglePassword');
    const eyeOn = togglePassword?.querySelector('.password-eye');
    const eyeOff = togglePassword?.querySelector('.password-eye-off');

    function setPasswordVisible(visible) {
        passwordInput.type = visible ? 'text' : 'password';
        eyeOn?.classList.toggle('hidden', visible);
        eyeOff?.classList.toggle('hidden', !visible);
        togglePassword?.setAttribute('title', visible ? L["User_HidePassword"] : L["User_ShowPassword"]);
        togglePassword?.setAttribute('aria-label', togglePassword.title);
    }

    window.resetPasswordField = function () {
        passwordInput.value = '';
        setPasswordVisible(false);
    };

    togglePassword?.addEventListener('click', () => {
        setPasswordVisible(passwordInput.type === 'password');
        passwordInput.focus();
    });

    // Oxşar simvollar (l,1,0,O) istisna — server tərəfindəki generasiya ilə eyni məntiq.
    function generatePassword() {
        const letters = 'abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ';
        const digits = '23456789';
        const pick = pool => pool[crypto.getRandomValues(new Uint32Array(1))[0] % pool.length];

        const chars = Array.from({ length: 12 }, (_, i) => pick(i % 3 === 2 ? digits : letters));
        chars[0] = pick(letters);
        chars[11] = pick(digits);
        return chars.join('');
    }

    document.getElementById('generatePassword')?.addEventListener('click', () => {
        passwordInput.value = generatePassword();
        setPasswordVisible(true);
        passwordInput.focus();
    });
})();
