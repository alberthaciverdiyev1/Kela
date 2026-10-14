(function () {
    const errBox = document.getElementById('formError');

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
        window.showModal();
    };

    document.getElementById('createForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const res = await AdminUI.postJson('/permission', { name: document.getElementById('name').value });
        if (res.ok) { AdminUI.notify('Yaradıldı'); window.closeModal(); loadList(); }
        else AdminUI.error(AdminUI.errText(res));
    });
})();
