(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.selectTemplate = function (key) {
        const hidden = document.getElementById('templateKey');
        if (hidden) hidden.value = key;
        document.querySelectorAll('#templateGrid [data-key]').forEach(el => {
            const on = el.dataset.key === key;
            el.classList.toggle('border-primary', on);
            el.classList.toggle('border-slate-200', !on);
        });
    };

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
        window.selectTemplate(document.querySelector('#templateGrid [data-key]').dataset.key);
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
