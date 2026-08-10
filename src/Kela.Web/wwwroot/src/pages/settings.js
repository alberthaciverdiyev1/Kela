(function () {
    'use strict';

    const form = document.getElementById('settings-form');
    if (!form) return;

    const defaults = {
        SiteName: 'Kela',
        NavMode: 'navbar',
        NotificationProvider: 'sweetalert',
        PrimaryColor: '#4f46e5',
        SecondaryColor: '#64748b',
        SuccessColor: '#22c55e',
        WarningColor: '#f59e0b',
        ErrorColor: '#ef4444',
        InfoColor: '#0ea5e9'
    };

    const previewBtn = document.getElementById('notify-preview');
    if (previewBtn) {
        previewBtn.addEventListener('click', function () {
            let provider = form.querySelector('input[name="NotificationProvider"]:checked');
            let previous = Kela.notifyProvider;
            Kela.notifyProvider = provider ? provider.value : 'sweetalert';
            Kela.notify.success(Kela.t('settings.notifyPreviewMsg'));
            Kela.notifyProvider = previous;
        });
    }

    const resetBtn = document.getElementById('settings-reset');
    if (resetBtn) {
        resetBtn.addEventListener('click', function () {
            Object.keys(defaults).forEach(function (name) {
                let el = form.elements[name];
                if (!el) return;
                if (el.type === 'radio') {
                    el.checked = el.value === defaults[name];
                } else {
                    el.value = defaults[name];
                }
            });
        });
    }

    form.addEventListener('submit', async function (event) {
        event.preventDefault();
        let submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) submitBtn.disabled = true;

        try {
            await Kela.axios.post(form.action, new FormData(form));
            if (submitBtn) submitBtn.disabled = false;
            window.location.href = '/teacher/settings';
        } catch (err) {
            if (submitBtn) submitBtn.disabled = false;
            if (err.response && err.response.status === 422) {
                let summary = form.querySelector('.validation-summary');
                if (summary) summary.innerHTML = '';
                let errors = err.response.data.errors || {};
                Object.keys(errors).forEach(function (key) {
                    let input = form.elements[key];
                    if (input) {
                        let existing = input.parentElement.querySelector('.field-error');
                        if (existing) existing.remove();
                        let span = document.createElement('span');
                        span.className = 'field-error text-error text-xs mt-1';
                        span.textContent = errors[key];
                        input.parentElement.appendChild(span);
                    }
                });
            }
        }
    });
})();
