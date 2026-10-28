(function () {
    const form = document.querySelector('[data-register-form]');
    const steps = [...document.querySelectorAll('[data-register-step]')];

    // Addım daxilindəki sahələri brauzer qaydaları ilə yoxlayır (div-də reportValidity yoxdur).
    function validateStep(step) {
        const invalid = [...step.querySelectorAll('input, select, textarea')].find(el => !el.checkValidity());
        if (invalid) invalid.reportValidity();
        return !invalid;
    }

    function showStep(number) {
        steps.forEach(step => step.classList.toggle('hidden', step.dataset.registerStep !== String(number)));
        applyAccountType();
        if (number === 2) document.getElementById('adminName')?.focus({ preventScroll: true });
    }

    document.addEventListener('click', (e) => {
        if (e.target.closest('[data-register-next]')) {
            if (!form.querySelector('input[name="AccountType"]:checked')) {
                form.querySelector('[data-account-type]')?.focus();
                return;
            }
            const step1 = form.querySelector('[data-register-step="1"]');
            if (step1 && !validateStep(step1)) return;
            showStep(2);
        }
        if (e.target.closest('[data-register-back]')) showStep(1);
    });

    // Kurs ↔ fərdi müəllim: kurs sahələri yalnız kurs seçildikdə göstərilir.
    function applyAccountType() {
        const type = document.querySelector('input[name="AccountType"]:checked')?.value || 'course';

        const block = document.getElementById('courseBlock');
        if (block) block.classList.toggle('hidden', type !== 'course');

        // Gizli addımdakı required sahə brauzer yoxlamasını bloklayır — yalnız
        // birinci addım görünəndə tələb olunur.
        const step1Visible = !block?.closest('[data-register-step]')?.classList.contains('hidden');

        const courseName = document.getElementById('courseName');
        if (courseName) courseName.required = type === 'course' && step1Visible;
    }

    // Birinci addımda Enter basılanda forma göndərilmir — növbəti addıma keçilir.
    form?.addEventListener('submit', (e) => {
        const step1 = steps.find(step => step.dataset.registerStep === '1');
        if (!step1 || step1.classList.contains('hidden')) return;

        e.preventDefault();
        if (!validateStep(step1)) return;
        showStep(2);
    });

    document.addEventListener('change', (e) => {
        if (e.target.closest?.('[data-account-type]')) applyAccountType();

        const input = e.target.closest?.('[data-logo-input]');
        if (!input) return;
        const label = document.getElementById('logoName');
        if (label) label.textContent = input.files[0] ? input.files[0].name : (input.dataset.empty || '');
    });

    const EYE = '<svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" /><circle cx="12" cy="12" r="3" /></svg>';
    const EYE_OFF = '<svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="m3 3 18 18M10.6 10.6a3 3 0 0 0 4.2 4.2M9.9 5.2A10.5 10.5 0 0 1 12 5c6.5 0 10 7 10 7a17 17 0 0 1-3 3.9M6.6 6.6C3.6 8.5 2 12 2 12s3.5 7 10 7a10 10 0 0 0 4.4-1" /></svg>';

    // Şifrə sahələrinə göstər/gizlət düyməsi.
    function addPasswordToggle(input) {
        if (input.dataset.eyeEnhanced === '1') return;
        input.dataset.eyeEnhanced = '1';

        const wrap = document.createElement('div');
        wrap.className = 'relative';
        input.parentNode.insertBefore(wrap, input);
        wrap.appendChild(input);
        input.classList.add('pr-11');

        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'absolute right-2 top-1/2 grid h-8 w-8 -translate-y-1/2 place-items-center rounded-lg text-slate-400 transition hover:bg-surface-secondary hover:text-slate-600';
        btn.setAttribute('aria-label', 'Şifrəni göstər');
        btn.innerHTML = EYE;

        btn.addEventListener('click', () => {
            const visible = input.type === 'text';
            input.type = visible ? 'password' : 'text';
            btn.innerHTML = visible ? EYE : EYE_OFF;
            btn.setAttribute('aria-label', visible ? 'Şifrəni göstər' : 'Şifrəni gizlət');
            input.focus();
        });

        wrap.appendChild(btn);
    }

    document.addEventListener('DOMContentLoaded', () => {
        applyAccountType();
        if (form) showStep(form.dataset.startStep || '1');
        document.querySelectorAll('input[type="password"]').forEach(addPasswordToggle);
    });
})();
