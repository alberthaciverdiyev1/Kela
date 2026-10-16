// Birdəfəlik qeydiyyat kodları.
window.CodesPage = (function () {
    function open() { AdminUI.openModal('codeModal'); }

    function close() { AdminUI.closeModal('codeModal'); }

    async function reload() {
        const codes = await AdminUI.get('/node/codes');

        AdminUI.table('codeRows', (codes || []).map(code => [
            { html: `<code class="cursor-pointer rounded-lg bg-surface-secondary px-2 py-1 text-sm font-bold text-slate-800" onclick="CodesPage.copy('${code.code}')" title="${L["Node_Copied"]}">${code.code}</code>` },
            { html: `<span class="text-sm text-slate-500">${code.note || '—'}</span>` },
            { html: `<span class="text-sm text-slate-500">${code.expiresAt ? new Date(code.expiresAt).toLocaleString() : L["Node_NoExpiry"]}</span>` },
            {
                html: code.isUsed ? AdminUI.badge(L["Node_Used"], 'neutral')
                    : code.isExpired ? AdminUI.badge(L["Node_Expired"], 'danger')
                        : AdminUI.badge(L["Node_Unused"], 'success')
            },
            { html: AdminUI.rowActions([AdminUI.actionButton(L["Common_Delete"], `CodesPage.remove(${code.id})`, { tone: 'danger' })]), cls: 'text-right' }
        ]), 5, { empty: L["Node_NoCodes"] });
    }

    async function copy(code) {
        try {
            await navigator.clipboard.writeText(code);
            AdminUI.notify(L["Node_Copied"]);
        } catch (e) {
            AdminUI.error(L["Js_ErrorOccured"]);
        }
    }

    async function remove(id) {
        if (!await AdminUI.confirmDelete()) return;

        const res = await AdminUI.del(`/node/codes/${id}`);
        if (res.ok) { AdminUI.notify(L["Js_Deleted"]); reload(); }
        else AdminUI.error(AdminUI.errText(res));
    }

    function create() {
        document.getElementById('codeForm').reset();
        open();
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.getElementById('createCodeBtn').addEventListener('click', create);

        document.getElementById('codeForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            const form = new FormData(e.target);
            const res = await AdminUI.post('/node/codes', {
                note: form.get('note') || '',
                expires_hours: form.get('expires_hours') || 0
            });

            if (res.ok) { AdminUI.notify(L["Js_Created"]); close(); reload(); }
            else AdminUI.error(AdminUI.errText(res));
        });
    });

    return { reload, create, close, copy, remove };
})();
