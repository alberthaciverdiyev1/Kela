(function () {
    'use strict';

    let Kela = window.Kela || {};

    const TYPES = {
        0: { icon: 'book', labelKey: 'contentType.Lesson' },
        1: { icon: 'quiz', labelKey: 'contentType.Quiz' },
        2: { icon: 'pdf', labelKey: 'contentType.Pdf' },
        3: { icon: 'video', labelKey: 'contentType.Video' },
        4: { icon: 'link', labelKey: 'contentType.Link' }
    };

    Kela.fileManager = function (opts) {
        const listEl = document.querySelector(opts.el);
        const crumbEl = opts.crumbEl ? document.querySelector(opts.crumbEl) : null;
        const modalHost = document.getElementById('file-manager-modals');
        if (!listEl) return null;

        let tree = [];
        let nodeMap = {};
        let currentId = null;

        function esc(v) {
            return Kela.esc ? Kela.esc(String(v == null ? '' : v)) : String(v);
        }

        function t(key, params) {
            return Kela.t(key, params);
        }

        function indexTree(nodes) {
            nodes.forEach(function (n) {
                nodeMap[n.id] = n;
                if (n.children && n.children.length) indexTree(n.children);
            });
        }

        function load() {
            return Kela.axios.get(opts.treeUrl).then(function (res) {
                tree = res.data || [];
                nodeMap = {};
                indexTree(tree);
                render();
            }).catch(function () {
            });
        }

        function currentChildren() {
            if (currentId === null) return tree;
            let node = nodeMap[currentId];
            return node ? (node.children || []) : [];
        }

        function sortItems(items) {
            return items.slice().sort(function (a, b) {
                if (a.kind !== b.kind) return a.kind === 0 ? -1 : 1;
                return (a.position || 0) - (b.position || 0);
            });
        }

        function typeInfo(node) {
            let content = node.content;
            if (!content) return { icon: 'file', labelKey: 'contentType.Lesson' };
            return TYPES[content.type] || { icon: 'file', labelKey: 'contentType.Lesson' };
        }

        function render() {
            renderCrumb();
            renderList();
        }

        function renderCrumb() {
            if (!crumbEl) return;
            let path = [];
            let cur = currentId === null ? null : nodeMap[currentId];
            while (cur) {
                path.unshift(cur);
                cur = cur.parentId != null ? nodeMap[cur.parentId] : null;
            }
            let html = '<a href="#" data-crumb="root" class="btn btn-ghost btn-xs">' + esc(t('library.root')) + '</a>';
            path.forEach(function (n) {
                html += '<span class="text-base-content/30">/</span>';
                html += '<a href="#" data-crumb="' + n.id + '" class="btn btn-ghost btn-xs font-medium">' + esc(n.name) + '</a>';
            });
            crumbEl.innerHTML = html;
        }

        function renderList() {
            let items = sortItems(currentChildren());
            if (!items.length) {
                listEl.innerHTML = '<div class="card border border-base-200 bg-base-100 flex items-center justify-center gap-2 py-10 text-center">' +
                    '<span class="text-base-300">' + Kela.icon('folder', 'w-10 h-10') + '</span>' +
                    '<p class="text-sm text-base-content/50">' + esc(t('library.empty')) + '</p></div>';
                return;
            }

            let html = '<div class="card overflow-hidden border border-base-200 bg-base-100"><div class="overflow-x-auto"><table class="table">';
            html += '<thead><tr>' +
                '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + esc(t('library.name')) + '</th>' +
                '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + esc(t('library.type')) + '</th>' +
                '<th class="w-1 whitespace-nowrap bg-base-200/50 text-right"></th>' +
                '</tr></thead><tbody>';

            items.forEach(function (n) {
                if (n.kind === 0) {
                    html += folderRow(n);
                } else {
                    html += contentRow(n);
                }
            });

            html += '</tbody></table></div></div>';
            listEl.innerHTML = html;
        }

        function folderRow(n) {
            return '<tr class="hover:bg-base-200/50">' +
                '<td><a href="#" data-open-folder="' + n.id + '" class="flex items-center gap-2 font-medium text-primary hover:underline">' +
                Kela.icon('folder', 'w-4 h-4 opacity-70') + '<span>' + esc(n.name) + '</span></a></td>' +
                '<td><span class="badge badge-ghost">' + esc(t('library.folder')) + '</span></td>' +
                '<td class="w-1 whitespace-nowrap text-right">' + nodeActions(n) + '</td>' +
                '</tr>';
        }

        function contentRow(n) {
            let info = typeInfo(n);
            let content = n.content;
            let statusBadge = content && content.isPublished
                ? '<span class="badge badge-success badge-sm">' + esc(t('library.published')) + '</span>'
                : '<span class="badge badge-warning badge-sm">' + esc(t('library.draft')) + '</span>';
            let urlBtn = content && content.url
                ? '<button type="button" class="btn btn-sm btn-square btn-ghost" title="' + esc(t('library.open')) + '" data-open-url="' + esc(content.url) + '">' + Kela.icon('external-link', 'w-4 h-4') + '</button>'
                : '';

            return '<tr class="hover:bg-base-200/50">' +
                '<td><a href="#" data-open-content="' + n.id + '" class="flex items-center gap-2 font-medium">' +
                Kela.icon(info.icon, 'w-4 h-4 opacity-70') + '<span>' + esc(n.name) + '</span></a></td>' +
                '<td><span class="badge badge-ghost badge-sm">' + esc(t(info.labelKey)) + '</span> ' + statusBadge + '</td>' +
                '<td class="w-1 whitespace-nowrap text-right">' + urlBtn + nodeActions(n) + '</td>' +
                '</tr>';
        }

        function nodeActions(n) {
            let html = '';
            if (opts.context === 'library' && n.kind !== 0) {
                let published = n.content && n.content.isPublished;
                html += '<button type="button" class="btn btn-sm btn-square btn-ghost" title="' + esc(t('library.publish')) + '" data-toggle-publish="' + n.id + '">' +
                    Kela.icon(published ? 'eye-off' : 'eye', 'w-4 h-4') + '</button>';
            }
            html += '<button type="button" class="btn btn-sm btn-square btn-ghost" title="' + esc(t('library.rename')) + '" data-rename="' + n.id + '">' + Kela.icon('edit', 'w-4 h-4') + '</button>';
            html += '<button type="button" class="btn btn-sm btn-square btn-ghost" title="' + esc(t('library.move')) + '" data-move="' + n.id + '">' + Kela.icon('move', 'w-4 h-4') + '</button>';
            html += '<button type="button" class="btn btn-sm btn-square btn-ghost btn-error" title="' + esc(t('library.delete')) + '" data-delete-node="' + n.id + '" data-name="' + esc(n.name) + '">' + Kela.icon('trash', 'w-4 h-4') + '</button>';
            return html;
        }

        function findNode(id) {
            return nodeMap[id] || null;
        }

        function isDescendant(id, rootId) {
            let cur = findNode(id);
            while (cur && cur.parentId != null) {
                if (cur.parentId === rootId) return true;
                cur = findNode(cur.parentId);
            }
            return false;
        }

        function folderOptions(excludeId) {
            let html = '';
            function walk(nodes, depth) {
                nodes.forEach(function (n) {
                    if (n.kind !== 0) return;
                    if (excludeId !== undefined && (n.id === excludeId || isDescendant(n.id, excludeId))) return;
                    html += '<option value="' + n.id + '">' + new Array(depth * 2 + 1).join('&nbsp;') + esc(n.name) + '</option>';
                    if (n.children && n.children.length) walk(n.children, depth + 1);
                });
            }
            walk(tree, 0);
            return html;
        }

        function closeModal(dialog) {
            if (dialog) {
                dialog.close();
                dialog.remove();
            }
        }

        function showDialog(buildHtml, onMount) {
            if (!modalHost) return null;
            let dialog = document.createElement('dialog');
            dialog.className = 'modal';
            dialog.innerHTML = buildHtml();
            modalHost.appendChild(dialog);
            dialog.showModal();
            if (onMount) onMount(dialog);
            let closeBtn = dialog.querySelector('[data-close]');
            if (closeBtn) closeBtn.addEventListener('click', function () { closeModal(dialog); });
            return dialog;
        }

        function openRename(node) {
            showDialog(function () {
                return '<div class="modal-box">' +
                    '<div class="flex items-center justify-between border-b border-base-200 px-6 py-4"><h2 class="text-lg font-semibold tracking-tight">' + esc(t('library.rename')) + '</h2></div>' +
                    '<div class="px-6 py-5"><input id="rename-input" class="input w-full" type="text" value="' + esc(node.name) + '" autocomplete="off" /></div>' +
                    '<div class="flex justify-end gap-2 px-6 pb-5">' +
                    '<button type="button" data-close class="btn">' + esc(t('common.cancel')) + '</button>' +
                    '<button type="button" id="rename-confirm" class="btn btn-primary">' + esc(t('common.save')) + '</button>' +
                    '</div></div>';
            }, function (dialog) {
                let input = dialog.querySelector('#rename-input');
                input.focus();
                input.select();
                function save() {
                    let name = input.value.trim();
                    if (!name) return;
                    if (node.kind === 0) {
                        Kela.axios.put(opts.nodeBaseUrl + node.id, { name: name }).then(function () {
                            closeModal(dialog);
                            load();
                            Kela.notify.success(t('library.renamed'));
                        }).catch(function () { });
                    } else if (opts.context === 'library') {
                        Kela.axios.put(opts.updateContentUrl + node.content.id, {
                            title: name,
                            description: node.content.description,
                            url: node.content.url
                        }).then(function () {
                            closeModal(dialog);
                            load();
                            Kela.notify.success(t('library.renamed'));
                        }).catch(function () { });
                    } else {
                        Kela.axios.put(opts.nodeBaseUrl + node.id, { name: name }).then(function () {
                            closeModal(dialog);
                            load();
                            Kela.notify.success(t('library.renamed'));
                        }).catch(function () { });
                    }
                }
                dialog.querySelector('#rename-confirm').addEventListener('click', save);
                input.addEventListener('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); save(); } });
            });
        }

        function openMove(node) {
            showDialog(function () {
                let optsHtml = folderOptions(node.kind === 0 ? node.id : undefined);
                return '<div class="modal-box">' +
                    '<div class="flex items-center justify-between border-b border-base-200 px-6 py-4"><h2 class="text-lg font-semibold tracking-tight">' + esc(t('library.moveTo')) + '</h2></div>' +
                    '<div class="px-6 py-5">' +
                    (optsHtml ? '<select id="move-folder-select" class="select w-full">' + optsHtml + '</select>' : '<p class="text-sm text-base-content/50">' + esc(t('library.noFolders')) + '</p>') +
                    '</div>' +
                    '<div class="flex justify-end gap-2 px-6 pb-5">' +
                    '<button type="button" data-close class="btn">' + esc(t('common.cancel')) + '</button>' +
                    (optsHtml ? '<button type="button" id="move-confirm" class="btn btn-primary">' + esc(t('common.save')) + '</button>' : '') +
                    '</div></div>';
            }, function (dialog) {
                let confirm = dialog.querySelector('#move-confirm');
                if (!confirm) return;
                confirm.addEventListener('click', function () {
                    let parentId = Number(dialog.querySelector('#move-folder-select').value);
                    Kela.axios.put(opts.nodeBaseUrl + node.id, { parentId: parentId }).then(function () {
                        closeModal(dialog);
                        load();
                        Kela.notify.success(t('library.moved'));
                    }).catch(function () { });
                });
            });
        }

        function openPreview(node) {
            let content = node.content || {};
            let info = typeInfo(node);
            showDialog(function () {
                let body = '<p class="text-sm text-base-content/70">' + (content.description ? esc(content.description) : esc(t('library.noDescription'))) + '</p>';
                if (content.url) {
                    body += '<div class="mt-3"><a class="link text-sm text-primary" href="' + esc(content.url) + '" target="_blank" rel="noopener">' + esc(content.url) + '</a></div>';
                }
                let status = content.isPublished
                    ? '<span class="badge badge-success">' + esc(t('library.published')) + '</span>'
                    : '<span class="badge badge-warning">' + esc(t('library.draft')) + '</span>';
                return '<div class="modal-box">' +
                    '<div class="flex items-center justify-between border-b border-base-200 px-6 py-4">' +
                    '<h2 class="text-lg font-semibold tracking-tight">' + Kela.icon(info.icon, 'w-5 h-5 inline-block mr-2') + esc(node.name) + '</h2>' + status +
                    '</div>' +
                    '<div class="px-6 py-5 flex flex-col gap-3">' + body + '</div>' +
                    '<div class="flex justify-end gap-2 px-6 pb-5">' +
                    (content.url ? '<button type="button" data-open-url class="btn btn-primary">' + esc(t('library.open')) + '</button>' : '') +
                    '<button type="button" data-close class="btn">' + esc(t('common.cancel')) + '</button>' +
                    '</div></div>';
            }, function (dialog) {
                let openBtn = dialog.querySelector('[data-open-url]');
                if (openBtn) openBtn.addEventListener('click', function () { window.open(content.url, '_blank'); });
            });
        }

        function deleteNode(node) {
            let name = node.name;
            let message = t('library.deleteConfirm', { name: name });
            if (!confirm(message)) return;

            let req;
            if (node.kind !== 0 && opts.context === 'library') {
                req = Kela.axios.delete(opts.deleteContentUrl + node.content.id);
            } else {
                req = Kela.axios.delete(opts.nodeBaseUrl + node.id);
            }

            req.then(function () {
                load();
                Kela.notify.success(t('library.deleted'));
            }).catch(function () { });
        }

        function togglePublish(node) {
            if (!node.content) return;
            let published = !node.content.isPublished;
            Kela.axios.put(opts.publishContentUrl + node.content.id + '/publish?published=' + published).then(function () {
                node.content.isPublished = published;
                render();
                Kela.notify.success(published ? t('library.publishedOk') : t('library.draftOk'));
            }).catch(function () { });
        }

        function handleListClick(event) {
            let el = event.target.closest('a,button');
            if (!el) return;

            let openFolder = el.closest('[data-open-folder]');
            if (openFolder) {
                event.preventDefault();
                currentId = Number(openFolder.getAttribute('data-open-folder'));
                render();
                return;
            }

            let openContent = el.closest('[data-open-content]');
            if (openContent) {
                event.preventDefault();
                let node = findNode(Number(openContent.getAttribute('data-open-content')));
                if (node) {
                    if (opts.context === 'library' && node.content && node.content.type === 1) {
                        Kela.navigate('/teacher/quizzes/' + node.content.id);
                        return;
                    }
                    openPreview(node);
                }
                return;
            }

            let openUrl = el.closest('[data-open-url]');
            if (openUrl) {
                event.preventDefault();
                window.open(openUrl.getAttribute('data-open-url'), '_blank');
                return;
            }

            let rename = el.closest('[data-rename]');
            if (rename) {
                let node = findNode(Number(rename.getAttribute('data-rename')));
                if (node) openRename(node);
                return;
            }

            let move = el.closest('[data-move]');
            if (move) {
                let node = findNode(Number(move.getAttribute('data-move')));
                if (node) openMove(node);
                return;
            }

            let del = el.closest('[data-delete-node]');
            if (del) {
                let node = findNode(Number(del.getAttribute('data-delete-node')));
                if (node) deleteNode(node);
                return;
            }

            let publish = el.closest('[data-toggle-publish]');
            if (publish) {
                let node = findNode(Number(publish.getAttribute('data-toggle-publish')));
                if (node) togglePublish(node);
                return;
            }
        }

        function handleCrumbClick(event) {
            let crumb = event.target.closest('[data-crumb]');
            if (!crumb) return;
            event.preventDefault();
            let value = crumb.getAttribute('data-crumb');
            currentId = value === 'root' ? null : Number(value);
            render();
        }

        Kela.onPageEvent('click', function (event) {
            if (listEl.contains(event.target)) handleListClick(event);
            if (crumbEl && crumbEl.contains(event.target)) handleCrumbClick(event);
        });

        load();

        return {
            refresh: function () { return load(); },
            getCurrent: function () { return currentId; },
            open: function (id) {
                currentId = (id === undefined || id === null || id === 'root') ? null : Number(id);
                render();
            },
            createFolder: function (name) {
                return Kela.axios.post(opts.createFolderUrl, {
                    name: name,
                    parentId: currentId
                }).then(function () {
                    load();
                });
            },
            createContent: function (payload) {
                payload.parentId = currentId;
                return Kela.axios.post(opts.createContentUrl, payload).then(function (res) {
                    load();
                    return res;
                });
            }
        };
    };

    window.Kela = Kela;
})();
