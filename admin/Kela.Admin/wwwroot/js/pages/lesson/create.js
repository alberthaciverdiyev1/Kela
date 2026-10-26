(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');


    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Lesson_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('sort_order').value = '';
        // Səhifədə fənn filtri seçilibsə (URL-dəki subject_id də daxil) yeni dərs
        // həmin fənnə görə yaradılsın.
        document.getElementById('subject_id').value = document.getElementById('filter_subject')?.value || '';
        document.getElementById('title').value = '';
        document.getElementById('is_active').checked = true;
        const sharedToggle = document.getElementById('is_shared');
        if (sharedToggle) sharedToggle.checked = false;
        window.LESSON_CURRENT_SHARED = false;
        document.getElementById('add_to_groups').checked = false;
        document.getElementById('addToGroupsBlock').classList.remove('hidden');
        // Əvvəlki redaktədən qalan şəkil önizləməsi modaldan silinməlidir.
        AdminUI.fileReset('image');
        AdminUI.openModal('modal');
    };

    window.buildLessonForm = function () {
        const fd = new FormData();
        fd.append('subject_id', document.getElementById('subject_id').value);
        fd.append('title', document.getElementById('title').value);
        const sortOrder = document.getElementById('sort_order').value;
        if (sortOrder !== '') fd.append('sort_order', sortOrder);
        fd.append('is_active', document.getElementById('is_active').checked ? 'true' : 'false');
        const sharedField = document.getElementById('is_shared');
        const isShared = sharedField ? sharedField.checked : window.LESSON_CURRENT_SHARED === true;
        fd.append('is_shared', isShared ? 'true' : 'false');
        fd.append('add_to_groups', document.getElementById('add_to_groups').checked ? 'true' : 'false');
        fd.append('RemoveImage', document.getElementById('image_remove').value === 'true' ? 'true' : 'false');
        const file = document.getElementById('image').files[0];
        if (file) fd.append('Image', file);
        return fd;
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js idarə edir

        const res = await AdminUI.postForm('/lesson', window.buildLessonForm());
        if (res.ok) { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
