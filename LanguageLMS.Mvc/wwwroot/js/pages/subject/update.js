(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    window.openEdit = async function (id) {
        const data = await AdminUI.get(`/subject/${id}`);
        document.getElementById('modalTitle').textContent = 'Fənn redaktəsi';
        document.getElementById('entityId').value = data.id;
        document.getElementById('name').value = data.name;
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js idarə edir

        const fd = new FormData();
        fd.append('name', document.getElementById('name').value);

        const res = await AdminUI.putForm(`/subject/${id}`, fd);
        if (res.ok) { AdminUI.notify('Redaktə olundu'); window.closeModal(); loadList(currentPage); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
