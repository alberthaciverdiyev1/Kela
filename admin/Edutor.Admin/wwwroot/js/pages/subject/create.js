(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');
    const groupSelect = document.getElementById('group_id');
    const newGroupField = document.getElementById('newGroupField');
    const newGroupInput = document.getElementById('new_group_name');

    // "Yeni qrup yarat" seçiləndə ad sahəsi görünür; seçim könüllüdür.
    function syncGroupField() {
        const isNew = groupSelect?.value === 'new';
        newGroupField?.classList.toggle('hidden', !isNew);
        if (!isNew && newGroupInput) newGroupInput.value = '';
    }

    groupSelect?.addEventListener('change', syncGroupField);

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Subject_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('name').value = '';
        const sharedField = document.getElementById('is_shared');
        if (sharedField) sharedField.checked = false;
        if (groupSelect) groupSelect.value = '';
        syncGroupField();
        AdminUI.openModal('modal');
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js idarə edir

        const payload = {
            name: document.getElementById('name').value,
            is_shared: document.getElementById('is_shared')?.checked ? 'true' : 'false'
        };

        const groupId = groupSelect?.value || '';
        if (groupId === 'new') {
            const name = newGroupInput?.value.trim();
            if (name) payload.new_group_name = name;
        } else if (groupId) {
            payload.group_id = groupId;
        }

        const res = await AdminUI.post('/subject', payload);
        if (res.ok) { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
