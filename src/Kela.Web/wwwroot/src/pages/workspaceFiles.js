(function () {
    'use strict';

    const section = document.getElementById('workspace-files');
    if (!section) return;

    const wsId = Number(section.dataset.workspaceId || document.querySelector('#workspace-files').dataset.workspaceId);

    const fm = Kela.fileManager({
        el: '#workspace-files',
        crumbEl: '#workspace-breadcrumb',
        treeUrl: '/teacher/workspaces/' + wsId + '/tree',
        context: 'workspace',
        nodeBaseUrl: '/teacher/nodes/',
        createFolderUrl: '/teacher/workspaces/' + wsId + '/folder',
        addContentUrl: '/teacher/workspaces/' + wsId + '/content',
        copyFolderUrl: '/teacher/workspaces/' + wsId + '/copy-folder'
    });
    if (!fm) return;

    const folderForm = document.getElementById('ws-folder-form');
    if (folderForm) {
        folderForm.addEventListener('submit', function (event) {
            event.preventDefault();
            const name = document.getElementById('ws-folder-name').value.trim();
            if (!name) return;
            const btn = folderForm.querySelector('button[type="submit"]');
            if (btn) btn.disabled = true;
            fm.createFolder(name).then(function () {
                document.getElementById('ws-folder-dialog').close();
                document.getElementById('ws-folder-name').value = '';
                Kela.notify.success(Kela.t('library.created'));
            }).catch(function () {
            }).finally(function () {
                if (btn) btn.disabled = false;
            });
        });
    }

    let contents = [];
    const addDialog = document.getElementById('ws-add-content-dialog');
    const addList = document.getElementById('ws-add-content-list');
    const addSearch = document.getElementById('ws-add-content-search');
    const addConfirm = document.getElementById('ws-add-content-confirm');

    function esc(v) {
        return Kela.esc ? Kela.esc(String(v == null ? '' : v)) : String(v);
    }

    function typeLabel(type) {
        return Kela.t(['contentType.Lesson', 'contentType.Quiz', 'contentType.Pdf', 'contentType.Video', 'contentType.Link'][type] || 'contentType.Lesson');
    }

    function renderAddList() {
        if (!addList) return;
        const q = addSearch ? addSearch.value.trim().toLowerCase() : '';
        const filtered = contents.filter(function (c) {
            return !q || c.title.toLowerCase().indexOf(q) !== -1;
        });
        if (!filtered.length) {
            addList.innerHTML = '<li class="p-3 text-sm text-base-content/50">' + esc(Kela.t('library.noContent')) + '</li>';
            return;
        }
        addList.innerHTML = filtered.map(function (c) {
            const status = c.isPublished
                ? '<span class="badge badge-success badge-xs">' + esc(Kela.t('library.published')) + '</span>'
                : '<span class="badge badge-warning badge-xs">' + esc(Kela.t('library.draft')) + '</span>';
            return '<li><label class="flex items-center gap-3 rounded-lg px-2 py-2">' +
                '<input type="radio" name="ws-add-content-item" value="' + c.id + '" class="radio radio-sm">' +
                '<span class="flex-1 truncate text-sm">' + esc(c.title) + '</span>' +
                '<span class="badge badge-ghost badge-xs">' + esc(typeLabel(c.type)) + '</span> ' + status +
                '</label></li>';
        }).join('');
    }

    if (addDialog) {
        const openBtn = document.querySelector('[onclick*="ws-add-content-dialog"]');
        if (openBtn) {
            openBtn.addEventListener('click', function () {
                Kela.axios.get('/teacher/library/contents').then(function (res) {
                    contents = res.data || [];
                    if (addSearch) addSearch.value = '';
                    renderAddList();
                }).catch(function () {
                });
            });
        }
        if (addSearch) {
            addSearch.addEventListener('input', renderAddList);
        }
        if (addConfirm) {
            addConfirm.addEventListener('click', function () {
                const selected = document.querySelector('input[name="ws-add-content-item"]:checked');
                if (!selected) return;
                addConfirm.disabled = true;
                Kela.axios.post('/teacher/workspaces/' + wsId + '/content', {
                    contentId: Number(selected.value),
                    parentId: fm.getCurrent()
                }).then(function () {
                    addDialog.close();
                    fm.refresh();
                    Kela.notify.success(Kela.t('library.added'));
                }).catch(function () {
                }).finally(function () {
                    addConfirm.disabled = false;
                });
            });
        }
    }

    let folders = [];
    const copyDialog = document.getElementById('ws-copy-dialog');
    const copyList = document.getElementById('ws-copy-list');
    const copySearch = document.getElementById('ws-copy-search');
    const copyConfirm = document.getElementById('ws-copy-confirm');

    function flattenFolders(nodes, depth, out) {
        nodes.forEach(function (n) {
            if (n.kind === 0) {
                out.push({ id: n.id, name: n.name, depth: depth });
                if (n.children && n.children.length) flattenFolders(n.children, depth + 1, out);
            }
        });
    }

    function renderCopyList() {
        if (!copyList) return;
        const q = copySearch ? copySearch.value.trim().toLowerCase() : '';
        const filtered = folders.filter(function (f) {
            return !q || f.name.toLowerCase().indexOf(q) !== -1;
        });
        if (!filtered.length) {
            copyList.innerHTML = '<li class="p-3 text-sm text-base-content/50">' + esc(Kela.t('library.noFolders')) + '</li>';
            return;
        }
        copyList.innerHTML = filtered.map(function (f) {
            return '<li><label class="flex items-center gap-3 rounded-lg px-2 py-2">' +
                '<input type="radio" name="ws-copy-item" value="' + f.id + '" class="radio radio-sm">' +
                '<span class="flex-1 truncate text-sm">' + new Array(f.depth * 2 + 1).join('&nbsp;') + Kela.icon('folder', 'w-4 h-4 opacity-60') + ' ' + esc(f.name) + '</span>' +
                '</label></li>';
        }).join('');
    }

    if (copyDialog) {
        const openBtn = document.querySelector('[onclick*="ws-copy-dialog"]');
        if (openBtn) {
            openBtn.addEventListener('click', function () {
                Kela.axios.get('/teacher/library/tree').then(function (res) {
                    folders = [];
                    flattenFolders(res.data || [], 0, folders);
                    if (copySearch) copySearch.value = '';
                    renderCopyList();
                }).catch(function () {
                });
            });
        }
        if (copySearch) {
            copySearch.addEventListener('input', renderCopyList);
        }
        if (copyConfirm) {
            copyConfirm.addEventListener('click', function () {
                const selected = document.querySelector('input[name="ws-copy-item"]:checked');
                if (!selected) return;
                copyConfirm.disabled = true;
                Kela.axios.post('/teacher/workspaces/' + wsId + '/copy-folder', {
                    sourceNodeId: Number(selected.value),
                    parentId: fm.getCurrent()
                }).then(function () {
                    copyDialog.close();
                    fm.refresh();
                    Kela.notify.success(Kela.t('library.copied'));
                }).catch(function () {
                }).finally(function () {
                    copyConfirm.disabled = false;
                });
            });
        }
    }
})();
