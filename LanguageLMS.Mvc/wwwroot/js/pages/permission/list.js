let permissionData = [];

function roleBadges(roles) {
    if (!roles || !roles.length) return '<span class="text-slate-400">—</span>';
    const map = { admin: 'Admin', teacher: 'Müəllim', student: 'Şagird' };
    return roles.map(r => `<span class="mr-1 rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-600">${map[r] ?? r}</span>`).join('');
}

async function loadList() {
    AdminUI.loader(true);
    permissionData = await AdminUI.get('/permission/list');

    AdminUI.table('rows', permissionData.map(p => [
        { html: `<code class="text-sm text-slate-700">${p.name}</code>` },
        { html: roleBadges(p.roles) }
    ]), 2);

    AdminUI.loader(false);
}

document.addEventListener('DOMContentLoaded', loadList);
