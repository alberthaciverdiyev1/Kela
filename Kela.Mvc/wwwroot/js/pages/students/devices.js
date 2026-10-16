let devStudentId = null;

async function openDevices(id) {
    devStudentId = id;
    const s = await AdminUI.get('/students/' + id);
    document.getElementById('devicesTitle').textContent = s.fullName + ' — cihazlar';
    await refreshDevices();
    AdminUI.openModal('devicesModal');
}

function closeDevices() {
    AdminUI.closeModal('devicesModal');
}

async function refreshDevices() {
    const items = await AdminUI.get(`/students/${devStudentId}/devices`);
    const box = document.getElementById('devicesList');
    box.innerHTML = items.length
        ? items.map(d => `<div class="rounded-lg bg-slate-50 px-3 py-2 text-sm text-slate-700">${d.deviceId}</div>`).join('')
        : '<div class="text-sm text-slate-400">Cihaz yoxdur</div>';
}

async function unbindDevice() {
    if (!await AdminUI.confirmDelete(L["Js_ConfirmResetDevice"])) return;
    const res = await AdminUI.del(`/students/${devStudentId}/devices`);
    if (res.ok) { AdminUI.notify(L["Js_Unlocked"]); refreshDevices(); }
    else AdminUI.error();
}
