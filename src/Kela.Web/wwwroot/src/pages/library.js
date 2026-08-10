(function () {
    'use strict';

    const page = document.getElementById('library-page');
    if (!page) return;

    const TYPE = page.dataset.type ? Number(page.dataset.type) : null;

    const fm = Kela.fileManager({
        el: '#library-list',
        crumbEl: '#library-breadcrumb',
        treeUrl: '/teacher/library/tree',
        context: 'library',
        type: TYPE,
        nodeBaseUrl: '/teacher/nodes/',
        updateContentUrl: '/teacher/library/content/',
        publishContentUrl: '/teacher/library/content/',
        deleteContentUrl: '/teacher/library/content/',
        createFolderUrl: '/teacher/library/folder',
        createContentUrl: '/teacher/library/content'
    });
    if (!fm) return;

    const folderForm = document.getElementById('library-folder-form');
    if (folderForm) {
        folderForm.addEventListener('submit', function (event) {
            event.preventDefault();
            const name = document.getElementById('library-folder-name').value.trim();
            if (!name) return;
            const btn = folderForm.querySelector('button[type="submit"]');
            if (btn) btn.disabled = true;
            fm.createFolder(name).then(function () {
                document.getElementById('library-folder-dialog').close();
                document.getElementById('library-folder-name').value = '';
                Kela.notify.success(Kela.t('library.created'));
            }).catch(function () {
            }).finally(function () {
                if (btn) btn.disabled = false;
            });
        });
    }

    const contentForm = document.getElementById('library-content-form');
    if (contentForm) {
        contentForm.addEventListener('submit', function (event) {
            event.preventDefault();
            const title = document.getElementById('library-content-title').value.trim();
            if (!title) return;
            const type = TYPE != null ? TYPE : Number(document.getElementById('library-content-type').value);
            const url = document.getElementById('library-content-url').value.trim();
            const description = document.getElementById('library-content-desc').value.trim();
            const btn = contentForm.querySelector('button[type="submit"]');
            if (btn) btn.disabled = true;
            fm.createContent({ title: title, type: type, url: url || null, description: description || null }).then(function (res) {
                document.getElementById('library-content-dialog').close();
                document.getElementById('library-content-title').value = '';
                document.getElementById('library-content-url').value = '';
                document.getElementById('library-content-desc').value = '';
                Kela.notify.success(Kela.t('library.created'));
                let contentId = res && res.data ? res.data.contentId : null;
                if (type === 1 && contentId) {
                    Kela.navigate('/teacher/quizzes/' + contentId);
                }
            }).catch(function () {
            }).finally(function () {
                if (btn) btn.disabled = false;
            });
        });
    }
})();
