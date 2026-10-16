// Kela admin panel — ui
// Parçalanmışdır: hər fayl eyni AdminUI obyektini genişləndirir.
window.AdminUI = window.AdminUI || {};

(function (UI) {
    const TONE_CLASS = { primary: 'is-primary', danger: 'is-danger' };
    const toneOf = (opts) => TONE_CLASS[(opts && opts.tone) || ''] || '';

    function badge(text, variant, opts) {
        const dot = (opts && opts.dot) ? '<span class="badge-dot"></span>' : '';
        return `<span class="badge badge-${variant || 'neutral'}">${dot}${text}</span>`;
    }

    function chip(text, tone) {
        return `<span class="chip chip-${tone || 'neutral'}">${text}</span>`;
    }

    function statCard(label, value, tone) {
        return `<article class="admin-card p-4">
            <p class="text-xs font-semibold uppercase tracking-wide text-slate-400">${label}</p>
            <p class="mt-1 text-2xl font-extrabold ${tone || 'text-slate-950'}">${value}</p>
        </article>`;
    }

    function actionButton(label, onclick, opts) {
        return `<button type="button" class="admin-row-action ${toneOf(opts)}" onclick="${onclick}">${label}</button>`;
    }

    function actionLink(label, href, opts) {
        return `<a href="${href}" class="admin-row-action ${toneOf(opts)}">${label}</a>`;
    }

    function rowActions(parts) {
        return `<div class="admin-row-actions">${parts.filter(Boolean).join('')}</div>`;
    }

    function actions(id, opts) {
        opts = opts || {};
        return rowActions([
            opts.extra,
            actionButton(L["Js_Edit"], `openEdit(${id})`, { tone: 'primary' }),
            actionButton(L["Common_Delete"], `removeItem(${id})`, { tone: 'danger' })
        ]);
    }

    function statusSwitch(checked, onclick) {
        return `<button type="button" class="admin-switch${checked ? ' is-on' : ''}" role="switch"
                        aria-checked="${checked}" title="${checked ? L["Js_Deactivate"] : L["Js_Activate"]}"
                        onclick="${onclick}"><span></span></button>`;
    }

    function toggleHidden(id, hidden) {
        const el = document.getElementById(id);
        if (el) el.classList.toggle('hidden', hidden);
    }

    function syncTrashButton(btnId, on) {
        const btn = document.getElementById(btnId);
        if (btn) {
            btn.classList.toggle('admin-button-secondary', !on);
            btn.classList.toggle('admin-button-primary', on);
        }
        // Silinənlər rejimində yalnız bərpa mümkündür — aktiv/deaktiv mənasızdır.
        toggleHidden('bulkRestore', !on);
        toggleHidden('bulkActivate', on);
        toggleHidden('bulkDeactivate', on);
    }

    function restoreButton(id) {
        return rowActions([actionButton(L["Common_Restore"], `restoreRow(${id})`, { tone: 'primary' })]);
    }

    async function restoreItem(url, id, cb) {
        if (!await confirmRestore()) return;
        const res = await UI.postJson(url, { ids: [id], action: 'restore' });
        if (res.ok) { UI.notify(L["Js_Restored"]); if (typeof cb === 'function') cb(); }
        else UI.error(UI.errText(res));
    }

    async function confirmRestore() {
        const result = await UI.swal({
            title: L["Js_ConfirmRestore"], icon: 'question', showCancelButton: true,
            confirmButtonText: L["Common_Restore"], cancelButtonText: L["Common_Cancel"]
        });
        return result.isConfirmed;
    }

    Object.assign(UI, { badge, chip, statCard, actionButton, actionLink, rowActions, actions, statusSwitch, toggleHidden, syncTrashButton, restoreButton, restoreItem, confirmRestore });
})(window.AdminUI);
