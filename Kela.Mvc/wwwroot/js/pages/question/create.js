(function () {
    const form = document.getElementById('entityForm');
    const errBox = document.getElementById('formError');
    const VARIANTS = ['a', 'b', 'c', 'd', 'e'];


    window.toggleBlocks = function () {
        const titleType = document.getElementById('title_type').value;
        document.getElementById('titleTextBlock').classList.toggle('hidden', titleType === 'image');
        document.getElementById('titleImageBlock').classList.toggle('hidden', titleType !== 'image');
        // Başlıq sahəsi rejimə görə zəruridir: şəkil rejimində gizli qalan başlıq
        // yoxlanılmasın (əks halda forma saxlana bilmir).
        document.getElementById('title').setAttribute('data-required', titleType === 'text' ? 'true' : 'false');

        const variantType = document.getElementById('variant_type').value;
        document.getElementById('variantsTextBlock').classList.toggle('hidden', variantType === 'image');
        document.getElementById('variantsImageBlock').classList.toggle('hidden', variantType !== 'image');
        // Server A–D variantlarını yalnız mətn rejimində tələb edir (E sərbəstdir).
        ['a', 'b', 'c', 'd'].forEach(v => {
            const el = document.getElementById('variant_' + v);
            if (el) el.setAttribute('data-required', variantType === 'text' ? 'true' : 'false');
        });
    };

    window.buildQuestionForm = function () {
        const fd = new FormData();
        const titleType = document.getElementById('title_type').value;
        const variantType = document.getElementById('variant_type').value;

        fd.append('subject_id', document.getElementById('subject_id').value);
        fd.append('title_type', titleType);
        fd.append('variant_type', variantType);
        fd.append('correct_answer', document.getElementById('correct_answer').value);

        if (titleType === 'text') {
            fd.append('title', document.getElementById('title').value);
        } else {
            const f = document.getElementById('title_image').files[0];
            if (f) fd.append('title_image', f);
            else if (document.getElementById('title_image_remove').value === 'true') fd.append('title', '');
            else fd.append('title', document.getElementById('rawTitle').value);
        }

        VARIANTS.forEach(v => {
            if (variantType === 'text') {
                fd.append('variant_' + v, document.getElementById('variant_' + v).value);
            } else {
                const f = document.getElementById('variant_' + v + '_image').files[0];
                if (f) fd.append('variant_' + v + '_image', f);
                else if (document.getElementById('variant_' + v + '_image_remove').value === 'true') fd.append('variant_' + v, '');
                else fd.append('variant_' + v, document.getElementById('raw' + v.toUpperCase()).value);
            }
        });
        return fd;
    };

    window.openCreate = function () {
        document.getElementById('modalTitle').textContent = L["Question_Create"];
        document.getElementById('entityId').value = '';
        document.getElementById('subject_id').value = '';
        document.getElementById('title_type').value = 'text';
        document.getElementById('variant_type').value = 'text';
        document.getElementById('title').value = '';
        VARIANTS.forEach(v => document.getElementById('variant_' + v).value = '');
        document.getElementById('correct_answer').value = 'A';
        document.getElementById('rawTitle').value = '';
        ['A', 'B', 'C', 'D', 'E'].forEach(v => document.getElementById('raw' + v).value = '');
        window.toggleBlocks();
        AdminUI.openModal('modal');
    };

    document.getElementById('title_type').addEventListener('change', window.toggleBlocks);
    document.getElementById('variant_type').addEventListener('change', window.toggleBlocks);

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (document.getElementById('entityId').value) return; // update.js

        const res = await AdminUI.postForm('/question', window.buildQuestionForm());
        if (res.ok) { AdminUI.notify(L["Js_Added"]); AdminUI.closeModal('modal'); loadList(1); return; }

        errBox.textContent = AdminUI.errText(res);
        errBox.classList.remove('hidden');
    });
})();
