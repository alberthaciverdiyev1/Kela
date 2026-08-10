(function () {
    'use strict';

    let Kela = window.Kela || {};

    Kela.Pager = {
        render(opts) {
            let page = opts.page || 1;
            let totalPages = opts.totalPages || 1;
            if (totalPages <= 1) return '';
            let prevText = opts.prevText || Kela.t('pager.prev');
            let nextText = opts.nextText || Kela.t('pager.next');

            let out = '<div class="flex items-center justify-end gap-1 border-t border-base-300 px-4 py-3.5">';
            out += '<button type="button" class="pager-btn btn btn-sm btn-ghost"' + (page <= 1 ? ' disabled' : '') + ' data-page="' + (page - 1) + '">' + Kela.esc(prevText) + '</button>';

            for (let p = 1; p <= totalPages; p++) {
                if (p === page) {
                    out += '<span class="pager-btn btn btn-sm btn-primary">' + p + '</span>';
                } else if (p <= 2 || p >= totalPages - 1 || Math.abs(p - page) <= 1) {
                    out += '<button type="button" class="pager-btn btn btn-sm btn-ghost" data-page="' + p + '">' + p + '</button>';
                } else if (p === 3 || p === totalPages - 2) {
                    out += '<span class="px-1 text-base-content/40">…</span>';
                }
            }

            out += '<button type="button" class="pager-btn btn btn-sm btn-ghost"' + (page >= totalPages ? ' disabled' : '') + ' data-page="' + (page + 1) + '">' + Kela.esc(nextText) + '</button>';
            out += '</div>';
            return out;
        }
    };

    class PagedList {
        constructor(options) {
            this.url = options.url;
            this.targetSelector = options.target;
            this.render = options.render || null;
            this.params = options.params || function () {
                return {};
            };
            this._bind();
        }

        _bind() {
            Kela.onPageEvent('click', (event) => {
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
                if (target) target.innerHTML = this.render ? this.render(res.data) : res.data;
            } catch (e) {
            }
        }
    }

    Kela.PagedList = PagedList;
    window.Kela = Kela;
})();
