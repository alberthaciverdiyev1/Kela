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

    Kela.pageListeners = [];

    Kela.onPageEvent = function (type, handler) {
        Kela.pageListeners.push({ type: type, handler: handler });
        document.addEventListener(type, handler);
    };

    Kela.unmountPage = function () {
        Kela.pageListeners.forEach(function (entry) {
            document.removeEventListener(entry.type, entry.handler);
        });
        Kela.pageListeners = [];
    };

    function reExecuteScripts(doc) {
        let scripts = Array.prototype.slice.call(doc.querySelectorAll('script[src]')).filter(function (script) {
            let src = script.getAttribute('src');
            return src && src.indexOf('axios.min.js') === -1 && src.indexOf('/js/site.js') === -1;
        });

        function chain(index) {
            if (index >= scripts.length) return;
            let script = document.createElement('script');
            script.src = scripts[index].getAttribute('src');
            script.addEventListener('load', function () { chain(index + 1); });
            script.addEventListener('error', function () { chain(index + 1); });
            document.body.appendChild(script);
        }

        chain(0);
    }

    Kela.navigate = function (url, opts) {
        opts = opts || {};
        let main = document.querySelector('main');
        if (!main) {
            window.location.href = url;
            return;
        }

        let target = new URL(url, window.location.origin);
        if (target.origin !== window.location.origin) {
            window.location.href = url;
            return;
        }

        let current = window.location.pathname + window.location.search;
        let nextPath = target.pathname + target.search;
        if (current === nextPath) {
            if (opts.replace) history.replaceState({ url: nextPath }, '', nextPath);
            return;
        }

        Kela.axios.get(target.pathname + target.search).then(function (res) {
            let doc = new DOMParser().parseFromString(res.data, 'text/html');
            let nextMain = doc.querySelector('main');
            if (!nextMain) {
                window.location.href = url;
                return;
            }

            let title = doc.querySelector('title');
            if (title) document.title = title.textContent;

            let csrf = doc.querySelector('meta[name="csrf-token"]');
            let currentMeta = document.querySelector('meta[name="csrf-token"]');
            if (csrf && currentMeta) currentMeta.setAttribute('content', csrf.getAttribute('content'));

            Kela.unmountPage();

            let header = document.querySelector('header');
            let nextHeader = doc.querySelector('header');
            if (header && nextHeader) header.outerHTML = nextHeader.outerHTML;
            main.outerHTML = nextMain.outerHTML;

            reExecuteScripts(doc);

            if (opts.replace) history.replaceState({ url: nextPath }, '', nextPath);
            else history.pushState({ url: nextPath }, '', nextPath);
        }).catch(function () {
            window.location.href = url;
        });
    };

    window.addEventListener('popstate', function (event) {
        if (event.state && event.state.url) {
            Kela.navigate(event.state.url, { replace: true });
        }
    });

    document.addEventListener('click', function (event) {
        let el = event.target;
        if (el && el.tagName === 'DIALOG' && el.open) el.close();
    });

    document.addEventListener('click', function (event) {
        if (!document.querySelector('main')) return;
        if (event.button !== 0 || event.defaultPrevented || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;

        let anchor = event.target.closest('a[href]');
        if (!anchor) return;
        if (anchor.hasAttribute('download') || anchor.target || anchor.getAttribute('href').charAt(0) === '#') return;
        let href = anchor.href;
        if (!href || href.indexOf(window.location.origin) !== 0) return;

        event.preventDefault();
        Kela.navigate(href);
    });

    document.addEventListener('submit', function (event) {
        let form = event.target;
        if (!form || (form.id !== 'logout-form' && form.id !== 'lang-form')) return;
        event.preventDefault();
        Kela.axios.post(form.action, new FormData(form)).then(function () {
            if (form.id === 'logout-form') window.location.href = '/auth/login';
            else if (document.querySelector('main')) Kela.navigate(location.pathname + location.search);
            else window.location.reload();
        }).catch(function () {
            if (form.id === 'logout-form') window.location.href = '/auth/login';
            else if (document.querySelector('main')) Kela.navigate(location.pathname + location.search);
            else window.location.reload();
        });
    });

    window.Kela = Kela;
})();
