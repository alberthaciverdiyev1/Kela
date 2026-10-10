let currentPage = 1;
let lastPage = 1;

async function loadList(page) {
    currentPage = page || 1;
    const subjectId = document.getElementById('filter_subject').value;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/lesson/list?page=${currentPage}&subject_id=${subjectId}&search=${encodeURIComponent(search)}`);

    lastPage = data.lastPage;
    const tbody = document.getElementById('rows');
    tbody.innerHTML = '';

    if (!data.data.length) {
        tbody.innerHTML = '<tr><td colspan="6" class="px-4 py-10 text-center text-slate-400">Nəticə yoxdur</td></tr>';
    } else {
        data.data.forEach(l => {
            const active = l.isActive
                ? '<span class="rounded-full bg-emerald-100 px-2 py-0.5 text-xs text-emerald-700">Aktiv</span>'
                : '<span class="rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-500">Deaktiv</span>';
            const tr = document.createElement('tr');
            tr.className = 'hover:bg-slate-50';
            tr.innerHTML = `
                <td class="px-4 py-3 text-slate-400">${l.id}</td>
                <td class="px-4 py-3 font-medium text-slate-800">${l.title}</td>
                <td class="px-4 py-3 text-slate-500">${window.SUBJECTS[l.subjectId] ?? '-'}</td>
                <td class="px-4 py-3 text-slate-500">${l.sortOrder}</td>
                <td class="px-4 py-3">${active}</td>
                <td class="px-4 py-3">
                    <div class="flex items-center justify-end gap-2">
                        <button type="button" class="px-2 py-1 text-primary hover:bg-primary/10 rounded-lg" onclick="openEdit(${l.id})">Redaktə</button>
                        <button type="button" class="px-2 py-1 text-red-600 hover:bg-red-50 rounded-lg" onclick="removeItem(${l.id})">Sil</button>
                    </div>
                </td>`;
            tbody.appendChild(tr);
        });
    }
    renderPagination();
    AdminUI.loader(false);
}

function renderPagination() {
    const box = document.getElementById('pagination');
    box.innerHTML = '';
    for (let i = 1; i <= lastPage; i++) {
        const a = document.createElement('button');
        a.type = 'button';
        a.textContent = i;
        a.className = 'rounded-lg px-3 py-1.5 text-sm ' +
            (i === currentPage ? 'bg-primary text-white' : 'border border-slate-200 bg-white text-slate-600 hover:bg-slate-50');
        a.onclick = () => loadList(i);
        box.appendChild(a);
    }
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu dərs silinəcək.')) return;
    const res = await AdminUI.del(`/lesson/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
