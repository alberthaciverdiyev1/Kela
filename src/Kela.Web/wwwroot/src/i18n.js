(function () {
    'use strict';

    let Kela = window.Kela || {};

    let cached = null;
    let cachedLang = null;

    function msgs() {
        let el = document.getElementById('kela-msgs');
        if (!el) return {};
        let lang = el.getAttribute('data-lang') || '';
        if (!cached || cachedLang !== lang) {
            try {
                cached = JSON.parse(el.textContent || '{}');
            } catch (e) {
                cached = {};
            }
            cachedLang = lang;
        }
        return cached;
    }

    Kela.t = function (key, params) {
        let dict = msgs();
        let value = dict[key] || key;
        if (params) {
            Object.keys(params).forEach(function (name) {
                value = value.split('{' + name + '}').join(params[name]);
            });
        }
        return value;
    };

    Kela.esc = function (s) {
        if (s == null) return '';
        return String(s).replace(/[&<>"']/g, function (c) {
            return '&#' + c.charCodeAt(0) + ';';
        });
    };

    Kela.clearErrors = function (fieldsEl) {
        if (!fieldsEl) return;
        fieldsEl.querySelectorAll('.field-error, .validation-summary').forEach(function (el) {
            el.remove();
        });
    };

    Kela.applyErrors = function (fieldsEl, errors) {
        Kela.clearErrors(fieldsEl);
        if (!fieldsEl || !errors) return;
        Object.keys(errors).forEach(function (key) {
            let message = errors[key];
            if (!message) return;
            if (key === '') {
                let summary = document.createElement('div');
                summary.className = 'validation-summary';
                summary.innerHTML = '<ul><li>' + Kela.esc(message) + '</li></ul>';
                fieldsEl.insertBefore(summary, fieldsEl.firstChild);
                return;
            }
            let input = fieldsEl.querySelector('input[name="' + key + '"], select[name="' + key + '"], textarea[name="' + key + '"]');
            let field = input ? input.closest('.field') : null;
            if (field) {
                let span = document.createElement('span');
                span.className = 'field-error text-error text-xs mt-1';
                span.textContent = message;
                field.appendChild(span);
            }
        });
    };

    window.Kela = Kela;
})();
