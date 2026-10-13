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
        document.getElementById('modalTitle').textContent = 'Yeni sertifikat';
        document.getElementById('entityId').value = '';
        document.getElementById('title').value = '';
        document.getElementById('templateKey').value = document.getElementById('templateKey').options[0].value;
        document.getElementById('bodyText').value = '';
        document.getElementById('signature').value = '';
        document.getElementById('signature_preview').classList.add('hidden');
        document.getElementById('signature_x').classList.add('hidden');
        document.getElementById('signature_remove').value = 'false';
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return;

        const fd = new FormData();
        fd.append('title', document.getElementById('title').value);
        fd.append('templateKey', document.getElementById('templateKey').value);
        fd.append('bodyText', document.getElementById('bodyText').value);
        const file = document.getElementById('signature').files[0];
        if (file) fd.append('Signature', file);

        const res = await AdminUI.postForm('/certificate', fd);
        if (res.ok) { AdminUI.notify('Əlavə olundu'); window.closeModal(); loadList(1); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
