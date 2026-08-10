let provider = 'sweetalert';

export function setNotifyProvider(name) {
    provider = name;
}

function show(type, message) {
    if (!message) return;
    if (provider === 'alertify' && typeof window.alertify !== 'undefined') {
        window.alertify.set('notifier', 'position', 'top-right');
        window.alertify.set('notifier', 'delay', 3);
        window.alertify.notify(message, type, 3);
    } else if (typeof window.Swal !== 'undefined') {
        window.Swal.fire({
            icon: type,
            title: message,
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            customClass: { popup: 'rounded-box shadow-xl' }
        });
    } else {
        console.log('[' + type + '] ' + message);
    }
}

export const notify = {
    success: (m) => show('success', m),
    error: (m) => show('error', m),
    warning: (m) => show('warning', m),
    info: (m) => show('info', m)
};
