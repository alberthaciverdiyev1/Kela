(function () {
    window.openGenerate = function () {
        document.getElementById('generateModal').classList.remove('hidden');
        document.getElementById('generateModal').classList.add('flex');
    };
    window.closeGenerate = function () {
        document.getElementById('generateModal').classList.add('hidden');
        document.getElementById('generateModal').classList.remove('flex');
    };

    document.getElementById('generateForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const res = await AdminUI.postJson('/students/generate', {
            count: Number(document.getElementById('gen_count').value),
            fullNamePrefix: document.getElementById('gen_prefix').value,
            emailDomain: document.getElementById('gen_domain').value,
            password: document.getElementById('gen_password').value,
            sendCredentials: document.getElementById('gen_send').checked
        });

        if (res.ok) { AdminUI.notify(res.data.created + L["Js_StudentsCreated"]); window.closeGenerate(); loadList(1); }
        else AdminUI.error();
    });
})();
