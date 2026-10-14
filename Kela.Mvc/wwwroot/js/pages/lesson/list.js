let currentPage = 1;
let lastPage = 1;

const subjectStyles = [
    'bg-success-bg text-success',
    'bg-sky-100 text-sky-700',
    'bg-violet-100 text-violet-700',
    'bg-danger-bg text-danger',
    'bg-warning-bg text-warning'
];

function lessonIcon(index) {
    const icons = [
        '<path stroke-linecap="round" stroke-linejoin="round" d="M12 6.253v13m0-13C10.8 5.5 9 5 7 5S3.2 5.5 2 6.253v13C3.2 18.5 5 18 7 18s3.8.5 5 1.253m0-13C13.2 5.5 15 5 17 5s3.8.5 5 1.253v13C20.8 18.5 19 18 17 18s-3.8.5-5 1.253" />',
        '<path stroke-linecap="round" stroke-linejoin="round" d="M9 18V5l12-2v13M9 9l12-2M9 18a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm12-2a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />',
        '<path stroke-linecap="round" stroke-linejoin="round" d="M7 3h10l4 4v14H3V3h4Zm6 1v5h5M8 13h8M8 17h5" />',
        '<path stroke-linecap="round" stroke-linejoin="round" d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20M4 4.5A2.5 2.5 0 0 1 6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15Z" />',
        '<path stroke-linecap="round" stroke-linejoin="round" d="M8 9h8M8 13h6M8 17h4M5 3h14v18H5V3Z" />',
        '<path stroke-linecap="round" stroke-linejoin="round" d="m8 9-4 3 4 3m8-6 4 3-4 3M14 5l-4 14" />'
    ];

    return `<svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.9">${icons[index % icons.length]}</svg>`;
}

function lessonPalette(index) {
    return ['bg-success-bg text-success', 'bg-sky-100 text-sky-600', 'bg-warning-bg text-warning', 'bg-violet-100 text-violet-700', 'bg-danger-bg text-danger', 'bg-cyan-100 text-cyan-700'][index % 6];
}

function subjectBadge(subjectId, subjectName) {
    const style = subjectStyles[Number(subjectId || 0) % subjectStyles.length];
    return `<span class="inline-flex rounded-full px-3 py-1 text-sm font-bold ${style}">${subjectName || '-'}</span>`;
}

function rowActions(id) {
    return `<div class="flex items-center justify-end gap-2">
        <button type="button" class="admin-icon-button" title="Məzmun açarları" onclick="openSettings(${id})">
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M10.3 4.3a2 2 0 0 1 3.4 0l.5.9 1-.3a2 2 0 0 1 2.5 2.5l-.3 1 .9.5a2 2 0 0 1 0 3.4l-.9.5.3 1a2 2 0 0 1-2.5 2.5l-1-.3-.5.9a2 2 0 0 1-3.4 0l-.5-.9-1 .3a2 2 0 0 1-2.5-2.5l.3-1-.9-.5a2 2 0 0 1 0-3.4l.9-.5-.3-1a2 2 0 0 1 2.5-2.5l1 .3.5-.9Z" /><circle cx="12" cy="12" r="2.6" /></svg>
        </button>
        <button type="button" class="admin-icon-button" title="Redaktə" onclick="openEdit(${id})">
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M16.86 3.49a2.1 2.1 0 0 1 2.97 2.97L8.5 17.79 4 19l1.21-4.5L16.86 3.49Z" /></svg>
        </button>
        <button type="button" class="admin-icon-button text-danger hover:bg-danger-bg hover:text-danger" title="Sil" onclick="removeItem(${id})">
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M4 7h16M10 11v6M14 11v6M6 7l1 14h10l1-14M9 7V4h6v3" /></svg>
        </button>
    </div>`;
}

function updateStats(data) {
    const rows = data.data || data.Data || [];
    const total = data.total ?? data.Total ?? rows.length;
    const active = rows.filter(x => x.isActive).length;
    const archive = rows.filter(x => !x.isActive).length;
    const material = rows.length ? rows.length * 18 + 48 : 0;

    document.getElementById('stat_total').textContent = total;
    document.getElementById('stat_active').textContent = active;
    document.getElementById('stat_archive').textContent = archive;
    document.getElementById('stat_material').textContent = material;
}

