(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/banner/${id}`);
        document.getElementById('modalTitle').textContent = L["Js_EditBanner"];
        document.getElementById('entityId').value = data.id;
        document.getElementById('title').value = data.title;
        document.getElementById('image').value = '';
        document.getElementById('image_remove').value = 'false';
        document.getElementById('image_preview').classList.add('hidden');
        document.getElementById('image_x').classList.add('hidden');
        if (data.image) AdminUI.fileSet('image', data.image);
        document.getElementById('is_active').checked = data.isActive;
        document.getElementById('active_until').value = data.activeUntil ? String(data.activeUntil).slice(0, 10) : '';
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js idarə edir

        const fd = window.buildBannerForm();
        fd.append('removeImage', document.getElementById('image_remove').value);

        const res = await AdminUI.putForm(`/banner/${id}`, fd);
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); AdminUI.closeModal('modal'); loadList(currentPage); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
