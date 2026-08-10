(function () {
    'use strict';

    const page = document.getElementById('questions-page');
    if (!page) return;

    const LIST_ID = 'questions-list';
    const SEARCH_ID = 'questions-search';

    let items = [];

    function esc(v) {
        return Kela.esc ? Kela.esc(String(v == null ? '' : v)) : String(v);
    }

    function t(key, params) {
        return Kela.t(key, params);
    }

    function letters() {
        return ['A', 'B', 'C', 'D', 'E'];
    }

    function optionList(q) {
        let labels = letters();
        let opts = [];
        [q.optionA, q.optionB, q.optionC, q.optionD, q.optionE].forEach(function (v, i) {
            if (v) opts.push(labels[i] + '. ' + v);
        });
        return opts;
    }

    function searchValue() {
        let input = document.getElementById(SEARCH_ID);
        return input ? input.value.trim().toLowerCase() : '';
    }

    function filtered() {
        let q = searchValue();
        if (!q) return items;
        return items.filter(function (it) {
            return it.text.toLowerCase().indexOf(q) !== -1
                || optionList(it).join(' ').toLowerCase().indexOf(q) !== -1;
        });
    }

    function renderList() {
        let el = document.getElementById(LIST_ID);
        if (!el) return;
        let list = filtered();
        if (!list.length) {
            el.innerHTML = '<div class="card border border-base-200 bg-base-100 flex items-center justify-center gap-2 py-10 text-center">' +
                '<span class="text-base-300">' + Kela.icon('quiz', 'w-10 h-10') + '</span>' +
                '<p class="text-sm text-base-content/50">' + esc(t('questions.empty')) + '</p></div>';
            return;
        }

        let html = '<div class="card overflow-hidden border border-base-200 bg-base-100"><div class="overflow-x-auto"><table class="table">';
        html += '<thead><tr>' +
            '<th class="w-10 bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">#</th>' +
            '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + esc(t('questions.text')) + '</th>' +
            '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + esc(t('questions.options')) + '</th>' +
            '<th class="whitespace-nowrap bg-base-200/50 text-xs font-semibold uppercase tracking-wider text-base-content/50">' + esc(t('questions.correct')) + '</th>' +
            '<th class="w-1 whitespace-nowrap bg-base-200/50 text-right"></th>' +
            '</tr></thead><tbody>';

        list.forEach(function (q, idx) {
            let opts = optionList(q);
            let correct = letters()[q.correctOption] || '';
            html += '<tr>';
            html += '<td class="text-base-content/40">' + (idx + 1) + '</td>';
            html += '<td class="max-w-md"><p class="font-medium">' + esc(q.text) + '</p></td>';
            html += '<td><div class="flex flex-wrap gap-1">' + opts.map(function (o, i) {
                let cls = i === q.correctOption ? 'badge badge-success badge-sm' : 'badge badge-ghost badge-sm';
                return '<span class="' + cls + '">' + esc(o) + '</span>';
            }).join('') + '</div></td>';
            html += '<td><span class="badge badge-primary badge-sm">' + esc(correct) + '</span></td>';
            html += '<td class="w-1 whitespace-nowrap text-right">' +
                '<button type="button" class="btn btn-sm btn-square btn-ghost" title="' + esc(t('questions.edit')) + '" data-edit-id="' + q.id + '">' + Kela.icon('edit', 'w-4 h-4') + '</button>' +
                '<button type="button" class="btn btn-sm btn-square btn-ghost btn-error" title="' + esc(t('questions.delete')) + '" data-delete-id="' + q.id + '" data-confirm="' + esc(t('questions.deleteConfirm', { text: q.text })) + '">' + Kela.icon('trash', 'w-4 h-4') + '</button>' +
                '</td>';
            html += '</tr>';
        });

        html += '</tbody></table></div></div>';
        el.innerHTML = html;
    }

    function load() {
        Kela.axios.get('/teacher/questions/list').then(function (res) {
            items = res.data || [];
            renderList();
        }).catch(function () { });
    }

    function resetForm() {
        document.getElementById('question-id').value = '';
        document.getElementById('question-text').value = '';
        document.getElementById('question-option-a').value = '';
        document.getElementById('question-option-b').value = '';
        document.getElementById('question-option-c').value = '';
        document.getElementById('question-option-d').value = '';
        document.getElementById('question-option-e').value = '';
        document.getElementById('question-correct').value = '0';
    }

    function readForm() {
        return {
            text: document.getElementById('question-text').value.trim(),
            optionA: document.getElementById('question-option-a').value.trim(),
            optionB: document.getElementById('question-option-b').value.trim(),
            optionC: document.getElementById('question-option-c').value.trim(),
            optionD: document.getElementById('question-option-d').value.trim() || null,
            optionE: document.getElementById('question-option-e').value.trim() || null,
            correctOption: Number(document.getElementById('question-correct').value)
        };
    }

    function fillForm(q) {
        document.getElementById('question-id').value = q.id;
        document.getElementById('question-text').value = q.text;
        document.getElementById('question-option-a').value = q.optionA || '';
        document.getElementById('question-option-b').value = q.optionB || '';
        document.getElementById('question-option-c').value = q.optionC || '';
        document.getElementById('question-option-d').value = q.optionD || '';
        document.getElementById('question-option-e').value = q.optionE || '';
        document.getElementById('question-correct').value = String(q.correctOption);
    }

    function openCreate() {
        resetForm();
        document.getElementById('question-dialog').showModal();
    }

    function openEdit(id) {
        let q = items.find(function (x) { return x.id === id; });
        if (!q) return;
        fillForm(q);
        document.getElementById('question-dialog').showModal();
    }

    const form = document.getElementById('question-form');
    if (form) {
        form.addEventListener('submit', function (event) {
            event.preventDefault();
            let id = document.getElementById('question-id').value;
            let payload = readForm();
            let btn = form.querySelector('button[type="submit"]');
            if (btn) btn.disabled = true;
            let req = id
                ? Kela.axios.put('/teacher/questions/' + id, payload)
                : Kela.axios.post('/teacher/questions', payload);
            req.then(function () {
                document.getElementById('question-dialog').close();
                load();
                Kela.notify.success(id ? t('questions.updated') : t('questions.created'));
            }).catch(function () { }).finally(function () {
                if (btn) btn.disabled = false;
            });
        });
    }

    const searchInput = document.getElementById(SEARCH_ID);
    if (searchInput) {
        searchInput.addEventListener('input', renderList);
        searchInput.addEventListener('search', renderList);
    }

    Kela.onPageEvent('click', function (event) {
        let edit = event.target.closest('[data-edit-id]');
        if (edit) {
            event.preventDefault();
            openEdit(Number(edit.getAttribute('data-edit-id')));
            return;
        }

        let del = event.target.closest('[data-delete-id]');
        if (del) {
            event.preventDefault();
            let id = Number(del.getAttribute('data-delete-id'));
            let message = del.getAttribute('data-confirm') || t('questions.deleteConfirm', { text: '' });
            if (!confirm(message)) return;
            Kela.axios.delete('/teacher/questions/' + id).then(function () {
                load();
                Kela.notify.success(t('questions.deleted'));
            }).catch(function () { });
        }
    });

    load();
})();
