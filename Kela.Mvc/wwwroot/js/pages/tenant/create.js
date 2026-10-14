(function () {
    window.showModal = function () {
        document.getElementById('modal').classList.remove('hidden');
        document.getElementById('modal').classList.add('flex');
    };
    window.closeModal = function () {
        document.getElementById('modal').classList.add('hidden');
        document.getElementById('modal').classList.remove('flex');
    };

    window.openCreate = function () {
        document.getElementById('name').value = '';
        document.getElementById('slug').value = '';
        window.showModal();
    };

    document.getElementById('entityForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const res = await AdminUI.postJson('/tenant', {
            name: document.getElementById('name').value,
            slug: document.getElementById('slug').value
        });
        if (res.ok) { AdminUI.notify(L["Js_Created"]); window.closeModal(); loadList(); }
        else AdminUI.error(AdminUI.errText(res));
    });
})();
