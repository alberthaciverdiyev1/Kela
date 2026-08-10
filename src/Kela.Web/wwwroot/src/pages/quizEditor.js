(function () {
    'use strict';

    const page = document.getElementById('quiz-editor');
    if (!page) return;

    const contentId = Number(page.dataset.contentId);
    const LIST_ID = 'quiz-questions-list';

    let quiz = null;
    let bank = [];

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

    function renderList() {
        let el = document.getElementById(LIST_ID);
        if (!el) return;
        let questions = quiz ? quiz.questions : [];
        if (!questions.length) {
            el.innerHTML = '<div class="card border border-base-200 bg-base-100 flex items-center justify-center gap-2 py-10 text-center">' +
                '<span class="text-base-300">' + Kela.icon('quiz', 'w-10 h-10') + '</span>' +
                '<p class="text-sm text-base-content/50">' + esc(t('quiz.noQuestions')) + '</p></div>';
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

        questions.forEach(function (entry, idx) {
            let q = entry.question;
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
                '<button type="button" class="btn btn-sm btn-square btn-ghost btn-error" title="' + esc(t('quiz.remove')) + '" data-remove-id="' + q.id + '" data-confirm="' + esc(t('quiz.removeConfirm', { text: q.text })) + '">' + Kela.icon('trash', 'w-4 h-4') + '</button>' +
                '</td>';
            html += '</tr>';
        });

        html += '</tbody></table></div></div>';
        el.innerHTML = html;
    }

    function load() {
        Kela.axios.get('/teacher/quizzes/' + contentId + '/data').then(function (res) {
            quiz = res.data || null;
            renderList();
        }).catch(function () { });
    }

    function existingIds() {
        return quiz ? quiz.questions.map(function (x) { return x.question.id; }) : [];
    }

    const addDialog = document.getElementById('quiz-add-question-dialog');
    const addList = document.getElementById('quiz-add-list');
    const addSearch = document.getElementById('quiz-add-search');
    const addConfirm = document.getElementById('quiz-add-confirm');

    function renderAddList() {
        if (!addList) return;
        let q = addSearch ? addSearch.value.trim().toLowerCase() : '';
        let existing = existingIds();
        let rows = bank.filter(function (b) {
            if (existing.indexOf(b.id) !== -1) return false;
            return !q || b.text.toLowerCase().indexOf(q) !== -1;
        });
        if (!rows.length) {
            addList.innerHTML = '<li class="p-3 text-sm text-base-content/50">' + esc(t('quiz.noMoreQuestions')) + '</li>';
            return;
        }
        addList.innerHTML = rows.map(function (b) {
            return '<li><label class="flex items-center gap-3 rounded-lg px-2 py-2">' +
                '<input type="checkbox" name="quiz-add-item" value="' + b.id + '" class="checkbox checkbox-sm">' +
                '<span class="flex-1 truncate text-sm">' + esc(b.text) + '</span>' +
                '</label></li>';
        }).join('');
    }

    const addBtn = document.querySelector('[onclick*="quiz-add-question-dialog"]');
    if (addBtn) {
        addBtn.addEventListener('click', function () {
            Kela.axios.get('/teacher/questions/list').then(function (res) {
                bank = res.data || [];
                if (addSearch) addSearch.value = '';
                renderAddList();
            }).catch(function () { });
        });
    }

    if (addSearch) {
        addSearch.addEventListener('input', renderAddList);
        addSearch.addEventListener('search', renderAddList);
    }

    if (addConfirm) {
        addConfirm.addEventListener('click', function () {
            let checked = Array.prototype.slice.call(document.querySelectorAll('input[name="quiz-add-item"]:checked'))
                .map(function (c) { return Number(c.value); });
            if (!checked.length) return;
            addConfirm.disabled = true;
            Kela.axios.post('/teacher/quizzes/' + contentId + '/questions', { questionIds: checked }).then(function () {
                addDialog.close();
                load();
                Kela.notify.success(t('quiz.questionAdded'));
            }).catch(function () { }).finally(function () {
                addConfirm.disabled = false;
            });
        });
    }

    const newForm = document.getElementById('quiz-new-question-form');
    if (newForm) {
        newForm.addEventListener('submit', function (event) {
            event.preventDefault();
            let payload = {
                text: document.getElementById('quiz-new-text').value.trim(),
                optionA: document.getElementById('quiz-new-option-a').value.trim(),
                optionB: document.getElementById('quiz-new-option-b').value.trim(),
                optionC: document.getElementById('quiz-new-option-c').value.trim(),
                optionD: document.getElementById('quiz-new-option-d').value.trim() || null,
                optionE: document.getElementById('quiz-new-option-e').value.trim() || null,
                correctOption: Number(document.getElementById('quiz-new-correct').value)
            };
            let btn = newForm.querySelector('button[type="submit"]');
            if (btn) btn.disabled = true;
            Kela.axios.post('/teacher/questions', payload).then(function (res) {
                let newId = res.data && res.data.id;
                document.getElementById('quiz-new-question-dialog').close();
                if (newId) {
                    return Kela.axios.post('/teacher/quizzes/' + contentId + '/questions', { questionIds: [newId] });
                }
            }).then(function () {
                load();
                Kela.notify.success(t('quiz.created'));
            }).catch(function () { }).finally(function () {
                if (btn) btn.disabled = false;
            });
        });
    }

    Kela.onPageEvent('click', function (event) {
        let remove = event.target.closest('[data-remove-id]');
        if (!remove) return;
        event.preventDefault();
        let qid = Number(remove.getAttribute('data-remove-id'));
        let message = remove.getAttribute('data-confirm') || t('quiz.removeConfirm', { text: '' });
        if (!confirm(message)) return;
        Kela.axios.delete('/teacher/quizzes/' + contentId + '/questions/' + qid).then(function () {
            load();
            Kela.notify.success(t('quiz.questionRemoved'));
        }).catch(function () { });
    });

    load();
})();
