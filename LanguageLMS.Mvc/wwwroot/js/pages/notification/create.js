(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');

    function toggleStudents() {
        document.getElementById('studentsBlock').classList.toggle('hidden', document.getElementById('all').checked);
    }

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
        document.getElementById('title').value = '';
        document.getElementById('body').value = '';
        document.getElementById('all').checked = true;
        toggleStudents();
        window.showModal();
    };

    document.getElementById('all').addEventListener('change', toggleStudents);

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const all = document.getElementById('all').checked;
        const fd = new FormData();
        fd.append('title', document.getElementById('title').value);
        fd.append('body', document.getElementById('body').value || '');
        fd.append('all', all ? 'true' : 'false');
        const file = document.getElementById('image').files[0];
        if (file) fd.append('Image', file);

        if (!all) {
            const selected = [...document.getElementById('student_ids').selectedOptions];
            selected.forEach((o, i) => fd.append(`studentIds[${i}]`, o.value));
        }

        const res = await AdminUI.postForm('/notification', fd);
        if (res.ok) { AdminUI.notify('Göndərildi'); window.closeModal(); loadList(1); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
