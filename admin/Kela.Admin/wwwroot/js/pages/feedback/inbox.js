// Superadmin inbox: söhbət siyahısı + mesajlaşma (SignalR canlı).
(function () {
    let threads = [];
    let activeId = null;
    let connection = null;

    const $ = (id) => document.getElementById(id);

    function updateBadge() {
        const el = $('fbBadge');
        if (!el) return;
        const unread = threads.filter(t => t.hasUnreadForAdmin).length;
        el.textContent = String(unread);
        el.classList.toggle('hidden', unread === 0);
    }

    const threadLabel = (t) => `${t.userName} · ${t.userEmail}`;

    function renderList() {
        const box = $('fbList');
        if (!box) return;

        if (!threads.length) {
            box.innerHTML = `<p class="p-4 text-center text-sm text-slate-400">${L["Feedback_Empty"]}</p>`;
            updateBadge();
            return;
        }

        if (activeId === null) {
            box.innerHTML = threads.map(t => `
                <button type="button" onclick="openThread(${t.id})"
                        class="w-full rounded-xl border p-3 text-left transition hover:bg-surface-secondary ${t.hasUnreadForAdmin ? 'border-primary/30 bg-primary-light/40' : 'border-border bg-white'}">
                    <div class="flex items-center justify-between gap-2">
                        <span class="truncate text-sm font-bold text-slate-800">${threadLabel(t)}</span>
                        <span class="shrink-0 text-[11px] text-slate-400">${new Date(t.lastMessageAt).toLocaleString('az')}</span>
                    </div>
                    <p class="mt-1 truncate text-xs text-slate-500">${t.messages[t.messages.length - 1]?.body || ''}</p>
                </button>`).join('');
        } else {
            const t = threads.find(x => x.id === activeId);
            if (!t) { activeId = null; renderList(); return; }

            box.innerHTML = `
                <div class="flex items-center gap-2 border-b border-border pb-2">
                    <button type="button" class="admin-row-action" onclick="backToThreads()">←</button>
                    <span class="truncate text-sm font-bold text-slate-800">${threadLabel(t)}</span>
                    <button type="button" class="ml-auto admin-row-action is-danger" onclick="deleteFeedback(${t.id})">${L["Common_Delete"]}</button>
                </div>
                <div class="space-y-2 py-2">
                    ${t.messages.map(m => `
                        <div class="flex ${m.senderRole === 'admin' ? 'justify-end' : 'justify-start'}">
                            <div class="max-w-[80%] rounded-2xl px-3 py-2 text-sm ${m.senderRole === 'admin' ? 'bg-primary text-white' : 'bg-surface-secondary text-slate-800'}">
                                <p class="whitespace-pre-wrap break-words">${m.body}</p>
                                ${m.imageUrl ? `<a href="${m.imageUrl}" target="_blank" rel="noopener"><img src="${m.imageUrl}" alt="" class="mt-2 max-h-32 rounded-lg object-contain" /></a>` : ''}
                                <span class="mt-1 block text-[10px] ${m.senderRole === 'admin' ? 'text-white/70' : 'text-slate-400'}">${new Date(m.createdAt).toLocaleString('az')}${m.senderRole === 'admin' && m.isRead ? ' · ' + L["Feedback_Read"] : ''}</span>
                            </div>
                        </div>`).join('')}
                </div>
                <form id="fbReplyForm" class="flex items-end gap-2 border-t border-border pt-2">
                    <textarea id="fbReply" rows="2" class="admin-field flex-1 resize-none" placeholder="${L["Feedback_ReplyPlaceholder"]}"></textarea>
                    <button type="submit" class="admin-button-primary !min-h-0 !py-2">${L["Feedback_Send"]}</button>
                </form>`;

            $('fbReplyForm').addEventListener('submit', async (e) => {
                e.preventDefault();
                const body = $('fbReply').value.trim();
                if (!body) return;
                await sendReply(activeId, body);
            });
        }

        updateBadge();
    }

    async function load() {
        const data = await AdminUI.get('/feedback/list?perPage=50');
        threads = data.data || [];
        renderList();
    }

    window.openThread = async function (id) {
        activeId = id;
        const t = threads.find(x => x.id === id);
        if (t && t.hasUnreadForAdmin) {
            await AdminUI.postJson(`/feedback/${id}/read`, {});
            t.hasUnreadForAdmin = false;
            t.messages.forEach(m => { if (m.senderRole === 'user') m.isRead = true; });
        }
        renderList();
    };

    window.backToThreads = function () {
        activeId = null;
        renderList();
    };

    async function sendReply(id, body) {
        const cfg = window.FEEDBACK_CONFIG;
        if (!cfg) return;

        const fd = new FormData();
        fd.append('message', body);

        const res = await fetch(`${cfg.apiUrl}/api/feedback/${id}/messages`, {
            method: 'POST',
            headers: { Authorization: 'Bearer ' + cfg.token },
            body: fd
        });

        if (!res.ok) { AdminUI.error(L["Feedback_Error"]); return; }

        const payload = await res.json();
        const updated = payload.data;
        threads = threads.map(t => t.id === id ? updated : t);
        renderList();
    }

    window.openFeedback = async function () {
        $('fbPanel').classList.remove('hidden');
        if (!threads.length) await load();
        else renderList();
    };

    window.closeFeedback = function () {
        $('fbPanel').classList.add('hidden');
        activeId = null;
    };

    window.deleteFeedback = async function (id) {
        if (!await AdminUI.confirmDelete()) return;
        const res = await AdminUI.del(`/feedback/${id}`);
        if (res.ok) {
            threads = threads.filter(t => t.id !== id);
            if (activeId === id) activeId = null;
            renderList();
        }
    };

    function connectHub() {
        const cfg = window.FEEDBACK_CONFIG;
        if (!cfg || !cfg.token || typeof signalR === 'undefined') return;

        connection = new signalR.HubConnectionBuilder()
            .withUrl(cfg.hubUrl, { accessTokenFactory: () => cfg.token, withCredentials: false })
            .withAutomaticReconnect()
            .build();

        connection.on('feedback:message', (thread) => {
            const exists = threads.some(t => t.id === thread.id);
            threads = exists
                ? threads.map(t => t.id === thread.id ? thread : t)
                : [thread, ...threads];
            if (thread.hasUnreadForAdmin) AdminUI.notify(L["Feedback_New"]);
            renderList();
        });

        connection.start().catch((err) => {
            console.error('feedback hub:', err && err.message ? err.message : err);
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        load();
        connectHub();
    });
})();
