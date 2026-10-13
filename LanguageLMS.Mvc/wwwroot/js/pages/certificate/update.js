(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/certificate/${id}`);
        document.getElementById('modalTitle').textContent = 'Sertifikat redaktəsi';
        document.getElementById('entityId').value = data.id;
        document.getElementById('title').value = data.title;
        document.getElementById('templateKey').value = data.templateKey;
        document.getElementById('bodyText').value = data.bodyText;
        document.getElementById('signature').value = '';
        document.getElementById('signature_remove').value = 'false';
        document.getElementById('signature_preview').classList.add('hidden');
        document.getElementById('signature_x').classList.add('hidden');
        if (data.signature) AdminUI.fileSet('signature', data.signature);
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return;

        const fd = new FormData();
        fd.append('title', document.getElementById('title').value);
        fd.append('templateKey', document.getElementById('templateKey').value);
        fd.append('bodyText', document.getElementById('bodyText').value);
        fd.append('removeSignature', document.getElementById('signature_remove').value);
        const file = document.getElementById('signature').files[0];
        if (file) fd.append('Signature', file);

        const res = await AdminUI.putForm(`/certificate/${id}`, fd);
        if (res.ok) { AdminUI.notify('Redaktə olundu'); window.closeModal(); loadList(currentPage); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
