(function () {

    window.openCreate = function () {
        document.getElementById('name').value = '';
        document.getElementById('slug').value = '';
        AdminUI.openModal('modal');
    };

    document.getElementById('entityForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const res = await AdminUI.postJson('/tenant', {
            name: document.getElementById('name').value,
            slug: document.getElementById('slug').value
        });
        if (res.ok) { AdminUI.notify(L["Js_Created"]); AdminUI.closeModal('modal'); loadList(); }
        else AdminUI.error(AdminUI.errText(res));
    });
})();
