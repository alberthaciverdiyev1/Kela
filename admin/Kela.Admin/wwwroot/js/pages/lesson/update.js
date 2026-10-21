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
        document.getElementById('is_shared').checked = data.isShared === true;
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
