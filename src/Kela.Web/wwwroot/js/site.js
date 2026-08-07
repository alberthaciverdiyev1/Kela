(function () {
    'use strict';

    let Kela = window.Kela || {};

    Kela.csrfToken = function () {
        let meta = document.querySelector('meta[name="csrf-token"]');
        return meta ? meta.content : '';
    };

    Kela.axios = axios.create();

    Kela.axios.interceptors.request.use(function (config) {
        config.headers['X-CSRF-Token'] = Kela.csrfToken();
        config.headers['X-Requested-With'] = 'XMLHttpRequest';
        return config;
    });

    Kela.axios.interceptors.response.use(
        function (response) {
            return response;
        },
        function (error) {
            if (error.response) {
                if (error.response.status === 401) window.location.href = '/auth/login';
                else if (error.response.status === 403) window.location.href = '/blocked';
            }
            return Promise.reject(error);
        }
    );

    document.addEventListener('click', function (event) {
        let el = event.target;
        if (el && el.tagName === 'DIALOG' && el.open) el.close();
    });

    document.addEventListener('submit', function (event) {
        let form = event.target;
        if (!form || (form.id !== 'logout-form' && form.id !== 'lang-form')) return;
        event.preventDefault();
        Kela.axios.post(form.action, new FormData(form)).then(function () {
            if (form.id === 'logout-form') window.location.href = '/auth/login';
            else window.location.reload();
        }).catch(function () {
            if (form.id === 'logout-form') window.location.href = '/auth/login';
            else window.location.reload();
        });
    });

    window.Kela = Kela;
})();
