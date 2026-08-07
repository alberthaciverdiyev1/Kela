document.addEventListener('htmx:configRequest', function (event) {
    var meta = document.querySelector('meta[name="csrf-token"]');
    if (meta) event.detail.headers['X-CSRF-Token'] = meta.content;
});

document.addEventListener('htmx:beforeSwap', function (event) {
    if (event.detail.xhr.status === 401) {
        window.location.href = '/auth/login';
    }
    if (event.detail.xhr.status === 403) {
        window.location.href = '/blocked';
    }
});

document.body.addEventListener('kela:createDone', function () {
    var create = document.getElementById('create-student-dialog');
    if (create && create.open) create.close();
    var creds = document.getElementById('credentials-dialog');
    if (creds) creds.showModal();
});

document.addEventListener('click', function (event) {
    var el = event.target;
    if (el && el.tagName === 'DIALOG' && el.open) el.close();
});

function copyCredentials() {
    var input = document.getElementById('credentials-text');
    if (!input) return;
    navigator.clipboard.writeText(input.value).then(function () {
        var btn = document.getElementById('credentials-copy');
        if (btn) {
            var old = btn.innerHTML;
            btn.innerHTML = '&#10003;';
            setTimeout(function () { btn.innerHTML = old; }, 1200);
        }
    });
}
