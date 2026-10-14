let mgGroupId = null;
let mgOptions = null;

function fillSelect(id, items) {
    document.getElementById(id).innerHTML =
        items.map(o => `<option value="${o.id}">${o.text}</option>`).join('');
}

function switchTab(tab) {
    document.querySelectorAll('.manage-tab').forEach(el => el.classList.add('hidden'));
    document.getElementById('tab-' + tab).classList.remove('hidden');
    document.querySelectorAll('.manage-tabbtn').forEach(el => {
        const on = el.dataset.tab === tab;
        el.classList.toggle('bg-primary', on);
        el.classList.toggle('text-white', on);
        el.classList.toggle('text-slate-600', !on);
    });
}

async function openManage(id) {
    mgGroupId = id;
    const g = await AdminUI.get('/group/' + id);
    document.getElementById('manageTitle').textContent = g.name + ' — idarə';

    if (!mgOptions) mgOptions = await AdminUI.get('/group/options');
    fillSelect('studentsSelect', mgOptions.students);
    fillSelect('subjectsSelect', mgOptions.subjects);
    fillSelect('quizzesSelect', mgOptions.quizzes);

    await refreshManage();
    document.getElementById('manageModal').classList.remove('hidden');
    document.getElementById('manageModal').classList.add('flex');
    switchTab('students');
}

function closeManage() {
    document.getElementById('manageModal').classList.add('hidden');
    document.getElementById('manageModal').classList.remove('flex');
}

async function refreshManage() {
    renderMembers('studentsList', await AdminUI.get(`/group/${mgGroupId}/students`), 'students', m => m.fullName, m => m.studentId);
    renderMembers('subjectsList', await AdminUI.get(`/group/${mgGroupId}/subjects`), 'subjects', s => s.name, s => s.id);
    renderMembers('quizzesList', await AdminUI.get(`/group/${mgGroupId}/quizzes`), 'quizzes', q => q.title, q => q.id);
}

function renderMembers(containerId, items, type, labelFn, idFn) {
    const box = document.getElementById(containerId);
    box.innerHTML = '';
    if (!items.length) { box.innerHTML = '<div class="text-sm text-slate-400">Boşdur</div>'; return; }

    items.forEach(item => {
        const row = document.createElement('div');
        row.className = 'flex items-center justify-between rounded-lg bg-slate-50 px-3 py-2';
        row.innerHTML = `<span class="text-sm text-slate-700">${labelFn(item)}</span>
            <button type="button" class="text-sm text-red-600 hover:text-red-800" onclick="removeMember('${type}', ${idFn(item)})">Sil</button>`;
        box.appendChild(row);
    });
}

async function assignMember(type) {
    const value = document.getElementById(type + 'Select').value;
    if (!value) return;

    const key = type === 'students' ? 'studentIds' : type === 'subjects' ? 'subjectIds' : 'quizIds';
    const payload = {};
    payload[key] = [Number(value)];

    const res = await AdminUI.postJson(`/group/${mgGroupId}/${type}`, payload);
    if (res.ok) { AdminUI.notify('Əlavə olundu'); refreshManage(); }
    else AdminUI.error();
}

async function removeMember(type, id) {
    if (!await AdminUI.confirmDelete('Çıxarılsın?')) return;
    const res = await AdminUI.del(`/group/${mgGroupId}/${type}/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); refreshManage(); }
    else AdminUI.error();
}
