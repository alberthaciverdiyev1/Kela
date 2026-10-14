(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.showModal = function () {
        document.getElementById('modal').classList.remove('hidden');
        document.getElementById('modal').classList.add('flex');
    };
    window.closeModal = function () {
        document.getElementById('modal').classList.add('hidden');
        document.getElementById('modal').classList.remove('flex');
        errBox.classList.add('hidden');
    };

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = 'Yeni sənəd';
        document.getElementById('type').value = '';
        document.getElementById('type').disabled = false;
        document.getElementById('content').value = '';
        window.showModal();
    };

    window.openEdit = function (type) {
        const p = policyData.find(x => x.type === type);
        document.getElementById('modalTitle').textContent = 'Redaktə: ' + type;
        document.getElementById('type').value = type;
        document.getElementById('type').disabled = true;
        document.getElementById('content').value = p ? p.content : '';
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const fd = new FormData();
        fd.append('type', document.getElementById('type').value);
        fd.append('content', document.getElementById('content').value);

        const res = await AdminUI.putForm('/privacy-policy', fd);
        if (res.ok) { AdminUI.notify('Yadda saxlanıldı'); window.closeModal(); loadList(); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
