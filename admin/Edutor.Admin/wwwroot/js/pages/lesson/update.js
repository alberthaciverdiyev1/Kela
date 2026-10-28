(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/lesson/${id}`);
        document.getElementById('modalTitle').textContent = L["Js_EditLesson"];
        document.getElementById('entityId').value = data.id;
        document.getElementById('sort_order').value = data.sortOrder ?? 0;
        document.getElementById('subject_id').value = data.subjectId;
        document.getElementById('title').value = data.title;
        document.getElementById('is_active').checked = data.isActive;
        window.LESSON_CURRENT_SHARED = data.isShared === true;
        const sharedToggle = document.getElementById('is_shared');
        if (sharedToggle) sharedToggle.checked = window.LESSON_CURRENT_SHARED;

        // "Bütün qruplara əlavə et" yalnız yaratma zamanı tətbiq olunur.
        document.getElementById('add_to_groups').checked = false;
        document.getElementById('addToGroupsBlock').classList.add('hidden');

        // Paylaşımı yalnız dərsin sahibi və ya admin dəyişir; digər müəllim
        // yalnız statusu dəyişə bilər (o da siyahıdaki açardan).
        const canShare = window.ADMIN && (window.ADMIN.isAdmin || data.userId === window.ADMIN.userId);
        if (sharedToggle) sharedToggle.disabled = !canShare;
        AdminUI.fileSet('image', data.image);
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js idarə edir

        const res = await AdminUI.putForm(`/lesson/${id}`, window.buildLessonForm());
        if (res.ok) { AdminUI.notify(L["Js_Updated"]); AdminUI.closeModal('modal'); loadList(pager.page); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
