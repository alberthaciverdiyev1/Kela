(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');


    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Subject_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('name').value = '';
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js idarə edir

        const res = await AdminUI.post('/subject', { name: document.getElementById('name').value });
        if (res.ok) { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
