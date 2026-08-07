(function () {
    'use strict';

    const LIST_ID = 'students-list';
    const SEARCH_ID = 'students-search';

    function searchValue() {
        let input = document.getElementById(SEARCH_ID);
        return input ? input.value.trim() : '';
    }

    function listEl() {
        return document.getElementById(LIST_ID);
    }

    let paged = new Kela.PagedList({
        url: '/teacher/students/table',
        target: '#' + LIST_ID,
        params: function () {
            return { search: searchValue() };
        }
    });

    let debounce;
    let searchInput = document.getElementById(SEARCH_ID);
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            clearTimeout(debounce);
            debounce = setTimeout(function () {
                paged.load(1);
            }, 500);
        });
        searchInput.addEventListener('search', function () {
            paged.load(1);
        });
    }

    document.addEventListener('click', async function (event) {
        let del = event.target.closest('[data-delete-id]');
        if (del) {
            if (del.dataset.confirm && !confirm(del.dataset.confirm)) return;
            del.disabled = true;
            let params = {
                page: del.dataset.page || '1'
            };
            let search = searchValue();
            if (search) params.search = search;
            try {
                let res = await Kela.axios.delete(del.dataset.deleteUrl, {
                    params: params
                });
                let list = listEl();
                if (list) list.outerHTML = res.data;
            } catch (e) {
                del.disabled = false;
            }
            return;
        }

        let copyBtn = event.target.closest('#credentials-copy');
        if (copyBtn) {
            let input = document.getElementById('credentials-text');
            if (input) {
                try {
                    await navigator.clipboard.writeText(input.value);
                } catch (e) {
                }
                let old = copyBtn.innerHTML;
                copyBtn.innerHTML = '&#10003;';
                setTimeout(function () {
                    copyBtn.innerHTML = old;
                }, 1200);
            }
        }
    });

    let createForm = document.querySelector('#create-student-dialog form');
    if (createForm) {
        createForm.addEventListener('submit', async function (event) {
            event.preventDefault();
            let submitBtn = createForm.querySelector('button[type="submit"]');
            if (submitBtn) submitBtn.disabled = true;

            try {
                let res = await Kela.axios.post(createForm.action, new FormData(createForm));

                let dialog = createForm.closest('dialog');
                if (dialog) dialog.close();
                if (submitBtn) submitBtn.disabled = false;

                let placeholder = document.getElementById('credentials-dialog');
                if (placeholder) placeholder.outerHTML = res.data;
                let creds = document.getElementById('credentials-dialog');
                if (creds && typeof creds.showModal === 'function') creds.showModal();
            } catch (err) {
                if (submitBtn) submitBtn.disabled = false;
                if (err.response && err.response.status === 422) {
                    let fields = document.getElementById('create-form-fields');
                    if (fields) fields.outerHTML = err.response.data;
                    return;
                }
                return;
            }

            let fields = document.getElementById('create-form-fields');
            if (fields) {
                fields.querySelectorAll('input, textarea, select').forEach(function (el) {
                    el.value = '';
                });
                fields.querySelectorAll('.field-error, .validation-summary').forEach(function (el) {
                    el.remove();
                });
            }

            paged.load(1);
        });
    }
})();
