(function () {
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
