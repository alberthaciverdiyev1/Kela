(function () {
    function fillRolePermissions() {
        const role = document.getElementById('role').value;
        const box = document.getElementById('rolePermissions');
        box.innerHTML = permissionData.map(p =>
            `<option value="${p.name}" ${p.roles.includes(role) ? 'selected' : ''}>${p.name}</option>`).join('');
    }

    window.openRole = function () {
        if (!permissionData.length) { AdminUI.error('Əvvəlcə icazə yaradın'); return; }
        document.getElementById('role').value = 'teacher';
        fillRolePermissions();
        document.getElementById('roleModal').classList.remove('hidden');
        document.getElementById('roleModal').classList.add('flex');
    };
    window.closeRole = function () {
        document.getElementById('roleModal').classList.add('hidden');
        document.getElementById('roleModal').classList.remove('flex');
    };

    document.getElementById('role').addEventListener('change', fillRolePermissions);

    document.getElementById('roleForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const permissions = [...document.getElementById('rolePermissions').selectedOptions].map(o => o.value);
        const res = await AdminUI.postJson('/permission/role', { role: document.getElementById('role').value, permissions });
        if (res.ok) { AdminUI.notify('Yadda saxlanıldı'); window.closeRole(); loadList(); }
        else AdminUI.error();
    });
})();
