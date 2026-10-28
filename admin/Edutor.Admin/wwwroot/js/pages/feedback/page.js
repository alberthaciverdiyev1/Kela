// Superadmin — geri bildirişlər səhifəsi: söhbət siyahısı + mesajlaşma (SignalR canlı).
(function () {
    let threads = [];
    let activeId = null;
    let connection = null;
    const $ = (id) => document.getElementById(id);

    const label = (t) => `${t.userName} · ${t.userEmail}`;
    const safeLabel = (t) => AdminUI.escapeHtml(label(t));

    function renderThreads() {
        const box = $('fbThreads');
        if (!box) return;

        if (!threads.length) {
            box.innerHTML = `<p class="p-4 text-center text-sm text-slate-400">${L["Feedback_Empty"]}</p>`;
            return;
        }

        box.innerHTML = threads.map(t => `
            <button type="button"${AdminUI.actionAttrs('selectThread', [t.id])}
                    class="w-full rounded-xl border p-3 text-left transition ${t.id === activeId
                        ? 'border-primary bg-primary-light'
                        : t.hasUnreadForAdmin ? 'border-primary/30 bg-primary-light/40 hover:bg-primary-light'
                        : 'border-border bg-white hover:bg-surface-secondary'}">
                <div class="flex items-center justify-between gap-2">
                    <span class="truncate text-sm font-bold text-slate-800">${safeLabel(t)}</span>
                    ${t.hasUnreadForAdmin ? '<span class="h-2 w-2 shrink-0 rounded-full bg-primary"></span>' : ''}
                </div>
                <p class="mt-1 truncate text-xs text-slate-500">${AdminUI.escapeHtml(t.messages[t.messages.length - 1]?.body || '')}</p>
                <p class="mt-1 text-[11px] text-slate-400">${new Date(t.lastMessageAt).toLocaleString('az')}</p>
            </button>`).join('');
    }

    function renderConversation() {
        const box = $('fbConversation');
        const form = $('fbReplyForm');
        if (!box || !form) return;

        const t = threads.find(x => x.id === activeId);
        if (!t) {
            box.innerHTML = `<p class="p-6 text-center text-sm text-slate-400">${L["Feedback_PickThread"]}</p>`;
            form.classList.add('hidden');
            return;
        }

        form.classList.remove('hidden');
        box.innerHTML = `
            <div class="mb-2 flex items-center justify-between gap-2 border-b border-border pb-2">
                <div class="min-w-0">
                    <p class="truncate text-sm font-bold text-slate-800">${safeLabel(t)}</p>
                    <p class="text-[11px] text-slate-400">${AdminUI.escapeHtml(t.role)}</p>
                </div>
                <button type="button" class="admin-row-action is-danger"${AdminUI.actionAttrs('deleteThread', [t.id])}>${L["Common_Delete"]}</button>
            </div>
            ${t.messages.map(m => `
                <div class="flex ${m.senderRole === 'admin' ? 'justify-end' : 'justify-start'}">
                    <div class="max-w-[75%] rounded-2xl px-3 py-2 text-sm ${m.senderRole === 'admin' ? 'bg-primary text-white' : 'bg-surface-secondary text-slate-800'}">
                        <p class="whitespace-pre-wrap break-words">${AdminUI.escapeHtml(m.body)}</p>
                        ${m.imageUrl ? `<a href="${AdminUI.escapeHtml(m.imageUrl)}" target="_blank" rel="noopener"><img src="${AdminUI.escapeHtml(m.imageUrl)}" alt="" class="mt-2 max-h-40 rounded-lg object-contain" /></a>` : ''}
                        <span class="mt-1 block text-[10px] ${m.senderRole === 'admin' ? 'text-white/70' : 'text-slate-400'}">
                            ${new Date(m.createdAt).toLocaleString('az')}${m.senderRole === 'admin' && m.isRead ? ' · ' + L["Feedback_Read"] : ''}
                        </span>
                    </div>
                </div>`).join('')}`;

        box.scrollTop = box.scrollHeight;
    }

    async function load() {
        const data = await AdminUI.get('/feedback/list?perPage=100');
        threads = data.data || [];
        renderThreads();
        renderConversation();
    }

    window.selectThread = async function (id) {
        activeId = id;
        const t = threads.find(x => x.id === id);
        if (t && t.hasUnreadForAdmin) {
            await AdminUI.postJson(`/feedback/${id}/read`, {});
            t.hasUnreadForAdmin = false;
            t.messages.forEach(m => { if (m.senderRole === 'user') m.isRead = true; });
        }
        renderThreads();
        renderConversation();
    };

    window.deleteThread = async function (id) {
        if (!await AdminUI.confirmDelete()) return;
        const res = await AdminUI.del(`/feedback/${id}`);
        if (res.ok) {
            threads = threads.filter(t => t.id !== id);
            if (activeId === id) activeId = null;
            renderThreads();
            renderConversation();
        }
    };

    document.getElementById('fbReplyForm')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const text = $('fbReply').value.trim();
        const cfg = window.FEEDBACK_CONFIG;
        if (!text || !cfg || !activeId) return;

        const fd = new FormData();
        fd.append('message', text);

        const res = await fetch(`${cfg.apiUrl}/api/feedback/${activeId}/messages`, {
            method: 'POST',
            headers: { Authorization: 'Bearer ' + cfg.token },
            body: fd
        });

        if (!res.ok) { AdminUI.error(L["Feedback_Error"]); return; }

        const payload = await res.json();
        threads = threads.map(t => t.id === activeId ? payload.data : t);
        $('fbReply').value = '';
        renderThreads();
        renderConversation();
    });

    function connectHub() {
        const cfg = window.FEEDBACK_CONFIG;
        if (!cfg || !cfg.token || typeof signalR === 'undefined') return;

        connection = new signalR.HubConnectionBuilder()
            .withUrl(cfg.hubUrl, { accessTokenFactory: () => cfg.token, withCredentials: false })
            .withAutomaticReconnect()
            .build();

        connection.on('feedback:message', (thread) => {
            const exists = threads.some(t => t.id === thread.id);
            threads = exists ? threads.map(t => t.id === thread.id ? thread : t) : [thread, ...threads];
            if (thread.hasUnreadForAdmin) AdminUI.notify(L["Feedback_New"]);
            renderThreads();
            if (thread.id === activeId) renderConversation();
        });

        connection.start().catch((err) => console.error('feedback hub:', err && err.message ? err.message : err));
    }

    document.addEventListener('DOMContentLoaded', async () => {
        await load();
        if (threads.length && activeId === null) await selectThread(threads[0].id);
        connectHub();
    });
})();
