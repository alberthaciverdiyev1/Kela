(function () {
    'use strict';

    let Kela = window.Kela || {};

    class PagedList {
        constructor(options) {
            this.url = options.url;
            this.targetSelector = options.target;
            this.params = options.params || function () {
                return {};
            };
            this._bind();
        }

        _bind() {
            document.addEventListener('click', (event) => {
                let btn = event.target.closest('.pager-btn');
                if (!btn || btn.disabled || !btn.dataset.page) return;
                let target = document.querySelector(this.targetSelector);
                if (!target || !target.contains(btn)) return;
                this.load(parseInt(btn.dataset.page, 10));
            });
        }

        async load(page) {
            let params = {
                page: String(page)
            };
            Object.entries(this.params()).forEach(([key, value]) => {
                if (value === '' || value == null) return;
                params[key] = value;
            });

            try {
                let res = await Kela.axios.get(this.url, {
                    params: params
                });
                let target = document.querySelector(this.targetSelector);
                if (target) target.outerHTML = res.data;
            } catch (e) {
            }
        }
    }

    Kela.PagedList = PagedList;
    window.Kela = Kela;
})();
