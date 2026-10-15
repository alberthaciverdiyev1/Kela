(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    let createMode = true;
    let step = 1;
    let options = null;
    let generatedStudentIds = [];

    function showError(m) { errBox.textContent = m; errBox.classList.remove('hidden'); }
    function clearError() { errBox.classList.add('hidden'); }
    function done() { AdminUI.notify(L["Js_Added"]); window.closeModal(); loadList(1); }

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

    window.wizGenerate = async function () {
        const res = await AdminUI.postJson('/students/generate', {
            count: Number(document.getElementById('wg_count').value),
            fullNamePrefix: document.getElementById('wg_prefix').value,
            emailDomain: document.getElementById('wg_domain').value,
            password: document.getElementById('wg_password').value
        });
        if (!res.ok) { AdminUI.error(AdminUI.errText(res)); return; }

        const list = res.data.students || [];
        list.forEach(s => generatedStudentIds.push(s.id));
        if (list.length) AdminUI.downloadCsv('students-' + Date.now() + '.csv', ['Ad', 'E-poçt', 'Şifrə'], list.map(s => [s.fullName, s.email, s.password]));
        const info = document.getElementById('wizGenInfo');
        info.textContent = generatedStudentIds.length + ' ' + L["Common_Generate"];
        info.classList.remove('hidden');
        document.getElementById('wizGenPanel').classList.add('hidden');
        AdminUI.notify(res.data.created + L["Js_StudentsCreated"]);
    };

    window.toggleAllCheck = function (containerId, checked) {
        document.querySelectorAll('#' + containerId + ' [data-name]').forEach(row => {
            if (row.classList.contains('hidden')) return;
            const cb = row.querySelector('input[type=checkbox]');
            if (cb) cb.checked = checked;
        });
    };

    window.showModal = function () {
        document.getElementById('modal').classList.remove('hidden');
        document.getElementById('modal').classList.add('flex');
    };
    window.closeModal = function () {
        document.getElementById('modal').classList.add('hidden');
        document.getElementById('modal').classList.remove('flex');
        clearError();
    };

    function setStep(n) {
        step = n;
        document.querySelectorAll('[data-step-panel]').forEach(el =>
            el.classList.toggle('hidden', Number(el.dataset.stepPanel) !== n));
        document.querySelectorAll('[data-step-dot]').forEach(el => {
            const on = Number(el.dataset.stepDot) <= n;
            el.classList.toggle('bg-primary', on);
            el.classList.toggle('text-white', on);
            el.classList.toggle('bg-slate-100', !on);
            el.classList.toggle('text-slate-500', !on);
        });
        document.getElementById('prevBtn').classList.toggle('hidden', n === 1);
        document.getElementById('nextBtn').classList.toggle('hidden', !createMode || n === 4);
        document.getElementById('saveBtn').classList.toggle('hidden', createMode && n !== 4);
        document.querySelector('[data-step-panel="' + n + '"]')?.querySelector('input,select,textarea')?.focus();
    }

    function renderCheckList(containerId, items) {
        const box = document.getElementById(containerId);
        if (!items.length) { box.innerHTML = '<div class="p-3 text-sm text-slate-400">' + L["Js_Empty"] + '</div>'; return; }
        box.innerHTML = items.map(o =>
            `<label class="flex cursor-pointer items-center gap-3 rounded-lg px-3 py-2 hover:bg-surface-secondary" data-name="${(o.text || '').toLowerCase()}">
                <input type="checkbox" value="${o.id}" class="h-4 w-4 rounded border-slate-300 text-primary focus:ring-primary" />
                <span class="text-sm text-slate-700">${o.text}</span>
            </label>`).join('');
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
        renderCheckList('quizzesList', options.quizzes || []);
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
        document.getElementById('user_id').value = '';
        document.getElementById('image').value = '';
        generatedStudentIds = [];
        document.getElementById('wizGenInfo')?.classList.add('hidden');
        AdminUI.fileRemove('image');
        clearError();
        clearInvalid();

        window.setWizardMode(true);
        window.showModal();

        try { await loadOptions(); } catch { /* options boş */ }
    };

    window.buildGroupForm = function () {
        const fd = new FormData();
        fd.append('name', document.getElementById('name').value);
        fd.append('description', document.getElementById('description').value || '');
        fd.append('user_id', document.getElementById('user_id').value || '');
        fd.append('RemoveImage', document.getElementById('image_remove').value === 'true' ? 'true' : 'false');
        const file = document.getElementById('image').files[0];
        if (file) fd.append('Image', file);
        return fd;
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        if (!validateRequired(1)) {
            setStep(1);
            return;
        }

        const res = await AdminUI.postForm('/group', window.buildGroupForm());
        if (!res.ok) { setStep(1); showError(AdminUI.errText(res)); return; }

        const id = res.data.id;
        const subjects = collect('subjectsList');
        const students = [...new Set([...collect('studentsList'), ...generatedStudentIds])];
        const quizzes = collect('quizzesList');

        if (subjects.length) await AdminUI.postJson(`/group/${id}/subjects`, { subjectIds: subjects });
        if (students.length) await AdminUI.postJson(`/group/${id}/students`, { studentIds: students });
        if (quizzes.length) await AdminUI.postJson(`/group/${id}/quizzes`, { quizIds: quizzes });

        done();
    });
})();
