(function () {
    'use strict';

    const LIST_ID = 'workspaces-list';
    const SEARCH_ID = 'workspaces-search';

    function listEl() {
        return document.getElementById(LIST_ID);
    }

    function searchValue() {
        let input = document.getElementById(SEARCH_ID);
        return input ? input.value.trim() : '';
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

    function fullName(s) {
        return ((s.firstName || '') + ' ' + (s.lastName || '')).trim();
    }

    function renderWorkspaces(data) {
        let items = data.items || [];
        if (!items.length) {
            return '<div class="card border border-base-200 bg-base-100 items-center text-center py-10 px-4">' +
                '<span class="text-base-300 mb-3">' + Kela.icon('folder', 'w-12 h-12') + '</span>' +
                '<p class="text-base-content/60 max-w-sm">' + Kela.esc(Kela.t('workspaces.empty')) + '</p></div>';
        }

        let totalPages = Math.ceil(data.totalCount / (data.pageSize || 20));
        let html = '<div class="card overflow-hidden border border-base-200 bg-base-100"><div class="overflow-x-auto"><table class="table">';
        html += '<thead><tr>';
        html += '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + Kela.esc(Kela.t('workspaces.col.name')) + '</th>';
        html += '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + Kela.esc(Kela.t('workspaces.col.students')) + '</th>';
        html += '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + Kela.esc(Kela.t('workspaces.col.created')) + '</th>';
        html += '<th class="w-1 whitespace-nowrap text-right"></th>';
        html += '</tr></thead><tbody>';

        items.forEach(function (w) {
            html += '<tr>';
            html += '<td><a class="flex items-center gap-2 font-medium text-primary hover:underline" href="/teacher/workspaces/' + w.id + '">' + Kela.icon('folder', 'w-4 h-4 opacity-60') + '<span>' + Kela.esc(w.name) + '</span></a></td>';
            html += '<td>' + w.studentCount + '</td>';
            html += '<td class="text-base-content/60">' + Kela.esc(fmtDate(w.createdAt)) + '</td>';
            html += '<td class="w-1 whitespace-nowrap text-right">' +
                '<button type="button" class="btn btn-sm btn-square btn-ghost btn-error" title="' + Kela.esc(Kela.t('students.delete')) + '"' +
                ' data-delete-id="' + w.id + '" data-delete-url="/teacher/workspaces/' + w.id + '/delete"' +
                ' data-page="' + data.page + '" data-confirm="' + Kela.esc(Kela.t('workspaces.deleteConfirm', { name: w.name })) + '">' +
                Kela.icon('trash', 'w-4 h-4') + '</button></td>';
            html += '</tr>';
        });

        html += '</tbody></table></div>';
        html += Kela.Pager.render({ page: data.page, totalPages: totalPages });
        html += '</div>';
        return html;
    }

    function renderStudentsSection(data) {
        let w = data.workspace;
        let students = w.students || [];
        let available = data.availableStudents || [];
        let html = '<div class="card overflow-hidden border border-base-200 bg-base-100">';
        html += '<div class="flex items-center justify-between border-b border-base-200 px-5 py-4">' +
            '<h2 class="text-base font-bold">' + Kela.esc(Kela.t('workspaces.detail.students')) + '</h2>' +
            '<span class="badge badge-ghost">' + w.studentCount + '</span></div>';

        if (!students.length) {
            html += '<div class="flex flex-col items-center gap-2 px-5 py-10 text-center">' +
                '<span class="text-base-300">' + Kela.icon('students', 'w-10 h-10') + '</span>' +
                '<p class="text-sm text-base-content/50">' + Kela.esc(Kela.t('workspaces.detail.emptyStudents')) + '</p></div>';
        } else {
            html += '<div class="overflow-x-auto"><table class="table"><thead><tr>' +
                '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + Kela.esc(Kela.t('students.col.firstName')) + '</th>' +
                '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + Kela.esc(Kela.t('students.col.email')) + '</th>' +
                '<th class="w-1 whitespace-nowrap bg-base-200/50 text-right"></th>' +
                '</tr></thead><tbody>';
            students.forEach(function (s) {
                html += '<tr>';
                html += '<td>' + Kela.esc(fullName(s)) + '</td>';
                html += '<td class="text-base-content/60">' + Kela.esc(s.email || '') + '</td>';
                html += '<td class="w-1 whitespace-nowrap text-right">' +
                    '<button type="button" class="btn btn-sm btn-square btn-ghost btn-error" title="' + Kela.esc(Kela.t('workspaces.detail.remove')) + '"' +
                    ' data-remove-student="' + s.id + '" data-workspace-id="' + w.id + '">' +
                    Kela.icon('user-minus', 'w-4 h-4') + '</button></td>';
                html += '</tr>';
            });
            html += '</tbody></table></div>';
        }
        html += '</div>';

        html += '<div class="card border border-base-200 bg-base-100">';
        html += '<div class="border-b border-base-200 px-5 py-4"><h2 class="text-base font-bold">' + Kela.esc(Kela.t('workspaces.detail.addStudents')) + '</h2></div>';
        html += '<div class="px-5 py-4">';
        html += '<label class="input flex items-center gap-2 mb-3 max-w-sm">' +
            Kela.icon('search', 'w-4 h-4 opacity-40') +
            '<input id="available-students-search" type="search" class="grow" placeholder="' + Kela.esc(Kela.t('nav.search')) + '" autocomplete="off"></label>';
        if (!available.length) {
            let emptyKey = data.totalAvailable === 0 ? 'workspaces.detail.noAvailable' : 'workspaces.detail.noMatches';
            html += '<p class="text-sm text-base-content/50">' + Kela.esc(Kela.t(emptyKey)) + '</p>';
        } else {
            html += '<form id="add-students-form" method="post" action="/teacher/workspaces/' + w.id + '/students" novalidate>';
            html += '<div class="grid grid-cols-1 gap-2 sm:grid-cols-2">';
            available.forEach(function (s) {
                html += '<label class="flex items-center gap-3 rounded-box border border-base-200 px-3 py-2.5">' +
                    '<input type="checkbox" name="studentIds" value="' + s.userId + '" class="checkbox checkbox-sm">' +
                    '<span class="text-sm">' + Kela.esc(fullName(s)) + '</span></label>';
            });
            html += '</div><div class="mt-4 flex justify-end">' +
                '<button type="submit" class="btn btn-primary">' + Kela.icon('user-plus') + Kela.esc(Kela.t('workspaces.detail.add')) + '</button>' +
                '</div></form>';
        }
        if (data.totalPages > 1) {
            html += '<div id="available-students-pager" class="mt-2">' +
                Kela.Pager.render({ page: data.page, totalPages: data.totalPages }) + '</div>';
        }
        html += '</div></div>';
        return html;
    }

    function renderWorkspaceData(data) {
        let w = data.workspace;
        let statCount = document.getElementById('stat-student-count');
        if (statCount) statCount.textContent = String(w.studentCount);
        let statCreated = document.getElementById('stat-created');
        if (statCreated) statCreated.textContent = fmtDate(w.createdAt);
        let section = document.getElementById('workspace-students-section');
        if (section) section.innerHTML = renderStudentsSection(data);
    }

    async function loadWorkspaceData(id, search, page) {
        try {
            let res = await Kela.axios.get('/teacher/workspaces/' + id + '/data', {
                params: { search: search || '', page: page || 1 }
            });
            renderWorkspaceData(res.data);
        } catch (e) {
        }
    }

    let list = listEl();
    if (list) {
        let paged = new Kela.PagedList({
            url: '/teacher/workspaces/table',
            target: '#' + LIST_ID,
            render: renderWorkspaces,
            params: function () {
                return { search: searchValue() };
            }
        });
        paged.load(1);

        let searchInput = document.getElementById(SEARCH_ID);
        if (searchInput) {
            let debounce;
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
                let params = { page: del.dataset.page || '1' };
                let search = searchValue();
                if (search) params.search = search;
                try {
                    let res = await Kela.axios.delete(del.dataset.deleteUrl, {
                        params: params
                    });
                    let listEl2 = listEl();
                    if (listEl2) listEl2.innerHTML = renderWorkspaces(res.data);
                    Kela.notify.success(Kela.t('workspaces.deleted'));
                } catch (e) {
                    del.disabled = false;
                }
            }
        });
    }

    let section = document.getElementById('workspace-students-section');
    if (section) {
        let wsId = section.dataset.workspaceId;
        let availableState = { search: '', page: 1 };
        let avDebounce;

        function refreshAvailable() {
            loadWorkspaceData(wsId, availableState.search, availableState.page);
        }

        loadWorkspaceData(wsId, '', 1);

        Kela.onPageEvent('click', async function (event) {
            let removeBtn = event.target.closest('[data-remove-student]');
            if (removeBtn) {
                removeBtn.disabled = true;
                try {
                    await Kela.axios.delete(
                        '/teacher/workspaces/' + removeBtn.dataset.workspaceId + '/students/' + removeBtn.dataset.removeStudent
                    );
                    refreshAvailable();
                    Kela.notify.success(Kela.t('workspaces.removedStudent'));
                } catch (e) {
                    removeBtn.disabled = false;
                }
                return;
            }

            let pagerBtn = event.target.closest('#available-students-pager .pager-btn');
            if (pagerBtn && !pagerBtn.disabled && pagerBtn.dataset.page) {
                availableState.page = parseInt(pagerBtn.dataset.page, 10);
                refreshAvailable();
            }
        });

        Kela.onPageEvent('input', function (event) {
            let input = event.target;
            if (!input || input.id !== 'available-students-search') return;
            clearTimeout(avDebounce);
            avDebounce = setTimeout(function () {
                availableState.search = input.value.trim();
                availableState.page = 1;
                refreshAvailable();
            }, 500);
        });
    }

    async function submitForm(form, fieldsId) {
        let submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) submitBtn.disabled = true;

        try {
            let res = await Kela.axios.post(form.action, new FormData(form));
            if (submitBtn) submitBtn.disabled = false;

            let dialog = form.closest('dialog');
            if (dialog) dialog.close();

            if (res.data && res.data.redirect) {
                Kela.navigate(res.data.redirect);
            }
        } catch (err) {
            if (submitBtn) submitBtn.disabled = false;
            if (err.response && err.response.status === 422) {
                let fields = document.getElementById(fieldsId);
                if (fields) Kela.applyErrors(fields, err.response.data.errors);
            }
        }
    }

    let createForm = document.querySelector('#create-workspace-dialog form');
    if (createForm) {
        createForm.addEventListener('submit', function (event) {
            event.preventDefault();
            submitForm(createForm, 'create-form-fields');
        });
    }

    let renameForm = document.querySelector('#rename-workspace-dialog form');
    if (renameForm) {
        renameForm.addEventListener('submit', function (event) {
            event.preventDefault();
            submitForm(renameForm, 'rename-form-fields');
        });
    }

    Kela.onPageEvent('submit', function (event) {
        if (event.target && event.target.id === 'add-students-form') {
            event.preventDefault();
            let form = event.target;
            let submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) submitBtn.disabled = true;
            Kela.axios.post(form.action, new FormData(form)).then(function (res) {
                if (submitBtn) submitBtn.disabled = false;
                renderWorkspaceData(res.data);
                Kela.notify.success(Kela.t('workspaces.addedStudents'));
            }).catch(function () {
                if (submitBtn) submitBtn.disabled = false;
            });
        }
    });
})();
