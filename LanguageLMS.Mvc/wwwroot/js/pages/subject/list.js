let currentPage = 1;
let lastPage = 1;

async function loadList(page) {
    currentPage = page || 1;
    const search = document.getElementById('search').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/subject/list?page=${currentPage}&search=${encodeURIComponent(search)}`);

    lastPage = data.lastPage;
    const tbody = document.getElementById('rows');
    tbody.innerHTML = '';

    if (!data.data.length) {
        tbody.innerHTML = '<tr><td colspan="3" class="px-4 py-10 text-center text-slate-400">Nəticə yoxdur</td></tr>';
    } else {
        data.data.forEach(s => {
            const tr = document.createElement('tr');
            tr.className = 'hover:bg-slate-50';
            tr.innerHTML = `
                <td class="px-4 py-3 text-slate-400">${s.id}</td>
                <td class="px-4 py-3 font-medium text-slate-800">${s.name}</td>
                <td class="px-4 py-3">
                    <div class="flex items-center justify-end gap-2">
                        <button type="button" class="text-[15px] px-2 py-1 text-primary hover:bg-primary/10 rounded-lg" onclick="openEdit(${s.id})">Redaktə</button>
                        <button type="button" class="text-[15px] px-2 py-1 text-red-600 hover:bg-red-50 rounded-lg" onclick="removeItem(${s.id})">Sil</button>
                    </div>
                </td>`;
            tbody.appendChild(tr);
        });
    }

    AdminUI.pagination('pagination', currentPage, lastPage, loadList);
    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu fənn silinəcək.')) return;
    const res = await AdminUI.del(`/subject/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => loadList(1));