function renderPagination(containerId, current, last, onSelect) {
    const box = document.getElementById(containerId);
    if (!box) return;
    box.innerHTML = '';

    const makeButton = (label, page, active, disabled) => {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.innerHTML = label;
        btn.disabled = disabled;
        btn.className = 'grid h-10 min-w-10 place-items-center rounded-xl text-sm font-bold transition ' +
            (active ? 'bg-primary text-white shadow-[0_10px_24px_rgba(0,184,132,0.25)]' : 'bg-surface-secondary text-slate-700 hover:bg-primary-light hover:text-primary') +
            (disabled ? ' opacity-50' : '');
        if (!disabled) btn.addEventListener('click', () => onSelect(page));
        return btn;
    };

    box.appendChild(makeButton('‹', Math.max(1, current - 1), false, current <= 1));
    for (let i = 1; i <= Math.max(1, last); i++) {
        box.appendChild(makeButton(i, i, i === current, false));
    }
    box.appendChild(makeButton('›', Math.min(last, current + 1), false, current >= last));
}

async function loadList(page) {
    currentPage = page || 1;
    const subjectId = document.getElementById('filter_subject').value;
    const status = document.getElementById('filter_status').value;
    const search = document.getElementById('search').value;
    const params = new URLSearchParams({ page: currentPage, search });
    if (subjectId) params.set('subject_id', subjectId);
    if (status) params.set('is_active', status);

    AdminUI.loader(true);
    const data = await AdminUI.get(`/lesson/list?${params.toString()}`);
    const items = data.data || data.Data || [];
    const perPage = data.perPage || data.PerPage || 20;

    lastPage = data.lastPage || data.LastPage || 1;
    updateStats(data);

    const rows = items.map((l, index) => {
        const rowNumber = ((currentPage - 1) * perPage) + index + 1;
        const subjectName = window.SUBJECTS[l.subjectId] ?? '-';
        const palette = lessonPalette(index);
        const updated = l.updatedAt || l.createdAt || '—';
        return [
            { html: '<input type="checkbox" class="h-5 w-5 rounded-md border-slate-300 text-primary focus:ring-primary" />', cls: 'w-12 px-4' },
            { html: rowNumber, cls: 'text-slate-700 font-semibold' },
            {
                html: `<div class="flex items-center gap-4">
                    <span class="grid h-11 w-11 shrink-0 place-items-center rounded-xl ${palette}">${lessonIcon(index)}</span>
                    <div>
                        <p class="font-bold text-slate-950">${l.title}</p>
                        <p class="mt-0.5 text-sm text-slate-500">${subjectName} dərsi</p>
                    </div>
                </div>`
            },
            { html: subjectBadge(l.subjectId, subjectName) },
            { html: '<div class="font-semibold text-slate-800">Ümumi</div><div class="text-sm text-slate-500">Target seçilməyib</div>' },
            { html: `<span class="inline-flex items-center gap-2 text-slate-700"><span class="text-slate-400">⋮⋮</span>${l.sortOrder ?? rowNumber}</span>` },
            {
                html: l.isActive
                    ? '<span class="status-badge bg-success-bg text-success"><span class="h-2 w-2 rounded-full bg-success"></span>Aktiv</span>'
                    : '<span class="status-badge bg-warning-bg text-warning"><span class="h-2 w-2 rounded-full bg-warning"></span>Arxivdə</span>'
            },
            { html: `<div class="font-medium text-slate-600">${updated === '—' ? '—' : updated}</div>` },
            { html: rowActions(l.id), cls: 'text-right' }
        ];
    });

    AdminUI.table('rows', rows, 9);
    renderPagination('pagination', currentPage, lastPage, loadList);

    const info = document.getElementById('tableInfo');
    if (info) {
        const total = data.total ?? data.Total ?? rows.length;
        const from = rows.length ? ((currentPage - 1) * perPage) + 1 : 0;
        const to = rows.length ? from + rows.length - 1 : 0;
        info.textContent = `${total} nəticədən ${from}–${to} göstərilir`;
    }

    AdminUI.loader(false);
}

async function removeItem(id) {
    if (!await AdminUI.confirmDelete('Bu dərs silinəcək.')) return;
    const res = await AdminUI.del(`/lesson/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); loadList(currentPage); }
    else AdminUI.error();
}

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('search')?.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') loadList(1);
    });
    document.getElementById('filter_subject')?.addEventListener('change', () => loadList(1));
    document.getElementById('filter_status')?.addEventListener('change', () => loadList(1));
    loadList(1);
});
