// Edutor admin panel — modal
// Parçalanmışdır: hər fayl eyni AdminUI obyektini genişləndirir.
window.AdminUI = window.AdminUI || {};

(function (UI) {
    function openModal(id) {
        const modal = typeof id === 'string' ? document.getElementById(id) : id;
        if (!modal) return;
        // Sahələr JS ilə doldurulub — açılan siyahıların etiketi təzələnsin.
        modal.querySelectorAll('select[data-enhanced]').forEach(syncSelect);
        UI.syncEditors(modal);
        modal.dispatchEvent(new CustomEvent('modal:open', { bubbles: true }));
        modal.classList.remove('hidden');
        modal.classList.add('flex');
    }

    function closeModal(id) {
        const modal = typeof id === 'string' ? document.getElementById(id) : id;
        if (!modal || modal.classList.contains('hidden')) return;
        if (!modal.dispatchEvent(new CustomEvent('modal:before-close', { cancelable: true }))) return;

        setFullscreen(modal, false);
        modal.classList.add('hidden');
        modal.classList.remove('flex');

        // Bağlanan modalın forması təmizlənir — növbəti açılış köhnə məlumatla başlamasın.
        modal.querySelectorAll('form').forEach(resetForm);
        UI.closeSelects();

        const err = modal.querySelector('[data-modal-error]');
        if (err) err.classList.add('hidden');

        modal.dispatchEvent(new CustomEvent('modal:closed', { bubbles: true }));
    }

    function openModals() {
        return [...document.querySelectorAll('[data-modal]')].filter(m => !m.classList.contains('hidden'));
    }

    function resetForm(formOrId) {
        const form = typeof formOrId === 'string' ? document.getElementById(formOrId) : formOrId;
        if (!form || !form.reset) return;
        form.reset();
        // Gizli sahələri JS doldurur və onların default dəyəri yoxdur —
        // reset() onları boşaltmır, ona görə əl ilə sıfırlanır. data-persist
        // olanlar səhifə kontekstini daşıyır və qalmalıdır (məs. mənbə dərs).
        form.querySelectorAll('input[type=hidden]:not([data-persist])').forEach(input => { input.value = ''; });
        form.querySelectorAll('input[type=file]').forEach(input => { if (input.id) UI.fileReset(input.id); });
        form.querySelectorAll('.admin-editor-body').forEach(el => {
            if (el._quill) el._quill.setText('');
            else el.innerHTML = '';
        });
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
        const picked = opts.filter(o => o.selected);
        const text = multiple
            ? (picked.length === 1
                ? picked[0].text
                : (picked.length ? (L["Js_SelectedCount"] || '{0}').replace('{0}', picked.length) : ''))
            : ((opts[select.selectedIndex] || {}).text || '');
        trigger.textContent = text;
        trigger.classList.toggle('is-empty', !text);
    }


    // Tam ekran: modal qabığı ekranı doldurur, bağlananda normal ölçüyə qayıdır.
    // Keçid FLIP ilə animasiya olunur — ölçü dəyişikliyi layout-u təkrar hesablamır.
    function setFullscreen(modal, on) {
        const panel = modal.querySelector('[data-modal-panel]');
        const first = panel ? panel.getBoundingClientRect() : null;

        modal.classList.toggle('is-fullscreen', on);

        const expand = modal.querySelector('[data-fullscreen-expand]');
        const collapse = modal.querySelector('[data-fullscreen-collapse]');
        const button = modal.querySelector('[data-modal-fullscreen]');

        if (expand) expand.classList.toggle('hidden', on);
        if (collapse) collapse.classList.toggle('hidden', !on);
        if (button) button.setAttribute('aria-pressed', on ? 'true' : 'false');

        if (!panel || !first || typeof panel.animate !== 'function') return;

        const last = panel.getBoundingClientRect();
        if (!last.width || !last.height) return;

        const dx = first.left - last.left;
        const dy = first.top - last.top;
        const sx = first.width / last.width;
        const sy = first.height / last.height;

        panel.style.transformOrigin = 'top left';
        const animation = panel.animate([
            { transform: `translate(${dx}px, ${dy}px) scale(${sx}, ${sy})` },
            { transform: 'none' }
        ], { duration: 280, easing: 'cubic-bezier(0.4, 0, 0.2, 1)' });

        animation.onfinish = () => {
            panel.style.transform = '';
            panel.style.transformOrigin = '';
        };
    }

    // data-modal-close — bağlama düymələri inline onclick olmadan.
    document.addEventListener('click', (e) => {
        const full = e.target.closest?.('[data-modal-fullscreen]');
        if (full) {
            const modal = full.closest('[data-modal]');
            if (modal) setFullscreen(modal, !modal.classList.contains('is-fullscreen'));
            return;
        }

        const btn = e.target.closest?.('[data-modal-close]');
        if (!btn) return;
        const modal = btn.closest('[data-modal]');
        if (modal) closeModal(modal);
    });

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

    // Form göndərməsi: validasiya → təkrar-göndərmə qoruyucusu → düymə kilidi.
    const _lastSubmit = new WeakMap();

    document.addEventListener('submit', (e) => {
        const form = e.target;
        if (!form || !form.querySelector) return;

        // 1) Zəruri sahələr
        if (form.querySelector('[data-required="true"]') && !UI.validateForm(form)) {
            e.preventDefault();
            e.stopImmediatePropagation();
            return;
        }

        // 2) Qısa müddət ərzində təkrar göndərmə (sürətli ikiqat klik)
        const now = Date.now();
        const prev = _lastSubmit.get(form) || 0;
        if (now - prev < 3000) {
            e.preventDefault();
            e.stopImmediatePropagation();
            return;
        }
        _lastSubmit.set(form, now);

        // 3) Düyməni kilidlə — sorğu qatı cavab gələndə açır (AdminUI.lockSubmitButton).
        UI.lockSubmitButton(e.submitter || form.querySelector('[type="submit"]'));
    }, true);

    Object.assign(UI, { openModal, closeModal, openModals, resetForm, validateForm, syncSelect });
})(window.AdminUI);
