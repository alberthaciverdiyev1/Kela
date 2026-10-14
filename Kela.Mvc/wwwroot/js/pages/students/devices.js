let devStudentId = null;

async function openDevices(id) {
    devStudentId = id;
    const s = await AdminUI.get('/students/' + id);
    document.getElementById('devicesTitle').textContent = s.fullName + ' — cihazlar';
    await refreshDevices();
    document.getElementById('devicesModal').classList.remove('hidden');
    document.getElementById('devicesModal').classList.add('flex');
}

function closeDevices() {
    document.getElementById('devicesModal').classList.add('hidden');
    document.getElementById('devicesModal').classList.remove('flex');
}

async function refreshDevices() {
    const items = await AdminUI.get(`/students/${devStudentId}/devices`);
    const box = document.getElementById('devicesList');
    box.innerHTML = items.length
        ? items.map(d => `<div class="rounded-lg bg-slate-50 px-3 py-2 text-sm text-slate-700">${d.deviceId}</div>`).join('')
        : '<div class="text-sm text-slate-400">Cihaz yoxdur</div>';
}

async function unbindDevice() {
    if (!await AdminUI.confirmDelete('Cihaz kilidi açılsın?')) return;
    const res = await AdminUI.del(`/students/${devStudentId}/devices`);
    if (res.ok) { AdminUI.notify('Kilid açıldı'); refreshDevices(); }
    else AdminUI.error();
}
