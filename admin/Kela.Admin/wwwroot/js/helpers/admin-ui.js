// Kela admin panel — ui
// Parçalanmışdır: hər fayl eyni AdminUI obyektini genişləndirir.
window.AdminUI = window.AdminUI || {};

(function (UI) {
    const TONE_CLASS = { primary: 'is-primary', danger: 'is-danger' };
    const toneOf = (opts) => TONE_CLASS[(opts && opts.tone) || ''] || '';

    // İstifadəçi mətni innerHTML-ə düşməzdən əvvəl həmişə escape olunur.
    const esc = (v) => UI.escapeHtml(v);

    function badge(text, variant, opts) {
        const dot = (opts && opts.dot) ? '<span class="badge-dot"></span>' : '';
        return `<span class="badge badge-${variant || 'neutral'}">${dot}${esc(text)}</span>`;
    }

    function chip(text, tone) {
        return `<span class="chip chip-${tone || 'neutral'}">${esc(text)}</span>`;
    }

    function statCard(label, value, tone) {
        return `<article class="admin-card p-4">
            <p class="text-xs font-semibold uppercase tracking-wide text-slate-400">${esc(label)}</p>
            <p class="mt-1 text-2xl font-extrabold ${tone || 'text-slate-950'}">${esc(value)}</p>
        </article>`;
    }

    // "openEdit(12)" kimi expression-u data-action/data-args-a çevirir.
    function actionAttrs(actionOrExpr, args) {
        let action = actionOrExpr;
        let list = args;

        if (list === undefined) {
            const m = /^([\w.]+)\((.*)\)$/.exec(String(actionOrExpr).trim());
            if (!m) return ` onclick="${actionOrExpr}"`;
            action = m[1];
            const raw = m[2].trim();
            if (!raw) list = [];
            else {
                try { list = JSON.parse('[' + raw.replace(/'/g, '"') + ']'); }
                catch { return ` onclick="${actionOrExpr}"`; }
            }
        }

        const json = JSON.stringify(list || []).replace(/'/g, '&#39;');
        return ` data-action="${action}" data-args='${json}'`;
    }

    function actionButton(label, onclick, opts) {
        return `<button type="button" class="admin-row-action ${toneOf(opts)}"${actionAttrs(onclick)}>${esc(label)}</button>`;
    }

    // Yalnız ikonlu sətir əməliyyatı (tooltip ilə).
    function actionIcon(path, title, onclick, opts) {
        const safeTitle = esc(title);
        return `<button type="button" class="admin-row-action ${toneOf(opts)}" title="${safeTitle}" aria-label="${safeTitle}"${actionAttrs(onclick)}>
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.9">${path}</svg>
        </button>`;
    }

    function actionLink(label, href, opts) {
        return `<a href="${esc(href)}" class="admin-row-action ${toneOf(opts)}">${esc(label)}</a>`;
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
                        ${actionAttrs(onclick)}><span></span></button>`;
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


    // Tablar: klik → aktiv sinif + panellərin göstərilməsi (data-tab-panel).
    // Səhifə "tab:change" hadisəsini dinləyib öz yükləməsini edə bilər.
    function initTabs(root) {
        (root || document).querySelectorAll('.admin-tabs').forEach(box => {
            if (box.dataset.ready === '1') return;
            box.dataset.ready = '1';

            box.addEventListener('click', (e) => {
                const tab = e.target.closest('.admin-tab');
                if (!tab) return;
                selectTab(box, tab.dataset.tab);
            });
        });
    }

    function selectTab(boxOrId, name) {
        const box = typeof boxOrId === 'string' ? document.getElementById(boxOrId) : boxOrId;
        if (!box) return;
        box.querySelectorAll('.admin-tab').forEach(t => t.classList.toggle('is-active', t.dataset.tab === name));
        (box.dataset.panels
            ? document.querySelectorAll(box.dataset.panels)
            : document.querySelectorAll('[data-tab-panel]'))
            .forEach(p => p.classList.toggle('hidden', p.dataset.tabPanel !== name));

        document.dispatchEvent(new CustomEvent('tab:change', { detail: { tab: name } }));
    }

    Object.assign(UI, { actionAttrs, initTabs, selectTab, badge, chip, statCard, actionButton, actionIcon, actionLink, rowActions, actions, statusSwitch, toggleHidden, syncTrashButton, restoreButton, restoreItem, confirmRestore });
})(window.AdminUI);
