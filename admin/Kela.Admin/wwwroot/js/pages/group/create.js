(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    let createMode = true;
    let step = 1;
    let genSpec = null;
    let options = null;

    function showError(m) { errBox.textContent = m; errBox.classList.remove('hidden'); }
    function clearError() { errBox.classList.add('hidden'); }
    function done() { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); }

    function clearInvalid() {
        document.querySelectorAll('[data-invalid]').forEach(el => {
            el.style.borderColor = '';
            el.style.boxShadow = '';
            el.removeAttribute('data-invalid');
        });
    }
    function markInvalid(el) {
        el.style.borderColor = '#dc2626';
        el.style.boxShadow = '0 0 0 3px rgba(220,38,38,.15)';
        el.setAttribute('data-invalid', '1');
    }
    function validateRequired(n) {
        clearInvalid();
        const panel = document.querySelector('[data-step-panel="' + n + '"]');
        const req = panel ? [...panel.querySelectorAll('input[required],select[required],textarea[required]')] : [];
        const empty = req.filter(el => !el.value || !String(el.value).trim());
        if (empty.length) {
            empty.forEach(markInvalid);
            showError(L["Common_RequiredFields"]);
            return false;
        }
        return true;
    }


    window.wizToggleGen = function () {
        document.getElementById('wizGenPanel').classList.toggle('hidden');
    };

    window.wizGenerate = function () {
        // Generasiya dərhal başlamır: parametrlər yadda saxlanılır və qrup
        // yaradılan POST ilə serverə verilir (orada arxa plan işi kimi işləyir).
        genSpec = {
            count: Number(document.getElementById('wg_count').value || 0),
            prefix: document.getElementById('wg_prefix').value || 'student',
            domain: document.getElementById('wg_domain').value || 'kela.az',
            password: document.getElementById('wg_password').value || ''
        };
        document.getElementById('wizGenPanel').classList.add('hidden');
        const info = document.getElementById('wizGenInfo');
        info.textContent = (L["Group_WillGenerate"] || '{0}').replace('{0}', genSpec.count);
        info.classList.remove('hidden');
    };

    window.toggleAllCheck = function (containerId, checked) {
        document.querySelectorAll('#' + containerId + ' [data-name]').forEach(row => {
            if (row.classList.contains('hidden')) return;
            const cb = row.querySelector('input[type=checkbox]');
            if (cb) cb.checked = checked;
        });
    };


    function setStep(n) {
        step = n;
        document.querySelectorAll('[data-step-panel]').forEach(el =>
            el.classList.toggle('hidden', Number(el.dataset.stepPanel) !== n));
        document.querySelectorAll('[data-step-item]').forEach(item => {
            const num = Number(item.dataset.stepItem);
            const dot = item.querySelector('[data-step-dot]');
            const label = item.querySelector('[data-step-label]');
            const done = num < n, active = num === n;
            if (dot) {
                dot.textContent = done ? '✓' : String(num);
                dot.className = 'grid h-8 w-8 shrink-0 place-items-center rounded-full text-xs font-bold transition-colors ' +
                    (active ? 'bg-primary text-white' : done ? 'bg-primary-light text-primary' : 'bg-slate-100 text-slate-500');
            }
            if (label) {
                label.className = 'block text-sm font-semibold transition-colors ' +
                    (active ? 'text-primary' : done ? 'text-slate-600' : 'text-slate-400');
            }
        });
        updateCounts();
        if (n === 4) renderQuizzes();
        document.getElementById('prevBtn').classList.toggle('hidden', n === 1);
        document.getElementById('nextBtn').classList.toggle('hidden', !createMode || n === 4);
        document.getElementById('saveBtn').classList.toggle('hidden', createMode && n !== 4);
        document.querySelector('[data-step-panel="' + n + '"]')?.querySelector('input,select,textarea')?.focus();
    }

    // Hər siyahıda neçə seçim olduğunu addım başlığında göstərir.
    function updateCounts() {
        [['subjects', 'count-subjects'], ['students', 'count-students'], ['quizzes', 'count-quizzes']].forEach(([id, elId]) => {
            const el = document.getElementById(elId);
            if (!el) return;
            const count = document.querySelectorAll('#' + id + 'List input[type=checkbox]:checked').length;
            el.textContent = count ? (L["Js_SelectedCount"] || '{0}').replace('{0}', count) : '';
        });
    }

    document.addEventListener('change', (e) => {
        if (e.target.matches && e.target.matches('#subjectsList input, #studentsList input, #quizzesList input')) updateCounts();
    });

    // İmtahanlar yalnız seçilmiş fənnlərin dərslərinə aid olanlar göstərilir.
    function renderQuizzes() {
        const box = document.getElementById('quizzesList');
        if (!box) return;

        const selected = new Set(collect('subjectsList').map(Number));
        if (!selected.size) {
            box.innerHTML = `<div class="p-3 text-sm text-slate-400">${L["Group_PickSubjectsFirst"]}</div>`;
            updateCounts();
            return;
        }

        const allowed = (options?.quizzes || []).filter(q =>
            q.subjectId != null && selected.has(Number(q.subjectId)));
        renderCheckList('quizzesList', allowed, true);
    }

    function renderCheckList(containerId, items, checked) {
        const box = document.getElementById(containerId);
        if (!items.length) { box.innerHTML = '<div class="p-3 text-sm text-slate-400">' + L["Js_Empty"] + '</div>'; return; }

        // Elementlərin "group" sahəsi varsa (məs. imtahanın dərsi) başlıqlar altında yığılır.
        let lastGroup = null;
        box.innerHTML = items.map(o => {
            let header = '';
            const group = o.group || '';
            if (group !== lastGroup) {
                lastGroup = group;
                header = `<div class="px-3 pt-3 pb-1 text-xs font-bold uppercase tracking-wide text-slate-400">${group || L["Common_General"]}</div>`;
            }
            return header + `
            <label class="flex cursor-pointer items-center gap-3 rounded-lg px-3 py-2 hover:bg-surface-secondary" data-name="${(o.text || '').toLowerCase()}">
                <input type="checkbox" value="${o.id}" ${checked ? 'checked' : ''} class="h-4 w-4 rounded border-slate-300 text-primary focus:ring-primary" />
                <span class="text-sm text-slate-700">${o.text}</span>
            </label>`;
        }).join('');
    }

    function collect(containerId) {
        return [...document.querySelectorAll('#' + containerId + ' input[type=checkbox]:checked')]
            .map(c => Number(c.value));
    }

    async function loadOptions() {
        if (options) return;
        options = await AdminUI.get('/group/options');
        renderCheckList('subjectsList', options.subjects || []);
        renderCheckList('studentsList', options.students || []);
        renderQuizzes();
    }

    window.filterCheckList = function (containerId, q) {
        const term = (q || '').trim().toLowerCase();
        document.querySelectorAll('#' + containerId + ' [data-name]').forEach(el =>
            el.classList.toggle('hidden', term.length > 0 && !el.dataset.name.includes(term)));
    };

    window.wizardNext = function () { if (!validateRequired(step)) return; if (step < 4) setStep(step + 1); };
    window.wizardPrev = function () { if (step > 1) setStep(step - 1); };

    window.setWizardMode = function (isCreate) {
        createMode = isCreate;
        document.getElementById('wizardSteps').classList.toggle('hidden', !isCreate);
        setStep(1);
    };

    window.openCreate = async function () {
        document.getElementById('modalTitle').textContent = L["Group_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('name').value = '';
        document.getElementById('description').value = '';
        // Müəllim seçimi yalnız admin üçün göstərilir.
        const teacher = document.getElementById('user_id');
        if (teacher) teacher.value = '';
        document.getElementById('image').value = '';
        genSpec = null;
        document.getElementById('wizGenInfo')?.classList.add('hidden');
        AdminUI.fileRemove('image');
        clearError();
        clearInvalid();

        window.setWizardMode(true);
        AdminUI.openModal('modal');

        try { await loadOptions(); } catch { /* options boş */ }
    };

    window.buildGroupForm = function () {
        const fd = new FormData();
        fd.append('name', document.getElementById('name').value);
        fd.append('description', document.getElementById('description').value || '');
        const teacherField = document.getElementById('user_id');
        if (teacherField) fd.append('user_id', teacherField.value || '');
        fd.append('RemoveImage', document.getElementById('image_remove').value === 'true' ? 'true' : 'false');
        const file = document.getElementById('image').files[0];
        if (file) fd.append('Image', file);
        // Generasiya parametrləri (əgər istifadəçi generasiya seçibsə) — server arxa planda işlədir.
        if (genSpec && genSpec.count > 0) {
            fd.append('generate_count', String(genSpec.count));
            fd.append('generate_prefix', genSpec.prefix);
            fd.append('generate_domain', genSpec.domain);
            if (genSpec.password) fd.append('generate_password', genSpec.password);
        }
        return fd;
    };

    // Sürətli təkrar klik bir neçə qrup yaradırdı — göndərmə kilidlənir.
    let submitting = false;

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js
        if (submitting) return;

        if (!validateRequired(1)) {
            setStep(1);
            return;
        }

        submitting = true;
        const saveBtn = document.getElementById('saveBtn');
        if (saveBtn) saveBtn.disabled = true;

        try {
            const res = await AdminUI.postForm('/group', window.buildGroupForm());
            if (!res.ok) { setStep(1); showError(AdminUI.errText(res)); return; }

            const id = res.data.id;
            const subjects = collect('subjectsList');
            const students = collect('studentsList');
            const quizzes = collect('quizzesList');

            if (subjects.length) await AdminUI.postJson(`/group/${id}/subjects`, { subjectIds: subjects });
            if (students.length) await AdminUI.postJson(`/group/${id}/students`, { studentIds: students });
            if (quizzes.length) await AdminUI.postJson(`/group/${id}/quizzes`, { quizIds: quizzes });

            done();
        } finally {
            submitting = false;
            if (saveBtn) saveBtn.disabled = false;
        }
    });
})();
