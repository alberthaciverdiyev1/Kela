(function () {
    'use strict';

    const LIST_ID = 'workspaces-list';

    function listEl() {
        return document.getElementById(LIST_ID);
    }

    let paged = new Kela.PagedList({
        url: '/teacher/workspaces/table',
        target: '#' + LIST_ID
    });

    document.addEventListener('click', async function (event) {
        let del = event.target.closest('[data-delete-id]');
        if (del) {
            if (del.dataset.confirm && !confirm(del.dataset.confirm)) return;
            del.disabled = true;
            try {
                let res = await Kela.axios.delete(del.dataset.deleteUrl, {
                    params: { page: del.dataset.page || '1' }
                });
                let list = listEl();
                if (list) list.outerHTML = res.data;
            } catch (e) {
                del.disabled = false;
            }
            return;
        }

        let removeBtn = event.target.closest('[data-remove-student]');
        if (removeBtn) {
            removeBtn.disabled = true;
            try {
                let res = await Kela.axios.delete(
                    '/teacher/workspaces/' + removeBtn.dataset.workspaceId + '/students/' + removeBtn.dataset.removeStudent
                );
                let section = document.getElementById('workspace-students-section');
                if (section) section.outerHTML = res.data;
            } catch (e) {
                removeBtn.disabled = false;
            }
            return;
        }
    });

    async function submitForm(form, fieldsId) {
        let submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) submitBtn.disabled = true;

        try {
            let res = await Kela.axios.post(form.action, new FormData(form));
            if (submitBtn) submitBtn.disabled = false;

            let dialog = form.closest('dialog');
            if (dialog) dialog.close();

            if (res.data && res.data.redirect) {
                window.location.href = res.data.redirect;
                return;
            }

            let section = document.getElementById('workspace-students-section');
            if (section) section.outerHTML = res.data;
        } catch (err) {
            if (submitBtn) submitBtn.disabled = false;
            if (err.response && err.response.status === 422) {
                let fields = document.getElementById(fieldsId);
                if (fields) fields.outerHTML = err.response.data;
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

    let addForm = document.getElementById('add-students-form');
    if (addForm) {
        addForm.addEventListener('submit', function (event) {
            event.preventDefault();
            submitForm(addForm, null);
        });
    }
})();
