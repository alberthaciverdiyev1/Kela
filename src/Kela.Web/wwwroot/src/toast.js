(function () {
    'use strict';

    let Kela = window.Kela || {};

    const ICONS = {
        success: '<svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>',
        error: '<svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>'
    };

    Kela.toast = function (message, type) {
        type = type || 'success';

        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast toast-top toast-end z-[100]';
            document.body.appendChild(container);
        }

        let el = document.createElement('div');
        el.className = 'alert shadow-lg ' + (type === 'error' ? 'alert-error' : 'alert-success');
        el.innerHTML = ICONS[type] + '<span></span>';
        el.querySelector('span').textContent = message;
        container.appendChild(el);

        setTimeout(function () {
            el.remove();
        }, 3000);
    };

    window.Kela = Kela;
})();
