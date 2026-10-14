let currentPage = 1;
let lastPage = 1;

function studentActions(s) {
    const toggle = s.isActive ? 'Deaktiv et' : 'Aktiv et';
    return `<div class="flex items-center justify-end gap-2">
        <button type="button" class="px-2 py-1 text-primary hover:bg-primary/10 rounded-lg" onclick="toggleStatus(${s.id}, ${!s.isActive})">${toggle}</button>
        <button type="button" class="px-2 py-1 text-navy hover:bg-slate-100 rounded-lg" onclick="openDevices(${s.id})">Cihazlar</button>
        <button type="button" class="px-2 py-1 text-red-600 hover:bg-red-50 rounded-lg" onclick="removeItem(${s.id})">Sil</button>
    </div>`;
}

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    const active = document.getElementById('filter_active').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/students/list?page=${currentPage}&search=${encodeURIComponent(search)}&is_active=${active}`);
    lastPage = data.lastPage;

    AdminUI.table('rows', data.data.map(s => [
        { html: s.id, cls: 'text-slate-400' },
        { html: s.fullName, cls: 'font-medium text-slate-800' },
        { html: s.email, cls: 'text-slate-500' },
        { html: s.phone ?? '-', cls: 'text-slate-500' },
        { html: s.isActive ? AdminUI.badge('Aktiv', 'success') : AdminUI.badge('Deaktiv', 'neutral') },
        { html: studentActions(s), cls: 'text-right' }
    ]), 6, { rowIds: data.data.map(s => s.id), selectable: true });

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function toggleStatus(id, isActive) {
    const res = await AdminUI.postJson(`/students/${id}/status`, { isActive });
    if (res.ok) { AdminUI.notify('Status dəyişdi'); loadList(currentPage); }
    else AdminUI.error();
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu şagird silinəcək.')) return;
    const res = await AdminUI.del(`/students/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
