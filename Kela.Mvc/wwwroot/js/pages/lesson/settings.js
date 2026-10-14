let settingsLessonId = null;

const SETTINGS_FIELDS = [
    ['topicExplain', 's_topic_explain'],
    ['liveVideo', 's_live_video'],
    ['pdf', 's_pdf'],
    ['voiceRecord', 's_voice_record'],
    ['topicQuiz', 's_topic_quiz'],
    ['generalQuiz', 's_general_quiz']
];

async function openSettings(id) {
    settingsLessonId = id;
    const [lesson, settings] = await Promise.all([
        AdminUI.get(`/lesson/${id}`),
        AdminUI.get(`/lesson/${id}/settings`)
    ]);
    document.getElementById('settingsTitle').textContent = (lesson.title || '') + ' — məzmun açarları';
    document.getElementById('settingsError').classList.add('hidden');
    SETTINGS_FIELDS.forEach(([key, elId]) => {
        document.getElementById(elId).checked = !!settings[key];
    });
    document.getElementById('settingsModal').classList.remove('hidden');
    document.getElementById('settingsModal').classList.add('flex');
}

function closeSettings() {
    document.getElementById('settingsModal').classList.add('hidden');
    document.getElementById('settingsModal').classList.remove('flex');
}

async function saveSettings() {
    const payload = {};
    SETTINGS_FIELDS.forEach(([key, elId]) => {
        payload[key] = document.getElementById(elId).checked ? 1 : 0;
    });
    const res = await AdminUI.putJson(`/lesson/${settingsLessonId}/settings`, payload);
    if (res.ok) { AdminUI.notify('Yadda saxlanıldı'); closeSettings(); return; }
    const box = document.getElementById('settingsError');
    box.textContent = AdminUI.errText(res);
    box.classList.remove('hidden');
}
