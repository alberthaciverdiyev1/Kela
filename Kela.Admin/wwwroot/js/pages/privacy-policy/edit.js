(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');


    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Privacy_Create"];
        document.getElementById('type').value = '';
        document.getElementById('type').disabled = false;
        document.getElementById('content').value = '';
        AdminUI.openModal('modal');
    };

    window.openEdit = function (type) {
        const p = policyData.find(x => x.type === type);
        document.getElementById('modalTitle').textContent = L["Js_EditPrefix"] + type;
        document.getElementById('type').value = type;
        document.getElementById('type').disabled = true;
        document.getElementById('content').value = p ? p.content : '';
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const fd = new FormData();
        fd.append('type', document.getElementById('type').value);
        fd.append('content', document.getElementById('content').value);

        const res = await AdminUI.putForm('/privacy-policy', fd);
        if (res.ok) { AdminUI.notify(L["Js_Saved"]); AdminUI.closeModal('modal'); loadList(); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
