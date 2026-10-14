async function load() {
    const groupId = document.getElementById('group_id').value;
    if (!groupId) { AdminUI.error('Qrup seçin'); return; }

    const quizType = document.getElementById('quiz_type').value;
    document.getElementById('exportLink').href = `/group-stat/export?group_id=${groupId}&quiz_type=${quizType}`;

    AdminUI.loader(true);
    const data = await AdminUI.get(`/group-stat/data?group_id=${groupId}&quiz_type=${quizType}`);
    AdminUI.loader(false);

    const summary = document.getElementById('summary');
    summary.classList.remove('hidden');
    summary.classList.add('grid');
    summary.innerHTML = `
        <div class="rounded-xl border border-slate-200 bg-white p-5"><div class="text-sm text-slate-500">İmtahan sayı</div><div class="text-2xl font-bold text-primary">${data.quizCount}</div></div>
        <div class="rounded-xl border border-slate-200 bg-white p-5"><div class="text-sm text-slate-500">Ümumi sual</div><div class="text-2xl font-bold text-primary">${data.totalQuestions}</div></div>
        <div class="rounded-xl border border-slate-200 bg-white p-5"><div class="text-sm text-slate-500">Şagird sayı</div><div class="text-2xl font-bold text-primary">${data.students.length}</div></div>`;

    AdminUI.table('rows', data.students.map(s => [
        { html: `<div class="font-medium text-slate-800">${s.fullName}</div><div class="text-xs text-slate-400">${s.email}</div>` },
        { html: `${s.attemptedQuizCount} / ${s.quizCount}`, cls: 'text-slate-500' },
        { html: s.completedQuestions, cls: 'text-slate-500' },
        { html: `<span class="text-emerald-600 font-medium">${s.correct}</span>` },
        { html: `<span class="text-red-600 font-medium">${s.wrong}</span>` },
        { html: `<span class="font-semibold ${s.scorePercent >= 50 ? 'text-emerald-600' : 'text-red-600'}">${s.scorePercent}%</span>` }
    ]), 6);
}

document.addEventListener('DOMContentLoaded', load);
