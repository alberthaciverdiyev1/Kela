// Konversiya tapşırıqları.
window.JobsPage = (function () {
    var pager = AdminUI.createPager('jobPagination', reload);

    const statusVariant = (status) => {
        switch (status) {
            case 'completed': return 'success';
            case 'failed': return 'danger';
            case 'cancelled': return 'neutral';
            case 'pending': case 'queued': return 'warning';
            default: return 'primary';
        }
    };

    async function load(pageNo) {
        const status = document.getElementById('jobStatus').value;
        const data = await AdminUI.get(`/node/jobs?page=${pageNo}&perPage=25${status ? `&status=${status}` : ''}`);

        pager.apply(data);

        AdminUI.table('jobRows', (data.data || []).map((job, i) => {
            const percent = Math.round((job.progress || 0) * 100);
            const bar = `<div class="flex items-center gap-2"><div class="h-1.5 w-20 overflow-hidden rounded-full bg-slate-200">
                    <div class="h-full rounded-full bg-primary" style="width:${percent}%"></div></div>
                    <span class="text-xs font-semibold text-slate-500">${percent}%</span></div>`;

            const canRequeue = ['failed', 'cancelled'].includes(job.status);

            return [
                { html: AdminUI.rowNo(job, i, pager.page, pager.perPage), cls: 'text-slate-400' },
                { html: `<span class="font-medium text-slate-800">${AdminUI.escapeHtml(job.videoTitle || '#' + job.videoId)}</span>` },
                { html: `<span class="text-sm font-semibold text-slate-600">${AdminUI.escapeHtml(job.resolution)}</span>` },
                { html: AdminUI.badge(job.status, statusVariant(job.status)) + (job.skipped ? ' <span class="text-xs text-slate-400">(atlanıb)</span>' : '') },
                { html: bar },
                { html: `<span class="text-sm text-slate-500">${AdminUI.escapeHtml(job.nodeName || '—')}</span>` },
                { html: `<span class="text-sm text-slate-500">${job.attempts}/${job.maxAttempts}</span>` },
                {
                    html: AdminUI.rowActions([
                        canRequeue ? AdminUI.actionButton(L["Node_Requeue"], `JobsPage.requeue(${job.id})`, { tone: 'primary' }) : '',
                        job.errorMessage ? AdminUI.actionButton(L["Job_Error"], `JobsPage.error('${encodeURIComponent(job.errorMessage)}')`) : ''
                    ]), cls: 'text-right'
                }
            ];
        }), 8, { empty: L["Js_AdjustSearch"] });

    }

    // Pagination rəqəm səhifə ötürür; filtr dəyişəndə `filterChanged` səhifəni 1-ə qaytarır.
    function reload(page) {
        return load(Number.isInteger(page) && page > 0 ? page : pager.page);
    }

    function filterChanged() {
        return load(1);
    }

    async function requeue(id) {
        if (!await AdminUI.confirmDelete(L["Node_ConfirmRequeue"])) return;

        const res = await AdminUI.postJson(`/node/jobs/${id}/requeue`, {});
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); reload(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    function error(message) {
        AdminUI.swal({ title: L["Job_Error"], text: decodeURIComponent(message), icon: 'error' });
    }

    return { reload, filterChanged, requeue, error };
})();
