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
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js idarə edir

        const fd = new FormData();
        fd.append('title', document.getElementById('title').value);
        fd.append('removeImage', document.getElementById('image_remove').value);
        const file = document.getElementById('image').files[0];
        if (file) fd.append('Image', file);

        const res = await AdminUI.putForm(`/banner/${id}`, fd);
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); window.closeModal(); loadList(currentPage); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
