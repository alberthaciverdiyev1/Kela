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

    function display(v) {
        if (v == null || String(v).trim() === '') return '-';
        return v;
    }

    function fmtDate(v) {
        if (!v) return '-';
        let s = String(v);
        if (s.length === 10 && s.charAt(4) === '-') {
            return s.slice(8, 10) + '.' + s.slice(5, 7) + '.' + s.slice(0, 4);
        }
        let d = new Date(s);
        if (isNaN(d.getTime())) return '-';
        return String(d.getDate()).padStart(2, '0') + '.' + String(d.getMonth() + 1).padStart(2, '0') + '.' + d.getFullYear();
    }

    function renderStudents(data) {
        let items = data.items || [];
        if (!items.length) {
            return '<div class="card border border-base-200 bg-base-100 items-center text-center py-10 px-4">' +
                '<span class="text-base-300 mb-3">' + Kela.icon('students', 'w-12 h-12') + '</span>' +
                '<p class="text-base-content/60 max-w-sm">' + Kela.esc(Kela.t('students.empty')) + '</p></div>';
        }

        let totalPages = Math.ceil(data.totalCount / data.pageSize);
        let html = '<div class="card overflow-hidden border border-base-200 bg-base-100"><div class="overflow-x-auto"><table class="table">';
        html += '<thead><tr>';
        ['students.col.firstName', 'students.col.lastName', 'students.col.phone', 'students.col.email', 'students.col.birthDate', 'students.col.city'].forEach(function (key) {
            html += '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + Kela.esc(Kela.t(key)) + '</th>';
        });
        html += '<th class="w-1 whitespace-nowrap text-right"></th>';
        html += '</tr></thead><tbody>';

        items.forEach(function (s) {
            let fullName = ((s.firstName || '') + ' ' + (s.lastName || '')).trim();
            html += '<tr>';
            html += '<td>' + Kela.esc(display(s.firstName)) + '</td>';
            html += '<td>' + Kela.esc(display(s.lastName)) + '</td>';
            html += '<td>' + Kela.esc(display(s.phoneNumber)) + '</td>';
            html += '<td class="text-base-content/60">' + Kela.esc(s.email || '') + '</td>';
            html += '<td>' + Kela.esc(fmtDate(s.birthDate)) + '</td>';
            html += '<td>' + Kela.esc(display(s.cityName)) + '</td>';
            html += '<td class="w-1 whitespace-nowrap text-right">' +
                '<button type="button" class="btn btn-sm btn-square btn-ghost btn-error" title="' + Kela.esc(Kela.t('students.delete')) + '"' +
                ' data-delete-id="' + s.id + '" data-delete-url="/teacher/students/' + s.id + '/delete"' +
                ' data-page="' + data.page + '" data-confirm="' + Kela.esc(Kela.t('students.deleteConfirm', { name: fullName })) + '">' +
                Kela.icon('trash', 'w-4 h-4') + '</button></td>';
            html += '</tr>';
        });

        html += '</tbody></table></div>';
        html += Kela.Pager.render({ page: data.page, totalPages: totalPages });
        html += '</div>';
        return html;
    }

    function renderCredentials(c) {
        return '<div class="modal-box p-0 overflow-hidden">' +
            '<div class="flex items-center justify-between border-b border-base-200 px-6 py-4"><h2 class="text-lg font-semibold tracking-tight text-base-content">' + Kela.esc(Kela.t('students.createdTitle')) + '</h2></div>' +
            '<div class="flex flex-col gap-4 px-6 py-5">' +
            '<div class="alert alert-success">' + Kela.icon('check', 'w-4 h-4') + '<span>' + Kela.esc(Kela.t('students.createdMsg')) + '</span></div>' +
            '<div class="rounded-xl border border-base-300 p-4">' +
            '<div class="mb-2 text-xs text-base-content/60">' + Kela.esc(Kela.t('students.emailPassword')) + '</div>' +
            '<div class="flex items-center gap-2">' +
            '<input id="credentials-text" class="input w-full font-mono text-sm" readonly value="' + Kela.esc(c.email + ' | ' + c.password) + '">' +
            '<button type="button" id="credentials-copy" class="btn btn-sm btn-square btn-ghost btn-info" title="' + Kela.esc(Kela.t('students.copy')) + '">' + Kela.icon('copy', 'w-4 h-4') + '</button>' +
            '</div></div>' +
            '<div class="alert alert-warning">' + Kela.icon('lock', 'w-4 h-4') + '<span>' + Kela.esc(Kela.t('students.passwordOnce')) + '</span></div>' +
            '<div class="flex justify-end gap-2">' +
            '<button type="button" class="btn btn-primary" onclick="document.getElementById(\'credentials-dialog\').close()">' + Kela.esc(Kela.t('common.confirm')) + '</button>' +
            '</div></div></div>';
    }

    let paged = new Kela.PagedList({
        url: '/teacher/students/table',
        target: '#' + LIST_ID,
        render: renderStudents,
        params: function () {
            return { search: searchValue() };
        }
    });
    paged.load(1);

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

    Kela.onPageEvent('click', async function (event) {
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
                if (list) list.innerHTML = renderStudents(res.data);
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

                let creds = document.getElementById('credentials-dialog');
                if (creds) {
                    creds.innerHTML = renderCredentials(res.data);
                    if (typeof creds.showModal === 'function') creds.showModal();
                }
            } catch (err) {
                if (submitBtn) submitBtn.disabled = false;
                if (err.response && err.response.status === 422) {
                    let fields = document.getElementById('create-form-fields');
                    if (fields) Kela.applyErrors(fields, err.response.data.errors);
                    return;
                }
                return;
            }

            let fields = document.getElementById('create-form-fields');
            if (fields) {
                fields.querySelectorAll('input, textarea, select').forEach(function (el) {
                    el.value = '';
                });
            }

            paged.load(1);
        });
    }
})();
