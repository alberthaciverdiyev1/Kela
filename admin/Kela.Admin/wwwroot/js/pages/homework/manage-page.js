(function () {
    const rowsBox = document.getElementById('rows');
    const homeworkId = Number(rowsBox.dataset.homeworkId);
    let assignments = [];

    function statusLabel(status) {
        return {
            assigned: L["Homework_StatusAssigned"],
            submitted: L["Homework_StatusSubmitted"],
            graded: L["Homework_StatusGraded"],
            returned: L["Homework_StatusReturned"]
        }[status] || status;
    }

    function statusVariant(status) {
        return {
            assigned: 'neutral',
            submitted: 'warning',
            graded: 'success',
            returned: 'danger'
        }[status] || 'neutral';
    }

    function formatDate(value) {
        return value ? new Date(value).toLocaleString('az', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' }) : '—';
    }

    async function load() {
        const search = document.getElementById('search').value.trim();
        const status = document.getElementById('filter_status').value;

        const data = await AdminUI.get(`/homework/${homeworkId}/assignments?per_page=500&search=${encodeURIComponent(search)}&status=${encodeURIComponent(status)}`);
        assignments = data.data || [];

        AdminUI.table('rows', assignments.map((a, i) => [
            { html: i + 1, cls: 'text-slate-400' },
            { html: `<div class="font-medium text-slate-800">${AdminUI.escapeHtml(a.fullName)}</div><div class="text-xs text-slate-400">${AdminUI.escapeHtml(a.email)}</div>` },
            { html: AdminUI.badge(statusLabel(a.status), statusVariant(a.status)) + (a.isLate ? ` <span class="badge badge-warning">${L["Homework_Late"]}</span>` : '') },
            { html: formatDate(a.submittedAt), cls: 'text-sm text-slate-500' },
            {
                html: a.quizzes.length
                    ? a.quizzes.map(q => `${AdminUI.escapeHtml(q.title)}: ${q.completed ? `${q.correctCount}/${q.questionCount}` : '—'}`).join('<br>')
                    : '<span class="text-slate-400">—</span>', cls: 'text-sm text-slate-500'
            },
            { html: a.score != null ? a.score : '—', cls: 'text-sm font-semibold text-slate-700' },
            { html: AdminUI.rowActions([AdminUI.actionButton(L["Homework_Review"], `reviewAssignment(${a.studentId})`, { tone: 'primary' })]), cls: 'text-right' }
        ]), 7, { empty: L["Homework_NoSubmissions"] });
    }

    window.reviewAssignment = function (studentId) {
        const a = assignments.find(x => x.studentId === studentId);
        if (!a) return;

        document.getElementById('reviewStudentId').value = a.studentId;
        document.getElementById('reviewTitle').textContent = `${L["Homework_Review"]} — ${a.fullName}`;
        document.getElementById('reviewStudent').textContent = a.fullName;
        document.getElementById('reviewMeta').textContent = `${a.email} · ${statusLabel(a.status)}${a.isLate ? ` · ${L["Homework_Late"]}` : ''}`;

        document.getElementById('reviewText').textContent = a.text || '—';

        const link = document.getElementById('reviewLink');
        if (a.link) { link.href = a.link; link.textContent = a.link; link.classList.remove('hidden'); }
        else { link.removeAttribute('href'); link.textContent = ''; link.classList.add('hidden'); }

        document.getElementById('reviewFiles').innerHTML = a.files.length
            ? a.files.map(f => `<a href="${AdminUI.escapeHtml(f.url)}" target="_blank" rel="noopener" class="rounded-lg border border-border px-2.5 py-1 font-semibold text-primary hover:bg-primary-light">${AdminUI.escapeHtml(f.fileName)}</a>`).join('')
            : `<span class="text-slate-400">${L["Homework_NoFiles"]}</span>`;

        document.getElementById('reviewQuizzes').innerHTML = a.quizzes.length
            ? a.quizzes.map(q => `<div>${AdminUI.escapeHtml(q.title)} — ${q.completed ? `${q.correctCount}/${q.questionCount} · ${q.scorePercent}%` : L["Homework_NotCompleted"]}</div>`).join('')
            : `<span class="text-slate-400">—</span>`;

        document.getElementById('reviewScore').value = a.score ?? '';
        document.getElementById('reviewFeedback').value = a.feedback || '';
        document.getElementById('reviewError').classList.add('hidden');

        AdminUI.openModal('reviewModal');
    };

    window.gradeHomework = async function (returnToStudent) {
        const studentId = Number(document.getElementById('reviewStudentId').value);
        const errorBox = document.getElementById('reviewError');
        errorBox.classList.add('hidden');

        const score = parseFloat(document.getElementById('reviewScore').value);
        if (Number.isNaN(score)) {
            errorBox.textContent = L["Common_RequiredFields"];
            errorBox.classList.remove('hidden');
            return;
        }

        const res = await AdminUI.putJson(`/homework/${homeworkId}/assignments/${studentId}/grade`, {
            score,
            feedback: document.getElementById('reviewFeedback').value,
            returnToStudent
        });

        if (!res.ok) {
            errorBox.textContent = AdminUI.errText(res);
            errorBox.classList.remove('hidden');
            return;
        }

        AdminUI.notify(L["Js_Saved"]);
        AdminUI.closeModal('reviewModal');
        load();
    };

    document.addEventListener('DOMContentLoaded', load);

    let _timer = null;
    document.getElementById('search').addEventListener('input', () => {
        clearTimeout(_timer);
        _timer = setTimeout(load, 350);
    });
    document.getElementById('filter_status').addEventListener('change', load);
})();
