(function () {
    'use strict';

    function bindForm(formId, fieldsId) {
        let form = document.getElementById(formId);
        if (!form) return;
        form.addEventListener('submit', async function (event) {
            event.preventDefault();
            let submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) submitBtn.disabled = true;

            try {
                let res = await Kela.axios.post(form.action, new FormData(form));
                if (res.data && res.data.redirect) {
                    window.location.href = res.data.redirect;
                    return;
                }
            } catch (err) {
                if (err.response && err.response.status === 422) {
                    let fields = document.getElementById(fieldsId);
                    if (fields) fields.outerHTML = err.response.data;
                }
            }

            if (submitBtn) submitBtn.disabled = false;
        });
    }

    bindForm('login-form', 'login-form-fields');
    bindForm('register-form', 'register-form-fields');
})();
