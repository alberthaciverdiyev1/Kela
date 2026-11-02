(function () {
    const TABS = [
        { key: 'groups', label: L["Panel_Groups"] },
        { key: 'subjects', label: L["Panel_Subjects"] },
        { key: 'lessons', label: L["Student_TabLessons"] },
        { key: 'homeworks', label: L["Panel_Homeworks"] },
        { key: 'attendance', label: L["Panel_Attendance"] },
        { key: 'payments', label: L["Panel_Payments"] },
        { key: 'progress', label: L["Panel_Progress"] },
        { key: 'results', label: L["Student_TabExams"] },
        { key: 'violations', label: L["Student_TabViolations"] }
    ];

    const root = document.getElementById('studentPanelRoot');
    const state = {
        studentId: root?.dataset.studentId ? Number(root.dataset.studentId) : null,
        tab: 'groups', subjectId: null, lessonId: null
    };

    const esc = AdminUI.escapeHtml;
    const empty = () => `<p class="text-sm text-slate-400">${esc(L["Common_NoData"])}</p>`;
    const bar = (percent) => `
        <div class="mt-2 h-2 w-full overflow-hidden rounded-full bg-slate-100">
            <div class="h-full rounded-full bg-primary" style="width:${Math.min(percent, 100)}%"></div>
        </div>`;

    function renderProfile(s) {
        document.getElementById('profile').innerHTML = `
            <div class="flex flex-wrap items-center gap-4">
                <div class="grid h-14 w-14 place-items-center rounded-2xl bg-primary-light text-lg font-bold text-primary">
                    ${esc((s.fullName || '?').slice(0, 2).toUpperCase())}
                </div>
                <div class="min-w-0 flex-1">
                    <p class="text-lg font-bold text-slate-900">${esc(s.fullName)}</p>
                    <p class="text-sm text-slate-500">${esc(s.email)}${s.phone ? ' · ' + esc(s.phone) : ''}</p>
                </div>
                <div class="flex items-center gap-2">
                    ${s.isActive
                        ? AdminUI.badge(esc(L["Common_Active"]), 'success')
                        : AdminUI.badge(esc(L["Common_Inactive"]), 'danger')}
                    <a href="/students" class="admin-button-secondary">${esc(L["Nav_Students"])}</a>
                </div>
            </div>`;
    }

    function renderStats(s) {
        const o = s.overview || {};
        const box = document.getElementById('stats');
        box.innerHTML = AdminUI.statCard(L["Lesson_Total"], o.totalLessons)
            + AdminUI.statCard(L["Common_TotalMaterials"], o.totalContents)
            + AdminUI.statCard(L["Panel_Completed"], o.completedContents, 'text-success')
            + AdminUI.statCard(L["Panel_Completion"], `${o.completionPercent}%`, 'text-primary');
        box.classList.remove('hidden');
    }

    const rowNo = (i) => `<td class="text-slate-400">${i + 1}</td>`;
    const fmtDate = (value) => {
        if (!value) return '—';
        const d = new Date(value);
        if (isNaN(d)) return esc(value);
        const p = (n) => String(n).padStart(2, '0');
        return `${p(d.getDate())}.${p(d.getMonth() + 1)}.${d.getFullYear()} ${p(d.getHours())}:${p(d.getMinutes())}`;
    };

    function renderTabs() {
        document.getElementById('tabs').innerHTML = TABS.map(t => `
            <button type="button" data-tab="${t.key}"
                    class="shrink-0 rounded-xl px-4 py-2 text-sm font-semibold transition ${t.key === state.tab
                        ? 'bg-primary text-white'
                        : 'border border-border bg-white text-slate-600 hover:bg-slate-50'}">
                ${esc(t.label)}
            </button>`).join('');

        document.querySelectorAll('#tabs [data-tab]').forEach(btn => {
            btn.addEventListener('click', () => {
                state.tab = btn.dataset.tab;
                state.subjectId = null;
                state.lessonId = null;
                renderTabs();
                loadTab();
            });
        });
    }

    function renderGroups(groups) {
        if (!groups.length) return empty();
        return `<div class="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">${groups.map(g => `
            <div class="rounded-xl border border-border p-4">
                <p class="font-semibold text-slate-800">${esc(g.name)}</p>
                <p class="mt-1 text-xs text-slate-500">${g.studentCount} ${esc(L["Panel_StudentCount"])} · ${g.completedContents}/${g.totalContents}</p>
                ${bar(g.progressPercent)}
                <p class="mt-1 text-xs font-semibold text-slate-500">${g.progressPercent}%</p>
            </div>`).join('')}</div>`;
    }

    async function renderSubjects() {
        const subjects = await AdminUI.get(`/student-panel/${state.studentId}/subjects`) || [];
        if (!subjects.length) return empty();

        const lessons = state.subjectId
            ? await AdminUI.get(`/student-panel/${state.studentId}/subjects/${state.subjectId}/lessons`) || []
            : null;

        const contents = state.lessonId
            ? await AdminUI.get(`/student-panel/${state.studentId}/lessons/${state.lessonId}/contents`) || []
            : null;

        const chips = subjects.map(s => `
            <button type="button" data-subject="${s.id}"
                    class="rounded-xl border px-3 py-1.5 text-sm font-semibold transition ${s.id === state.subjectId
                        ? 'border-primary bg-primary-light text-primary'
                        : 'border-border text-slate-600 hover:bg-slate-50'}">
                ${esc(s.name)}
            </button>`).join('');

        const lessonRows = lessons === null ? '' : `
            <div class="mt-4 space-y-2">
                <p class="text-[11px] font-bold uppercase tracking-wider text-slate-400">${esc(L["Panel_Lessons"])}</p>
                ${lessons.length ? lessons.map(l => `
                    <button type="button" data-lesson="${l.id}"
                            class="flex w-full items-center justify-between rounded-xl border px-3 py-2 text-left text-sm font-semibold transition ${l.id === state.lessonId
                                ? 'border-primary bg-primary-light text-primary'
                                : 'border-border text-slate-700 hover:bg-slate-50'}">
                        <span>${esc(l.title)}</span>
                        <span class="text-xs text-slate-400">›</span>
                    </button>`).join('') : empty()}
            </div>`;

        const contentRows = contents === null ? '' : `
            <div class="mt-4 space-y-2">
                <p class="text-[11px] font-bold uppercase tracking-wider text-slate-400">${esc(L["Panel_Contents"])}</p>
                ${contents.length ? contents.map(c => `
                    <div class="flex items-center justify-between gap-3 rounded-xl border border-border px-3 py-2">
                        <span class="min-w-0 truncate text-sm font-medium text-slate-700">${esc(c.title)}</span>
                        ${c.type ? AdminUI.badge(esc(c.type), 'neutral') : ''}
                    </div>`).join('') : empty()}
            </div>`;

        return `
            <div class="flex flex-wrap gap-2">${chips}</div>
            ${lessonRows}
            ${contentRows}`;
    }

    const HOMEWORK_STATUS = {
        assigned: { key: 'Homework_StatusAssigned', tone: 'warning' },
        submitted: { key: 'Homework_StatusSubmitted', tone: 'primary' },
        graded: { key: 'Homework_StatusGraded', tone: 'success' },
        returned: { key: 'Homework_StatusReturned', tone: 'danger' }
    };

    function renderHomeworks(rows) {
        if (!rows.length) return empty();
        return `<div class="space-y-2">${rows.map(h => `
            <div class="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border px-3 py-2">
                <div class="min-w-0">
                    <p class="truncate font-semibold text-slate-800">${esc(h.title)}</p>
                    <p class="mt-0.5 text-xs text-slate-400">${esc(L["Homework_DueDate"])}: ${AdminUI.escapeHtml(h.dueDate.slice(0, 10))}</p>
                </div>
                <div class="flex items-center gap-2">
                    ${h.score !== null && h.score !== undefined
                        ? AdminUI.badge(`${h.score}/${h.maxScore}`, 'success')
                        : ''}
                    ${AdminUI.badge(esc(L[(HOMEWORK_STATUS[h.status] || {}).key] || h.status),
                        (HOMEWORK_STATUS[h.status] || {}).tone || 'neutral')}
                </div>
            </div>`).join('')}</div>`;
    }

    function renderAttendance(a) {
        const stats = `
            <div class="grid gap-3 sm:grid-cols-3">
                <div class="rounded-xl border border-border p-4">
                    <p class="text-xs font-semibold uppercase text-slate-400">${esc(L["Attendance_MyTitle"])}</p>
                    <p class="mt-1 text-2xl font-bold text-slate-800">${a.percent}%</p>
                </div>
                <div class="rounded-xl border border-border p-4">
                    <p class="text-xs font-semibold uppercase text-slate-400">${esc(L["Attendance_Present"])}</p>
                    <p class="mt-1 text-2xl font-bold text-success">${a.presentCount}</p>
                </div>
                <div class="rounded-xl border border-border p-4">
                    <p class="text-xs font-semibold uppercase text-slate-400">${esc(L["Attendance_Absent"])}</p>
                    <p class="mt-1 text-2xl font-bold text-danger">${a.absentCount}</p>
                </div>
            </div>`;

        const rows = a.recent?.length ? `
            <div class="mt-4 space-y-2">${a.recent.map(r => `
                <div class="flex items-center justify-between rounded-xl border border-border px-3 py-2 text-sm">
                    <span class="font-medium text-slate-700">${esc(r.date)} · ${esc(r.groupName)}</span>
                    ${r.isPresent
                        ? AdminUI.badge(esc(L["Attendance_Present"]), 'success')
                        : AdminUI.badge(esc(L["Attendance_Absent"]), 'danger')}
                </div>`).join('')}</div>` : '';

        return stats + rows;
    }

    const PAYMENT_STATUS = {
        current: { key: 'Payment_StatusCurrent', tone: 'success' },
        upcoming: { key: 'Payment_StatusUpcoming', tone: 'warning' },
        overdue: { key: 'Payment_StatusOverdue', tone: 'danger' },
        unpaid: { key: 'Payment_StatusUnpaid', tone: 'neutral' }
    };

    function renderPayments(p) {
        if (!p || !p.status) return empty();

        const status = PAYMENT_STATUS[p.status] || { key: 'Common_Status', tone: 'neutral' };
        const info = `
            <div class="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
                <div class="rounded-xl border border-border p-4">
                    <p class="text-xs font-semibold uppercase text-slate-400">${esc(L["Common_Status"])}</p>
                    <p class="mt-2">${AdminUI.badge(esc(L[status.key]), status.tone)}</p>
                </div>
                <div class="rounded-xl border border-border p-4">
                    <p class="text-xs font-semibold uppercase text-slate-400">${esc(L["Panel_NextDue"])}</p>
                    <p class="mt-1 font-bold text-slate-800">${p.nextDueOn ? esc(p.nextDueOn) : '—'}</p>
                </div>
                <div class="rounded-xl border border-border p-4">
                    <p class="text-xs font-semibold uppercase text-slate-400">${esc(L["Panel_LastPaid"])}</p>
                    <p class="mt-1 font-bold text-slate-800">${p.lastPaidOn ? esc(p.lastPaidOn) : '—'}</p>
                </div>
                <div class="rounded-xl border border-border p-4">
                    <p class="text-xs font-semibold uppercase text-slate-400">${esc(L["Panel_Amount"])}</p>
                    <p class="mt-1 font-bold text-slate-800">${p.amount !== null ? `${p.amount} ${esc(p.currency || '')}` : '—'}</p>
                </div>
            </div>`;

        const history = p.history?.length ? `
            <div class="mt-4 space-y-2">
                <p class="text-[11px] font-bold uppercase tracking-wider text-slate-400">${esc(L["Panel_History"])}</p>
                ${p.history.map(h => `
                    <div class="flex items-center justify-between rounded-xl border border-border px-3 py-2 text-sm">
                        <span class="font-medium text-slate-700">${esc(h.paidOn)}</span>
                        <span class="font-semibold text-slate-800">${h.amount} ${esc(h.currency)}</span>
                    </div>`).join('')}
            </div>` : '';

        return info + history;
    }

    function renderProgress(s) {
        const rows = s.bySubject?.length ? s.bySubject.map(x => `
            <div class="rounded-xl border border-border p-4">
                <div class="flex items-center justify-between">
                    <p class="font-semibold text-slate-800">${esc(x.subjectName)}</p>
                    <p class="text-sm font-bold text-slate-600">${x.completionPercent}%</p>
                </div>
                ${bar(x.completionPercent)}
                <p class="mt-1 text-xs text-slate-400">${x.completedContents}/${x.totalContents}</p>
            </div>`).join('') : empty();

        return `
            <div class="rounded-xl border border-border p-4">
                <p class="text-xs font-semibold uppercase text-slate-400">${esc(L["Panel_Completion"])}</p>
                <p class="mt-1 text-2xl font-bold text-slate-800">${s.overview.completionPercent}%</p>
                <p class="mt-1 text-xs text-slate-400">${s.overview.completedContents}/${s.overview.totalContents}</p>
            </div>
            <div class="mt-4 grid gap-3 sm:grid-cols-2">${rows}</div>`;
    }

    function renderLessons(p) {
        if (!p.items?.length) return empty();
        return `<div class="overflow-x-auto">
            <table class="admin-table">
                <thead><tr>
                    <th class="w-16">#</th>
                    <th>${esc(L["Common_Lesson"])}</th>
                    <th class="w-40">${esc(L["Panel_Completed"])}</th>
                    <th class="w-40">${esc(L["Panel_Completion"])}</th>
                    <th>${esc(L["Student_NextMaterial"])}</th>
                </tr></thead>
                <tbody>${p.items.map((x, i) => `<tr>
                    ${rowNo(i)}
                    <td class="font-semibold text-slate-800">${esc(x.lessonTitle)}</td>
                    <td class="text-slate-500">${x.completedContents} / ${x.totalContents}</td>
                    <td>
                        <div class="flex items-center gap-2">
                            <div class="h-2 w-24 overflow-hidden rounded-full bg-slate-200"><div class="h-full rounded-full bg-primary" style="width:${Math.min(x.progressPercent, 100)}%"></div></div>
                            <span class="text-sm font-semibold text-slate-700">${x.progressPercent}%</span>
                        </div>
                    </td>
                    <td class="text-slate-500">${esc(x.nextContent?.title ?? '—')}</td>
                </tr>`).join('')}</tbody>
            </table>
        </div>`;
    }

    function renderResults(rows) {
        if (!rows?.length) return `<p class="text-sm text-slate-400">${esc(L["Common_NoResults"])}</p>`;
        return `<div class="overflow-x-auto">
            <table class="admin-table">
                <thead><tr>
                    <th class="w-16">#</th>
                    <th>${esc(L["Common_Exam"])}</th>
                    <th class="w-32">${esc(L["GroupStat_Attempt"])}</th>
                    <th class="w-40">${esc(L["Student_CorrectOverTotal"])}</th>
                    <th class="w-32">${esc(L["Panel_Score"])}</th>
                    <th class="w-44">${esc(L["Common_Date"])}</th>
                </tr></thead>
                <tbody>${rows.map((r, i) => `<tr>
                    ${rowNo(i)}
                    <td class="font-semibold text-slate-800">${esc(r.quizTitle)}</td>
                    <td class="text-slate-500">#${r.attemptTime}</td>
                    <td class="text-slate-500">${r.correctCount} / ${r.totalQuestions}</td>
                    <td><span class="font-semibold ${r.scorePercent >= 50 ? 'text-emerald-600' : 'text-red-600'}">${r.scorePercent}%</span></td>
                    <td class="text-slate-500">${fmtDate(r.startDate)}</td>
                </tr>`).join('')}</tbody>
            </table>
        </div>`;
    }

    function renderViolations(rows) {
        if (!rows?.length) return `<p class="text-sm text-slate-400">${esc(L["Student_NoViolations"])}</p>`;
        return `<div class="overflow-x-auto">
            <table class="admin-table">
                <thead><tr>
                    <th class="w-16">#</th>
                    <th>${esc(L["Common_Type"])}</th>
                    <th>${esc(L["Student_Page"])}</th>
                    <th class="w-44">${esc(L["Common_Date"])}</th>
                    <th>${esc(L["Student_UserAgent"])}</th>
                </tr></thead>
                <tbody>${rows.map((v, i) => `<tr>
                    ${rowNo(i)}
                    <td><span class="rounded bg-danger-bg px-2 py-0.5 text-xs font-semibold text-danger">${esc(v.type)}</span></td>
                    <td class="text-slate-500">${esc(v.path ?? '—')}</td>
                    <td class="text-slate-500">${fmtDate(v.createdAt)}</td>
                    <td class="max-w-xs truncate text-slate-400" title="${esc(v.userAgent)}">${esc(v.userAgent ?? '—')}</td>
                </tr>`).join('')}</tbody>
            </table>
        </div>`;
    }

    async function loadTab() {
        const body = document.getElementById('tabBody');
        body.innerHTML = `<p class="text-sm text-slate-400">${esc(L["Common_LoadingResults"])}</p>`;

        try {
            let html;
            switch (state.tab) {
                case 'groups': html = renderGroups(await AdminUI.get(`/student-panel/${state.studentId}/groups`) || []); break;
                case 'subjects': html = await renderSubjects(); break;
                case 'lessons': html = renderLessons(await AdminUI.get(`/student-panel/${state.studentId}/lesson-progress`)); break;
                case 'homeworks': html = renderHomeworks(await AdminUI.get(`/student-panel/${state.studentId}/homeworks`) || []); break;
                case 'attendance': html = renderAttendance(await AdminUI.get(`/student-panel/${state.studentId}/attendance`)); break;
                case 'payments': html = renderPayments(await AdminUI.get(`/student-panel/${state.studentId}/payments`)); break;
                case 'progress': html = renderProgress(await AdminUI.get(`/student-panel/${state.studentId}/progress`)); break;
                case 'results': html = renderResults(await AdminUI.get(`/student-panel/${state.studentId}/results`) || []); break;
                case 'violations': html = renderViolations(await AdminUI.get(`/student-panel/${state.studentId}/violations`) || []); break;
                default: html = empty();
            }
            body.innerHTML = html;
            bindSubjectEvents();
        } catch (e) {
            body.innerHTML = `<p class="text-sm text-danger">${esc(L["Js_ErrorOccured"])}</p>`;
        }
    }

    function bindSubjectEvents() {
        document.querySelectorAll('#tabBody [data-subject]').forEach(btn => {
            btn.addEventListener('click', () => {
                state.subjectId = Number(btn.dataset.subject);
                state.lessonId = null;
                loadTab();
            });
        });

        document.querySelectorAll('#tabBody [data-lesson]').forEach(btn => {
            btn.addEventListener('click', () => {
                state.lessonId = Number(btn.dataset.lesson);
                loadTab();
            });
        });
    }

    async function loadPanel(id) {
        state.studentId = id;

        const [profile, stats] = await Promise.all([
            AdminUI.get(`/student-panel/${id}/profile`),
            AdminUI.get(`/student-panel/${id}/progress`)
        ]);
        document.getElementById('panelEmpty')?.classList.add('hidden');
        document.getElementById('panel').classList.remove('hidden');
        renderProfile(profile);
        renderStats(stats);
        renderTabs();
        await loadTab();
    }

    function openStudent(id) {
        if (Number(state.studentId) === id) return;
        location.href = `/student-panel/${id}`;
    }

    async function searchStudents(term) {
        const rows = await AdminUI.get(`/student-panel/students?search=${encodeURIComponent(term)}`) || [];
        const box = document.getElementById('studentResults');

        box.innerHTML = rows.length ? rows.map(s => `
            <button type="button" data-student="${s.id}"
                    class="flex w-full items-center justify-between px-3 py-2 text-left text-sm hover:bg-slate-50">
                <span class="font-semibold text-slate-800">${esc(s.name)}</span>
                <span class="text-xs text-slate-400">${esc(s.email)}</span>
            </button>`).join('') : `<p class="px-3 py-2 text-sm text-slate-400">${esc(L["Js_NoResults"])}</p>`;

        box.classList.remove('hidden');
        box.querySelectorAll('[data-student]').forEach(btn => {
            btn.addEventListener('click', () => openStudent(Number(btn.dataset.student)));
        });
    }

    let timer;
    document.getElementById('studentSearch')?.addEventListener('input', (e) => {
        clearTimeout(timer);
        const term = e.target.value.trim();
        timer = setTimeout(() => searchStudents(term), 300);
    });

    document.addEventListener('click', (e) => {
        const box = document.getElementById('studentResults');
        if (!box.contains(e.target) && e.target.id !== 'studentSearch') box.classList.add('hidden');
    });

    document.addEventListener('DOMContentLoaded', async () => {
        if (state.studentId) await loadPanel(state.studentId);
    });
})();
