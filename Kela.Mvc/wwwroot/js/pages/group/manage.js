let mgGroupId = null;

function downloadGenerated(list) {
    if (!list || !list.length) return;
    AdminUI.downloadCsv('students-' + Date.now() + '.csv',
        ['Ad', 'E-poçt', 'Şifrə'],
        list.map(s => [s.fullName, s.email, s.password]));
}
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
    if (tab === 'logs') loadLogs();
}

async function loadLogs() {
    const items = await AdminUI.get(`/group/${mgGroupId}/subject-logs`);
    const box = document.getElementById('logsList');
    const subjName = id => {
        const s = (mgOptions && mgOptions.subjects || []).find(x => x.id === id);
        return s ? s.text : ('#' + id);
    };
    box.innerHTML = items.length
        ? items.map(l => {
            const attach = l.action === 'attach';
            return `<div class="flex items-center justify-between rounded-lg bg-slate-50 px-3 py-2">
                <span class="text-sm text-slate-700"><span class="font-semibold">${subjName(l.subjectId)}</span> — <span class="${attach ? 'text-emerald-600' : 'text-red-600'}">${attach ? L["Js_Added"] : L["Js_Removed"]}</span></span>
                <span class="text-xs text-slate-400">${new Date(l.createdAt).toLocaleString('az')}</span>
            </div>`;
        }).join('')
        : '<div class="text-sm text-slate-400">Qeyd yoxdur</div>';
}

async function openManage(id) {
    mgGroupId = id;
    const g = await AdminUI.get('/group/' + id);
    document.getElementById('manageTitle').textContent = g.name + L["Js_ManageSuffix"];

    if (!mgOptions) mgOptions = await AdminUI.get('/group/options');
    fillSelect('studentsSelect', mgOptions.students);
    fillSelect('subjectsSelect', mgOptions.subjects);
    fillSelect('quizzesSelect', mgOptions.quizzes);

    await refreshManage();
    AdminUI.openModal('manageModal');
    switchTab('students');
}

function closeManage() {
    AdminUI.closeModal('manageModal');
}

async function refreshManage() {
    renderMembers('studentsList', await AdminUI.get(`/group/${mgGroupId}/students`), 'students', m => m.fullName, m => m.studentId);
    renderMembers('subjectsList', await AdminUI.get(`/group/${mgGroupId}/subjects`), 'subjects', s => s.name, s => s.id);
    renderMembers('quizzesList', await AdminUI.get(`/group/${mgGroupId}/quizzes`), 'quizzes', q => q.title, q => q.id);
}

function renderMembers(containerId, items, type, labelFn, idFn) {
    const box = document.getElementById(containerId);
    box.innerHTML = '';
    if (!items.length) { box.innerHTML = '<div class="text-sm text-slate-400">' + L["Js_Empty"] + '</div>'; return; }

    items.forEach(item => {
        const row = document.createElement('div');
        row.className = 'flex items-center justify-between rounded-lg bg-slate-50 px-3 py-2';
        row.innerHTML = `<span class="text-sm text-slate-700">${labelFn(item)}</span>
            <button type="button" class="text-sm text-red-600 hover:text-red-800" onclick="removeMember('${type}', ${idFn(item)})">Sil</button>`;
        box.appendChild(row);
    });
}


window.toggleGroupGen = function () {
    document.getElementById('groupGenPanel').classList.toggle('hidden');
};

window.generateIntoGroup = async function () {
    const res = await AdminUI.postJson('/students/generate', {
        groupId: mgGroupId,
        count: Number(document.getElementById('gg_count').value),
        fullNamePrefix: document.getElementById('gg_prefix').value,
        emailDomain: document.getElementById('gg_domain').value,
        password: document.getElementById('gg_password').value
    });
    if (res.ok) {
        AdminUI.notify(res.data.created + L["Js_StudentsCreated"]);
        downloadGenerated(res.data.students);
        document.getElementById('groupGenPanel').classList.add('hidden');
        refreshManage();
    } else {
        AdminUI.error(AdminUI.errText(res));
    }
};

async function assignMember(type) {
    const value = document.getElementById(type + 'Select').value;
    if (!value) return;

    const key = type === 'students' ? 'studentIds' : type === 'subjects' ? 'subjectIds' : 'quizIds';
    const payload = {};
    payload[key] = [Number(value)];

    const res = await AdminUI.postJson(`/group/${mgGroupId}/${type}`, payload);
    if (res.ok) { AdminUI.notify(L["Js_Added"]); refreshManage(); }
    else AdminUI.error();
}

async function removeMember(type, id) {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmRemove"])) return;
    const res = await AdminUI.del(`/group/${mgGroupId}/${type}/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); refreshManage(); }
    else AdminUI.error();
}
