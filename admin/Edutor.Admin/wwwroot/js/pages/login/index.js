    (function () {
        var modal = document.getElementById('tenantModal');
        var list = document.getElementById('tenantModalList');
        var search = document.getElementById('tenantSearch');
        var radios = document.querySelectorAll('input[name="TenantId"]');

        function norm(v) {
            return String(v || '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().replace(/ı/g, 'i');
        }

        function syncSelected() {
            var checked = document.querySelector('input[name="TenantId"]:checked');
            var hint = document.getElementById('tenantPicked');

            list.querySelectorAll('.tenant-option').forEach(function (btn) {
                var on = checked && btn.dataset.tenantId === checked.value;
                btn.classList.toggle('border-primary', on);
                btn.classList.toggle('bg-primary-light', on);
                btn.classList.toggle('text-primary', on);
            });

            if (hint) {
                hint.textContent = checked ? checked.dataset.name : '';
                hint.classList.toggle('hidden', !checked);
            }
        }

        function openModal() {
            search.value = '';
            list.querySelectorAll('.tenant-option').forEach(function (b) { b.classList.remove('hidden'); });
            syncSelected();
            modal.classList.remove('hidden');
            modal.classList.add('flex');
            search.focus();
        }

        function closeModal() {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
        }

        document.getElementById('tenantSeeAll').addEventListener('click', openModal);
        document.getElementById('tenantModalClose').addEventListener('click', closeModal);
        modal.addEventListener('click', function (e) { if (e.target === modal) closeModal(); });
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && !modal.classList.contains('hidden')) closeModal(); });

        search.addEventListener('input', function () {
            var q = norm(search.value).trim();
            list.querySelectorAll('.tenant-option').forEach(function (b) {
                b.classList.toggle('hidden', !!q && norm(b.dataset.tenantName).indexOf(q) === -1);
            });
        });

        list.addEventListener('click', function (e) {
            var btn = e.target.closest('.tenant-option');
            if (!btn) return;

            var radio = document.querySelector('input[name="TenantId"][value="' + btn.dataset.tenantId + '"]');
            if (radio) { radio.checked = true; radio.dispatchEvent(new Event('change', { bubbles: true })); }
            syncSelected();
            closeModal();
        });

        radios.forEach(function (r) { r.addEventListener('change', syncSelected); });

        // Şifrəni göstər/gizlət.
        var pwd = document.getElementById('password');
        var pwdToggle = document.getElementById('passwordToggle');
        var eyeOpen = document.getElementById('eyeOpen');
        var eyeOff = document.getElementById('eyeOff');
        pwdToggle.addEventListener('click', function () {
            var show = pwd.type === 'password';
            pwd.type = show ? 'text' : 'password';
            eyeOpen.classList.toggle('hidden', show);
            eyeOff.classList.toggle('hidden', !show);
            pwdToggle.setAttribute('aria-label', show ? pwdToggle.dataset.hideLabel : pwdToggle.dataset.showLabel);
        });
    })();
