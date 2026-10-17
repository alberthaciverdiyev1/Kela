// Kela admin panel — modal
// Parçalanmışdır: hər fayl eyni AdminUI obyektini genişləndirir.
window.AdminUI = window.AdminUI || {};

(function (UI) {
    function openModal(id) {
        const modal = typeof id === 'string' ? document.getElementById(id) : id;
        if (!modal) return;
        // Sahələr JS ilə doldurulub — açılan siyahıların etiketi təzələnsin.
        modal.querySelectorAll('select[data-enhanced]').forEach(syncSelect);
        UI.syncEditors(modal);
        modal.dispatchEvent(new CustomEvent('modal:open'));
        modal.classList.remove('hidden');
        modal.classList.add('flex');
    }

    function closeModal(id) {
        const modal = typeof id === 'string' ? document.getElementById(id) : id;
        if (!modal || modal.classList.contains('hidden')) return;
        if (!modal.dispatchEvent(new CustomEvent('modal:before-close', { cancelable: true }))) return;

        modal.classList.add('hidden');
        modal.classList.remove('flex');

        // Bağlanan modalın forması təmizlənir — növbəti açılış köhnə məlumatla başlamasın.
        modal.querySelectorAll('form').forEach(resetForm);
        UI.closeSelects();

        const err = modal.querySelector('[data-modal-error]');
        if (err) err.classList.add('hidden');

        modal.dispatchEvent(new CustomEvent('modal:closed'));
    }

    function openModals() {
        return [...document.querySelectorAll('[data-modal]')].filter(m => !m.classList.contains('hidden'));
    }

    function resetForm(formOrId) {
        const form = typeof formOrId === 'string' ? document.getElementById(formOrId) : formOrId;
        if (!form || !form.reset) return;
        form.reset();
        // Gizli sahələri JS doldurur və onların default dəyəri yoxdur —
        // reset() onları boşaltmır, ona görə əl ilə sıfırlanır.
        form.querySelectorAll('input[type=hidden]').forEach(input => { input.value = ''; });
        form.querySelectorAll('input[type=file]').forEach(input => { if (input.id) UI.fileReset(input.id); });
        form.querySelectorAll('.admin-editor-body').forEach(el => { el.innerHTML = ''; });
    }

    function validateForm(formOrId) {
        const form = typeof formOrId === 'string' ? document.getElementById(formOrId) : formOrId;
        if (!form) return true;

        form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
        const empty = [...form.querySelectorAll('[data-required="true"]')]
            .filter(el => !el.disabled && !String(el.value ?? '').trim());

        if (!empty.length) return true;

        empty.forEach(el => el.classList.add('is-invalid'));
        const box = form.closest('[data-modal]')?.querySelector('[data-modal-error]')
            || form.querySelector('[data-modal-error]');
        if (box) {
            box.textContent = L["Common_RequiredFields"];
            box.classList.remove('hidden');
        }
        empty[0].focus();
        return false;
    }

    function syncSelect(select) {
        const wrap = document.querySelector(`.admin-select[data-for="${select.id}"]`);
        const trigger = wrap && wrap.querySelector('.admin-select-trigger');
        if (!trigger) return;
        const opts = [...select.options];
        const multiple = !!select.multiple;
        const text = multiple
            ? (opts.filter(o => o.selected).length
                ? (L["Js_SelectedCount"] || '{0}').replace('{0}', opts.filter(o => o.selected).length)
                : '')
            : ((opts[select.selectedIndex] || {}).text || '');
        trigger.textContent = text;
        trigger.classList.toggle('is-empty', !text);
    }


    // ESC ən üstdəki modalı bağlayır; fon sahəsinə klik də bağlayır.
    document.addEventListener('keydown', (e) => {
        if (e.key !== 'Escape') return;
        const open = UI.openModals();
        if (open.length) closeModal(open[open.length - 1]);
    });

    document.addEventListener('click', (e) => {
        const backdrop = e.target.closest ? e.target.closest('[data-modal]') : null;
        if (backdrop && e.target === backdrop) closeModal(backdrop);
    });

    // Sahə doldurulan kimi qırmızı işarə götürülür.
    document.addEventListener('input', (e) => {
        if (e.target.classList) e.target.classList.remove('is-invalid');
    });

    // Göndərmədən əvvəl yoxla — səhifələrin öz submit handler-i işə düşməsin.
    document.addEventListener('submit', (e) => {
        if (!e.target.querySelector || !e.target.querySelector('[data-required="true"]')) return;
        if (!UI.validateForm(e.target)) {
            e.preventDefault();
            e.stopImmediatePropagation();
        }
    }, true);

    Object.assign(UI, { openModal, closeModal, openModals, resetForm, validateForm, syncSelect });
})(window.AdminUI);
