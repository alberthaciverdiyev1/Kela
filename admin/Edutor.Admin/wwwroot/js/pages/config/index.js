(function () {
    const MODES = [
        { value: 'none', title: L["Config_ModeNone"], hint: L["Config_ModeNoneHint"] },
        { value: 'all', title: L["Config_ModeAll"], hint: L["Config_ModeAllHint"] },
        { value: 'groups', title: L["Config_ModeGroups"], hint: L["Config_ModeGroupsHint"] }
    ];

    let current = 'none';
    let blockedGroups = [];

    function renderModes() {
        document.getElementById('blockModes').innerHTML = MODES.map(m => `
            <label class="flex cursor-pointer flex-col gap-1 rounded-xl border-2 p-4 transition ${m.value === current
                ? 'border-primary bg-primary-light'
                : 'border-border hover:border-slate-300'}">
                <input type="radio" name="payment_block_mode" value="${m.value}" class="hidden"
                       ${m.value === current ? 'checked' : ''} />
                <span class="text-sm font-bold text-slate-800">${AdminUI.escapeHtml(m.title)}</span>
                <span class="text-xs text-slate-500">${AdminUI.escapeHtml(m.hint)}</span>
            </label>`).join('');

        document.querySelectorAll('#blockModes input[name="payment_block_mode"]').forEach(input => {
            input.addEventListener('change', () => {
                current = input.value;
                renderModes();
                toggleGroups();
            });
        });

        toggleGroups();
    }

    function toggleGroups() {
        document.getElementById('groupsBox').classList.toggle('hidden', current !== 'groups');
    }

    function renderGroups(groups) {
        const box = document.getElementById('groupList');
        if (!groups.length) {
            box.innerHTML = `<p class="text-sm text-slate-400">${AdminUI.escapeHtml(L["Js_Empty"])}</p>`;
            return;
        }

        box.innerHTML = groups.map(g => `
            <label class="flex cursor-pointer items-center gap-2 rounded-xl border border-border px-3 py-2 text-sm hover:bg-slate-50">
                <input type="checkbox" class="config-group h-4 w-4 rounded border-slate-300 text-primary focus:ring-primary" value="${g.id}"
                       ${blockedGroups.includes(g.id) ? 'checked' : ''} />
                <span class="font-medium text-slate-700">${AdminUI.escapeHtml(g.name)}</span>
            </label>`).join('');
    }

    async function load() {
        const [config, groups] = await Promise.all([
            AdminUI.get('/config/current'),
            AdminUI.get('/config/groups')
        ]);

        current = config?.paymentBlockMode || 'none';
        blockedGroups = config?.paymentBlockedGroupIds || [];
        renderModes();
        renderGroups(groups || []);
    }

    document.getElementById('checkAll')?.addEventListener('click', () => {
        const boxes = Array.from(document.querySelectorAll('.config-group'));
        const allOn = boxes.every(b => b.checked);
        boxes.forEach(b => b.checked = !allOn);
    });

    document.getElementById('configForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();

        const fd = new FormData();
        fd.append('payment_block_mode', current);
        if (current === 'groups') {
            const selected = Array.from(document.querySelectorAll('.config-group:checked')).map(b => b.value);
            if (!selected.length) { AdminUI.notify(L["Config_BlockedGroupsRequired"], 'warning'); return; }
            selected.forEach(id => fd.append('payment_blocked_group_ids', id));
        }

        const res = await AdminUI.putForm('/config', fd);
        if (res.ok) AdminUI.notify(L["Js_Saved"]);
        else AdminUI.error(AdminUI.errText(res));
    });

    document.addEventListener('DOMContentLoaded', load);
})();
