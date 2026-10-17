(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');


    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Banner_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('title').value = '';
        document.getElementById('image').value = '';
        document.getElementById('image_preview').classList.add('hidden');
        document.getElementById('image_x').classList.add('hidden');
        document.getElementById('image_remove').value = 'false';
        document.getElementById('is_active').checked = true;
        document.getElementById('active_until').value = '';
        AdminUI.openModal('modal');
    };

    window.buildBannerForm = function () {
        const fd = new FormData();
        fd.append('title', document.getElementById('title').value);
        fd.append('is_active', document.getElementById('is_active').checked ? 'true' : 'false');
        const until = document.getElementById('active_until').value;
        if (until) fd.append('active_until', until);
        const file = document.getElementById('image').files[0];
        if (file) fd.append('Image', file);
        return fd;
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js idarə edir

        const res = await AdminUI.postForm('/banner', window.buildBannerForm());
        if (res.ok) { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
