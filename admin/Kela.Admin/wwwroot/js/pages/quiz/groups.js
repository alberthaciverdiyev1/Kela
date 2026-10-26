let qzGroupsQuizId = null;
let groupOptionsCache = null;

function fillGroupsSelect(items) {
    const select = document.getElementById('groupsSelect');
    select.innerHTML = items.length
        ? items.map(g => `<option value="${g.id}">${AdminUI.escapeHtml(g.text)}</option>`).join('')
        : `<option value="">${AdminUI.escapeHtml(L["Js_Empty"])}</option>`;

    // Select enhanced-dir — native siyahı dəyişəndə görünən etiketi də yenilə.
    AdminUI.syncSelect(select);
}

async function openGroups(id) {
    qzGroupsQuizId = id;

    const q = await AdminUI.get('/quiz/' + id);
    document.getElementById('groupsTitle').textContent = (q.title || '') + ' — ' + L["Nav_Groups"];

    groupOptionsCache = await loadGroupOptions(id);
    await refreshGroups();
    AdminUI.openModal('groupsModal');
}

async function loadGroupOptions(id) {
    // İmtahanın fənni olmayan qruplar serverdə artıq filtrlənir.
    const items = await AdminUI.get(`/quiz/${id}/group-options`);
    return items.map(g => ({ id: g.id, text: g.name }));
}

function refreshGroups() {
    return Promise.all([
        AdminUI.get(`/quiz/${qzGroupsQuizId}/groups`),
        groupOptionsCache ? Promise.resolve(groupOptionsCache) : loadGroupOptions(qzGroupsQuizId)
            .then(options => (groupOptionsCache = options))
    ]).then(([assigned, options]) => {
        const box = document.getElementById('groupsList');
        box.innerHTML = assigned.length
            ? assigned.map(g => `<div class="flex items-center justify-between gap-3 rounded-lg bg-slate-50 px-3 py-2">
                    <span class="truncate text-sm text-slate-700">${AdminUI.escapeHtml(g.name)}</span>
                    <button type="button" class="shrink-0 text-sm text-red-600 hover:text-red-800"${AdminUI.actionAttrs('removeQuizGroup', [g.id])}>${L["Common_Delete"]}</button>
                </div>`).join('')
            : `<div class="text-sm text-slate-400">${L["Js_Empty"]}</div>`;

        // Artıq bağlanmış qruplar seçimdə təkrar görünməsin.
        const assignedIds = new Set(assigned.map(g => g.id));
        fillGroupsSelect(options.filter(g => !assignedIds.has(g.id)));
    });
}

window.assignGroup = async () => {
    const value = document.getElementById('groupsSelect').value;
    if (!value) return;

    const res = await AdminUI.postJson(`/quiz/${qzGroupsQuizId}/groups`, { groupIds: [Number(value)] });
    if (res.ok) { AdminUI.notify(L["Js_Added"]); await refreshGroups(); loadList(pager.page); }
    else AdminUI.error(AdminUI.errText(res));
};

window.removeQuizGroup = async (groupId) => {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmRemove"])) return;

    const res = await AdminUI.del(`/quiz/${qzGroupsQuizId}/groups/${groupId}`);
    if (res.ok) { AdminUI.notify(L["Js_Deleted"]); await refreshGroups(); loadList(pager.page); }
    else AdminUI.error(AdminUI.errText(res));
};
