// Qrup idarəetməsi səhifəsi: üzv/fənn/imtahan əlavə etmə, loqlar, qrupun redaktəsi.
(function () {
    const groupId = Number(document.querySelector('[data-group-id]')?.dataset.groupId
        || location.pathname.match(/\/group\/(\d+)\/manage/)?.[1] || 0);

    let options = null;

    const fillSelect = (id, items) => {
        const el = document.getElementById(id);
        if (!el) return;
        el.innerHTML = items.map(o => `<option value="${o.id}">${o.text}</option>`).join('');
    };

    function downloadGenerated(list) {
        if (!list || !list.length) return;
        AdminUI.downloadCsv('students-' + Date.now() + '.csv',
            ['Ad', 'E-poçt', 'Şifrə'],
            list.map(s => [s.fullName, s.email, s.password]));
    }

    function renderMembers(containerId, items, type, labelFn, idFn) {
        const box = document.getElementById(containerId);
        if (!box) return;
        box.innerHTML = '';
        if (!items.length) {
            box.innerHTML = '<div class="text-sm text-slate-400">' + L["Js_Empty"] + '</div>';
            return;
        }
        items.forEach(item => {
            const row = document.createElement('div');
            row.className = 'flex items-center justify-between rounded-xl bg-surface-secondary px-4 py-2.5';
            row.innerHTML = `<span class="text-sm text-slate-700">${labelFn(item)}</span>
                <button type="button" class="admin-row-action is-danger" onclick="removeMember('${type}', ${idFn(item)})">${L["Common_Delete"]}</button>`;
            box.appendChild(row);
        });
    }

    async function refresh() {
        renderMembers('studentsList', await AdminUI.get(`/group/${groupId}/students`), 'students', m => `${m.fullName} · ${m.email}`, m => m.studentId);
        renderMembers('subjectsList', await AdminUI.get(`/group/${groupId}/subjects`), 'subjects', s => s.name, s => s.id);
        renderMembers('quizzesList', await AdminUI.get(`/group/${groupId}/quizzes`), 'quizzes', q => q.title, q => q.id);
    }

    async function loadOptions() {
        if (options) return;
        options = await AdminUI.get('/group/options');
        fillSelect('studentPick', options.students || []);
        fillSelect('subjectPick', options.subjects || []);
        fillSelect('quizPick', options.quizzes || []);
    }

    async function loadLogs() {
        const items = await AdminUI.get(`/group/${groupId}/subject-logs`);
        const box = document.getElementById('logsList');
        const subjName = id => {
            const s = (options?.subjects || []).find(x => x.id === id);
            return s ? s.text : ('#' + id);
        };
        box.innerHTML = items.length
            ? items.map(l => {
                const attach = l.action === 'attach';
                return `<div class="flex items-center justify-between rounded-xl bg-surface-secondary px-4 py-2.5">
                    <span class="text-sm text-slate-700"><span class="font-semibold">${subjName(l.subjectId)}</span> — <span class="font-semibold ${attach ? 'text-success' : 'text-danger'}">${attach ? L["Js_Added"] : L["Js_Removed"]}</span></span>
                    <span class="text-xs text-slate-400">${new Date(l.createdAt).toLocaleString('az')}</span>
                </div>`;
            }).join('')
            : '<div class="text-sm text-slate-400">' + L["Js_Empty"] + '</div>';
    }

    window.copyGroupLink = () => {
        const token = document.getElementById('groupJoinToken')?.dataset.token;
        if (!token) return;
        const base = (window.GROUP_APP_BASE_URL || location.origin).replace(/\/$/, '');
        AdminUI.copyText(base + '/join/' + token).then(ok => { if (ok) AdminUI.notify(L["Js_LinkCopied"]); });
    };

    function esc(v) {
        return String(v ?? '').replace(/[&<>"']/g, c => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
        }[c]));
    }

    async function loadJoins() {
        const box = document.getElementById('joinsList');
        if (!box) return;

        const items = await AdminUI.get(`/group/${groupId}/join-requests`);
        const badge = (s) => s === 'pending'
            ? AdminUI.badge(L["Group_JoinPending"], 'warning')
            : s === 'approved'
                ? AdminUI.badge(L["Group_JoinApproved"], 'success')
                : AdminUI.badge(L["Group_JoinRejected"], 'danger');

        box.innerHTML = items.length
            ? items.map(j => `<div class="flex items-center justify-between gap-3 rounded-xl bg-surface-secondary px-4 py-2.5">
                <div class="min-w-0">
                    <p class="truncate text-sm font-semibold text-slate-700">${esc(j.fullName)}</p>
                    <p class="truncate text-xs text-slate-400">${esc(j.email)}</p>
                </div>
                <div class="flex shrink-0 items-center gap-2">
                    ${badge(j.status)}
                    ${j.status === 'pending'
                        ? `<button type="button" class="admin-row-action" onclick="decideJoin(${j.id}, true)">${L["Group_JoinApprove"]}</button>
                           <button type="button" class="admin-row-action is-danger" onclick="decideJoin(${j.id}, false)">${L["Group_JoinReject"]}</button>`
                        : ''}
                </div>
            </div>`).join('')
            : '<div class="text-sm text-slate-400">' + L["Js_Empty"] + '</div>';
    }

    window.decideJoin = async (requestId, approve) => {
        const action = approve ? 'approve' : 'reject';
        const res = await AdminUI.postJson(`/group/${groupId}/join-requests/${requestId}/${action}`, {});
        if (res.ok) {
            AdminUI.notify(approve ? L["Group_JoinApproved"] : L["Group_JoinRejected"]);
            loadJoins();
            if (approve) refresh();
        } else {
            AdminUI.error(AdminUI.errText(res));
        }
    };

    document.addEventListener('tab:change', (e) => {
        if (e.detail.tab === 'logs') loadLogs();
        if (e.detail.tab === 'joins') loadJoins();
    });

    window.toggleGroupEdit = () => {
        document.getElementById('groupEdit').classList.toggle('hidden');
    };

    window.toggleGroupGen = () => {
        document.getElementById('groupGenPanel').classList.toggle('hidden');
    };

    window.generateIntoGroup = async () => {
        const res = await AdminUI.postJson('/students/generate', {
            groupId,
            count: Number(document.getElementById('gg_count').value),
            fullNamePrefix: document.getElementById('gg_prefix').value,
            emailDomain: document.getElementById('gg_domain').value,
            password: document.getElementById('gg_password').value
        });
        if (res.ok) {
            AdminUI.notify(res.data.created + L["Js_StudentsCreated"]);
            downloadGenerated(res.data.students);
            document.getElementById('groupGenPanel').classList.add('hidden');
            refresh();
        } else {
            AdminUI.error(AdminUI.errText(res));
        }
    };

    window.assignMember = async (type) => {
        const pickId = type === 'students' ? 'studentPick' : type === 'subjects' ? 'subjectPick' : 'quizPick';
        const value = document.getElementById(pickId).value;
        if (!value) return;

        const key = type === 'students' ? 'studentIds' : type === 'subjects' ? 'subjectIds' : 'quizIds';
        const res = await AdminUI.postJson(`/group/${groupId}/${type}`, { [key]: [Number(value)] });
        if (res.ok) { AdminUI.notify(L["Js_Added"]); refresh(); }
        else AdminUI.error(AdminUI.errText(res));
    };

    window.removeMember = async (type, id) => {
        if (!await AdminUI.confirmDelete(L["Js_ConfirmRemove"])) return;
        const res = await AdminUI.del(`/group/${groupId}/${type}/${id}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); refresh(); }
        else AdminUI.error(AdminUI.errText(res));
    };

    window.deleteGroup = async () => {
        if (!await AdminUI.confirmDelete(L["Js_ConfirmDeleteGroup"])) return;
        const res = await AdminUI.del(`/group/${groupId}`);
        if (res.ok) { location.href = '/group'; }
        else AdminUI.error(AdminUI.errText(res));
    };

    document.addEventListener('DOMContentLoaded', async () => {
        await loadOptions();
        await refresh();
    });

    // Qrupun adı/təsviri/müəllimi/şəkli — modalda olmayan redaktə.
    document.getElementById('groupEditForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const fd = new FormData();
        fd.append('name', document.getElementById('g_name').value);
        fd.append('description', document.getElementById('g_description').value || '');
        const teacher = document.getElementById('g_teacher');
        if (teacher) fd.append('user_id', teacher.value || '');
        fd.append('RemoveImage', 'false');
        const file = document.getElementById('g_image').files[0];
        if (file) fd.append('Image', file);

        const res = await AdminUI.putForm(`/group/${groupId}`, fd);
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); location.reload(); }
        else AdminUI.error(AdminUI.errText(res));
    });
})();
