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

    window.Kela = Kela;
})();
