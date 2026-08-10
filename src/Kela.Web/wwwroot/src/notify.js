(function () {
    'use strict';

    let Kela = window.Kela || {};

    Kela.notifyProvider = document.body && document.body.dataset.notifyProvider
        ? document.body.dataset.notifyProvider
        : (Kela.notifyProvider || 'sweetalert');

    function provider() {
        return document.body && document.body.dataset.notifyProvider
            ? document.body.dataset.notifyProvider
            : (Kela.notifyProvider || 'sweetalert');
    }

    function viaAlertify(type, message) {
        if (typeof alertify === 'undefined') return;
        alertify.set('notifier', 'position', 'top-right');
        alertify.set('notifier', 'delay', 3);
        alertify.notify(message, type, 3);
    }

    function viaSweetAlert(type, message) {
        if (typeof Swal === 'undefined') return;
        Swal.fire({
            icon: type,
            title: message,
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            customClass: {
                popup: 'rounded-box shadow-xl'
            }
        });
    }

    function show(type, message) {
        if (!message) return;
        if (provider() === 'alertify') {
            viaAlertify(type, message);
        } else {
            viaSweetAlert(type, message);
        }
    }

    Kela.notify = {
        success: function (message) { show('success', message); },
        error: function (message) { show('error', message); },
        warning: function (message) { show('warning', message); },
        info: function (message) { show('info', message); }
    };

    (function () {
        let el = document.getElementById('kela-notify');
        if (!el) return;
        try {
            let items = JSON.parse(el.textContent || '[]');
            items.forEach(function (item) {
                show(item.type, item.message);
            });
        } catch (e) {
        }
    })();

    window.Kela = Kela;
})();
