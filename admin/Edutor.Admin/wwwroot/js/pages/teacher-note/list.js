AdminUI.listPage({
    api: '/teacher-note/list',
    deleteBase: '/teacher-note',
    confirmKey: 'Js_AreYouSure',
    columns: 5,
    params: ({ page, search }) => ({ page, search, scope: document.getElementById('scope').value }),
    row: (c, i, ctx) => {
        const title = [
            c.isMine ? '' : `<div class="mb-1"><span class="badge badge-primary">${AdminUI.escapeHtml(L["Note_SharedBy"])}: ${AdminUI.escapeHtml(c.authorName)}</span></div>`,
            `<span class="font-semibold text-slate-800">${AdminUI.escapeHtml(c.title)}</span>`,
            c.visibility === 'shared'
                ? `<span class="badge badge-neutral ml-2">${AdminUI.escapeHtml(L["Note_Shared"])}</span>`
                : `<span class="badge badge-neutral ml-2">${AdminUI.escapeHtml(L["Note_Private"])}</span>`,
            c.body ? `<p class="mt-1 truncate text-sm text-slate-500">${AdminUI.escapeHtml(c.body)}</p>` : ''
        ].join('');

        const material = c.contentTitle
            ? `<span class="text-slate-700">${AdminUI.escapeHtml(c.lessonTitle || '')} — ${AdminUI.escapeHtml(c.contentTitle)}</span>` +
              (c.positionSeconds != null ? `<span class="ml-1 text-slate-400">${fmtTime(c.positionSeconds)}</span>` : '')
            : '<span class="text-slate-400">—</span>';

        return [
            { html: title },
            { html: material, cls: 'text-slate-500' },
            { html: c.date ? AdminUI.escapeHtml(c.date) : '<span class="text-slate-400">—</span>', cls: 'text-slate-500' },
            {
                html: c.isDone
                    ? AdminUI.badge(L["Note_Done"], 'success')
                    : AdminUI.badge(L["Note_Pending"], 'warning')
            },
            { html: c.isMine ? AdminUI.actions(c.id) : '', cls: 'text-right' }
        ];
    }
});

function fmtTime(seconds) {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${String(s).padStart(2, '0')}`;
}

document.getElementById('scope').addEventListener('change', () => loadList(1));
