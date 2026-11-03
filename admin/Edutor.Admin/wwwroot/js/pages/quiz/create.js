(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    const lessonSelect = document.getElementById('lesson_id');

    // Fənn filtri üçün ilkin siyahı; variant mətni artıq "Fənn — Dərs" formatındadır.
    const allLessons = [...lessonSelect.options].map(o => ({
        id: o.value,
        text: o.text,
        subjectId: (window.LESSON_SUBJECTS || {})[o.value] ?? ''
    }));

    // Fənn seçiminə uyğun dərs siyahısını yenidən qurur. selectedLessonId verilməsə
    // seçim sıfırlanır (filtr dəyişəndə köhnə dərs qalmasın).
    window.filterQuizLessons = function (selectedLessonId) {
        const subjectId = document.getElementById('subject_filter').value;
        const keep = selectedLessonId ?? '';
        const items = subjectId ? allLessons.filter(l => l.subjectId === subjectId) : allLessons;

        lessonSelect.innerHTML =
            `<option value="" disabled${keep ? '' : ' selected'}>${AdminUI.escapeHtml(L["Common_ChooseOne"] || '—')}</option>` +
            items.map(l => `<option value="${l.id}"${l.id === keep ? ' selected' : ''}>${AdminUI.escapeHtml(l.text)}</option>`).join('');

        if (keep && items.some(l => l.id === keep)) lessonSelect.value = keep;
        AdminUI.syncSelect(lessonSelect);
    };

    document.getElementById('subject_filter').addEventListener('change', () => window.filterQuizLessons(''));

    // Yaratma modalında dərs seçiləndə həmin fənnin sual və qrupları təklif olunur.
    const extrasBox = document.getElementById('createExtras');
    const questionListBox = document.getElementById('createQuestionList');
    const groupListBox = document.getElementById('createGroupList');

    // Dərs sürətlə dəyişərsə köhnə cavab yeni siyahının üstünə yazılmasın.
    let extrasToken = 0;

    function renderChecks(box, items, labelOf) {
        box.innerHTML = items.length
            ? items.map(o => `<label class="flex cursor-pointer items-center gap-2 rounded px-1 py-0.5 hover:bg-slate-50">
                    <input type="checkbox" value="${o.id}" class="h-4 w-4 rounded border-slate-300 text-primary focus:ring-primary" />
                    <span class="truncate text-sm text-slate-600">${AdminUI.escapeHtml(labelOf(o))}</span>
                </label>`).join('')
            : `<div class="text-sm text-slate-400">${AdminUI.escapeHtml(L["Js_Empty"])}</div>`;
    }

    function checkedIds(box) {
        return [...box.querySelectorAll('input[type=checkbox]:checked')].map(i => Number(i.value));
    }

    async function loadCreateExtras() {
        const lessonId = lessonSelect.value;
        const token = ++extrasToken;

        if (!lessonId) {
            extrasBox.classList.add('hidden');
            questionListBox.innerHTML = '';
            groupListBox.innerHTML = '';
            return;
        }

        extrasBox.classList.remove('hidden');
        const subjectId = (window.LESSON_SUBJECTS || {})[lessonId] ?? '';

        const [questions, groups] = await Promise.all([
            AdminUI.get('/quiz/options?lesson_id=' + encodeURIComponent(lessonId)),
            AdminUI.get('/quiz/group-options?subject_id=' + encodeURIComponent(subjectId))
        ]);
        if (token !== extrasToken) return;

        renderChecks(questionListBox, questions, q => q.text);
        renderChecks(groupListBox, groups, g => g.name);
    }

    lessonSelect.addEventListener('change', () => {
        if (!document.getElementById('entityId').value) loadCreateExtras();
    });

    window.buildQuizForm = function () {
        const fd = new FormData();
        fd.append('lesson_id', document.getElementById('lesson_id').value);
        fd.append('title', document.getElementById('title').value);
        fd.append('duration_minutes', document.getElementById('duration_minutes').value || '1');
        const sortOrder = document.getElementById('sort_order').value;
        if (sortOrder !== '') fd.append('sort_order', sortOrder);
        fd.append('type', document.getElementById('type').value);
        fd.append('is_active', document.getElementById('is_active').checked ? 'true' : 'false');
        fd.append('is_repeat', document.getElementById('is_repeat').checked ? 'true' : 'false');
        return fd;
    };

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Quiz_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('sort_order').value = '';
        document.getElementById('subject_filter').value = '';
        window.filterQuizLessons('');
        document.getElementById('title').value = '';
        document.getElementById('duration_minutes').value = 1;
        document.getElementById('type').value = 'lesson';
        document.getElementById('is_active').checked = true;
        document.getElementById('is_repeat').checked = false;
        extrasToken++;
        extrasBox.classList.add('hidden');
        questionListBox.innerHTML = '';
        groupListBox.innerHTML = '';
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const res = await AdminUI.postForm('/quiz', window.buildQuizForm());
        if (res.ok) {
            const quizId = res.data?.id;
            const questionIds = checkedIds(questionListBox);
            const groupIds = checkedIds(groupListBox);

            // İmtahan yaradıldıqdan sonra seçilmiş sual/qruplar bağlanır; uğursuzluq
            // imtahanın özünü ləğv etmir — istifadəçi onları sonra da əlavə edə bilər.
            if (quizId && questionIds.length) {
                await AdminUI.postJson(`/quiz/${quizId}/questions`, { questionIds });
            }
            if (quizId && groupIds.length) {
                const link = await AdminUI.postJson(`/quiz/${quizId}/groups`, { groupIds });
                if (!link.ok) AdminUI.error(AdminUI.errText(link));
            }

            AdminUI.notify(L["Js_Added"]);
            AdminUI.closeModal('modal');
            loadList(1);
            return;
        }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
