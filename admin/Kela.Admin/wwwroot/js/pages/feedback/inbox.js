// Superadmin inbox: siyahı + canlı SignalR bildirişləri.
(function () {
    let items = [];
    let connection = null;

    const list = () => document.getElementById('fbList');
    const badge = () => document.getElementById('fbBadge');

    function updateBadge() {
        const el = badge();
        if (!el) return;
        const unread = items.filter(i => i.status === 'new').length;
        el.textContent = String(unread);
        el.classList.toggle('hidden', unread === 0);
    }

    function render() {
        const box = list();
        if (!box) return;

        if (!items.length) {
            box.innerHTML = `<p class="p-4 text-center text-sm text-slate-400">${L["Feedback_Empty"]}</p>`;
            updateBadge();
            return;
        }

        box.innerHTML = items.map(f => `
            <article class="rounded-xl border p-3 ${f.status === 'new' ? 'border-primary/30 bg-primary-light/40' : 'border-border bg-white'}">
                <div class="flex items-start justify-between gap-2">
                    <div class="min-w-0">
                        <p class="truncate text-sm font-bold text-slate-800">${f.userName}</p>
                        <p class="truncate text-[11px] text-slate-400">${f.userEmail} · ${f.role}</p>
                    </div>
                    <span class="shrink-0 text-[11px] text-slate-400">${new Date(f.createdAt).toLocaleString('az')}</span>
                </div>
                <p class="mt-2 whitespace-pre-wrap break-words text-sm text-slate-700">${f.message}</p>
                ${f.imageUrl ? `<a href="${f.imageUrl}" target="_blank" rel="noopener"><img src="${f.imageUrl}" alt="" class="mt-2 max-h-40 rounded-lg border border-border object-contain" /></a>` : ''}
                <div class="mt-2 flex items-center gap-3">
                    ${f.status === 'new'
                        ? `<button type="button" class="text-xs font-bold text-primary hover:underline" onclick="markFeedbackRead(${f.id})">${L["Feedback_MarkRead"]}</button>`
                        : ''}
                    <button type="button" class="text-xs font-bold text-danger hover:underline" onclick="deleteFeedback(${f.id})">${L["Common_Delete"]}</button>
                </div>
            </article>`).join('');

        updateBadge();
    }

    async function load() {
        const data = await AdminUI.get('/feedback/list?perPage=50');
        items = data.data || [];
        render();
    }

    window.openFeedback = async function () {
        document.getElementById('fbPanel').classList.remove('hidden');
        if (!items.length) await load();
        else render();
    };

    window.closeFeedback = function () {
        document.getElementById('fbPanel').classList.add('hidden');
    };

    window.markFeedbackRead = async function (id) {
        const res = await AdminUI.postJson(`/feedback/${id}/read`, { isRead: true });
        if (res.ok) {
            const item = items.find(i => i.id === id);
            if (item) item.status = 'read';
            render();
        }
    };

    window.deleteFeedback = async function (id) {
        if (!await AdminUI.confirmDelete()) return;
        const res = await AdminUI.del(`/feedback/${id}`);
        if (res.ok) {
            items = items.filter(i => i.id !== id);
            render();
        }
    };

    function connectHub() {
        const cfg = window.FEEDBACK_CONFIG;
        if (!cfg || !cfg.token || typeof signalR === 'undefined') return;

        connection = new signalR.HubConnectionBuilder()
            // Token query/header ilə gedir; cookie lazım deyil. Əks halda brauzer
            // credentials:include göndərir və CORS '*' ilə uyğunsuz olur.
            .withUrl(cfg.hubUrl, { accessTokenFactory: () => cfg.token, withCredentials: false })
            .withAutomaticReconnect()
            .build();

        connection.on('feedback:new', (feedback) => {
            items.unshift(feedback);
            render();
            AdminUI.notify(L["Feedback_New"]);
        });

        connection.start().catch((err) => {
            // Hub əlçatmazsa siyahı yenə işləyir; səbəb konsolda qalır.
            console.error('feedback hub:', err && err.message ? err.message : err);
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        load();
        connectHub();
    });
})();
