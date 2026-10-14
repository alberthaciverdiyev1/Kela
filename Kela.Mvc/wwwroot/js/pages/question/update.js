(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');
    const VARIANTS = ['a', 'b', 'c', 'd', 'e'];

    window.openEdit = async function (id) {
        const d = await AdminUI.get(`/question/${id}`);
        document.getElementById('modalTitle').textContent = 'Sual redaktəsi';
        document.getElementById('entityId').value = d.id;
        document.getElementById('subject_id').value = d.subjectId;
        document.getElementById('title_type').value = d.titleType;
        document.getElementById('variant_type').value = d.variantType;

        document.getElementById('rawTitle').value = d.title ?? '';
        document.getElementById('title').value = d.titleType === 'text' ? (d.title ?? '') : '';

        const map = { a: d.variantA, b: d.variantB, c: d.variantC, d: d.variantD, e: d.variantE };
        VARIANTS.forEach(v => {
            document.getElementById('raw' + v.toUpperCase()).value = map[v] ?? '';
            document.getElementById('variant_' + v).value = d.variantType === 'text' ? (map[v] ?? '') : '';
        });

        if (d.titleType === 'image' && d.titleUrl) AdminUI.fileSet('title_image', d.titleUrl);
        const urlMap = { a: d.variantAUrl, b: d.variantBUrl, c: d.variantCUrl, d: d.variantDUrl, e: d.variantEUrl };
        if (d.variantType === 'image') VARIANTS.forEach(v => { if (urlMap[v]) AdminUI.fileSet('variant_' + v + '_image', urlMap[v]); });

        document.getElementById('correct_answer').value = d.correctAnswer;
        window.toggleBlocks();
        window.showModal();
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('entityId').value;
        if (!id) return; // create.js

        const res = await AdminUI.putForm(`/question/${id}`, window.buildQuestionForm());
        if (res.ok) { AdminUI.notify('Redaktə olundu'); window.closeModal(); loadList(currentPage); return; }

        errBox.textContent = res.data?.errors ? Object.values(res.data.errors).flat().join(' • ') : 'Xəta baş verdi';
        errBox.classList.remove('hidden');
    });
})();
