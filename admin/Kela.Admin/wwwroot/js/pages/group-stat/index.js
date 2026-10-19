async function load() {
    const groupId = document.getElementById('group_id').value;
    if (!groupId) { AdminUI.error(L["Common_SelectGroup"]); return; }

    const quizType = document.getElementById('quiz_type').value;
    AdminUI.loader(true);
    const data = await AdminUI.get(`/group-stat/data?group_id=${groupId}&quiz_type=${quizType}`);
    AdminUI.loader(false);

    const summary = document.getElementById('summary');
    summary.classList.remove('hidden');
    summary.classList.add('grid');
    summary.innerHTML = [
        AdminUI.statCard(L["Js_QuizCount"], data.quizCount, 'text-primary'),
        AdminUI.statCard(L["Js_TotalQuestions"], data.totalQuestions, 'text-primary'),
        AdminUI.statCard(L["Js_StudentCount"], data.students.length, 'text-primary')
    ].join('');

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
