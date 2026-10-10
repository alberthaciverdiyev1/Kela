(function () {
    const modal = document.getElementById('modal');
    const form = document.getElementById('subjectForm');
    const errBox = document.getElementById('formError');

    function showModal() { modal.classList.remove('hidden'); modal.classList.add('flex'); }
    function closeModal() { modal.classList.add('hidden'); modal.classList.remove('flex'); errBox.classList.add('hidden'); }

    function openCreate() {
        document.getElementById('modalTitle').textContent = 'Yeni fənn';
        document.getElementById('subjectId').value = '';
        document.getElementById('name').value = '';
        showModal();
    }

    async function openEdit(id) {
        const data = await AdminUI.get(`/Subject/Get?id=${id}`);
        document.getElementById('modalTitle').textContent = 'Fənn redaktəsi';
        document.getElementById('subjectId').value = data.id;
        document.getElementById('name').value = data.name;
        showModal();
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('subjectId').value;
        const url = id ? `/Subject/Update?id=${id}` : '/Subject/Create';
        const res = await AdminUI.post(url, { name: document.getElementById('name').value });

        if (res.ok) { AdminUI.notify(id ? 'Redaktə olundu' : 'Əlavə olundu'); AdminUI.reloadAfter(); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });

    window.openCreate = openCreate;
    window.openEdit = openEdit;
    window.closeModal = closeModal;
})();
