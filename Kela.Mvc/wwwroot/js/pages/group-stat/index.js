async function load() {
    const groupId = document.getElementById('group_id').value;
    if (!groupId) { AdminUI.error(L["Common_SelectGroup"]); return; }

    const quizType = document.getElementById('quiz_type').value;
    document.getElementById('exportLink').href = `/group-stat/export?group_id=${groupId}&quiz_type=${quizType}`;

    AdminUI.loader(true);
    const data = await AdminUI.get(`/group-stat/data?group_id=${groupId}&quiz_type=${quizType}`);
    AdminUI.loader(false);

    const summary = document.getElementById('summary');
    summary.classList.remove('hidden');
    summary.classList.add('grid');
    summary.innerHTML = `
        <div class="rounded-xl border border-slate-200 bg-white p-5"><div class="text-sm text-slate-500">${L["Js_QuizCount"]}</div><div class="text-2xl font-bold text-primary">${data.quizCount}</div></div>
        <div class="rounded-xl border border-slate-200 bg-white p-5"><div class="text-sm text-slate-500">${L["Js_TotalQuestions"]}</div><div class="text-2xl font-bold text-primary">${data.totalQuestions}</div></div>
        <div class="rounded-xl border border-slate-200 bg-white p-5"><div class="text-sm text-slate-500">${L["Js_StudentCount"]}</div><div class="text-2xl font-bold text-primary">${data.students.length}</div></div>`;

    const q = (document.getElementById('search')?.value ?? '').trim().toLowerCase();
    const students = q ? data.students.filter(s => (s.fullName + ' ' + s.email).toLowerCase().includes(q)) : data.students;

    AdminUI.setInfo(students.length);
    AdminUI.table('rows', students.map((s, index) => [
        { html: AdminUI.rowNo(s, index), cls: 'text-slate-400' },
        { html: `<div class="font-medium text-slate-800">${s.fullName}</div><div class="text-xs text-slate-400">${s.email}</div>` },
        { html: `${s.attemptedQuizCount} / ${s.quizCount}`, cls: 'text-slate-500' },
        { html: s.completedQuestions, cls: 'text-slate-500' },
        { html: AdminUI.badge(s.correct, 'success') },
        { html: AdminUI.badge(s.wrong, 'danger') },
        { html: AdminUI.badge(s.scorePercent + '%', s.scorePercent >= 50 ? 'success' : 'danger') }
    ]), 6);
}

document.addEventListener('DOMContentLoaded', load);
