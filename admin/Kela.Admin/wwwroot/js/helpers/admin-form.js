// Kela admin panel — form
// Parçalanmışdır: hər fayl eyni AdminUI obyektini genişləndirir.
window.AdminUI = window.AdminUI || {};

(function (UI) {
    function isVideoUrl(url) { return /\.(mp4|mov|m4v|webm|m3u8|mkv|avi)(\?|$)/i.test(url || ''); }

    function setPreview(id, url, video) {
        const img = document.getElementById(id + '_preview');
        const vid = document.getElementById(id + '_preview_video');
        if (!img && !vid) return;

        if (video && vid) {
            vid.src = url;
            vid.classList.remove('hidden');
            if (img) { img.classList.add('hidden'); img.removeAttribute('src'); }
        } else if (img) {
            img.src = url;
            img.classList.remove('hidden');
            if (vid) { vid.classList.add('hidden'); vid.removeAttribute('src'); }
        }

        document.getElementById(id + '_x').classList.remove('hidden');
        UI.toggleHidden(id + '_upload', true);   // fayl varsa yükləmə sahəsi görünməsin
        const nameEl = document.getElementById(id + 'Name');
        if (nameEl) nameEl.textContent = '';
        document.getElementById(id + '_remove').value = 'false';
    }

    function filePreview(id) {
        const input = document.getElementById(id);
        if (!input || !input.files.length) return;

        const file = input.files[0];
        setPreview(id, URL.createObjectURL(file), (file.type || '').startsWith('video/'));
    }

    function fileSet(id, url) { if (url) setPreview(id, url, isVideoUrl(url)); }

    function fileRemove(id) {
        const img = document.getElementById(id + '_preview');
        const vid = document.getElementById(id + '_preview_video');
        if (img) { img.classList.add('hidden'); img.removeAttribute('src'); }
        if (vid) { vid.classList.add('hidden'); vid.removeAttribute('src'); }
        document.getElementById(id + '_x').classList.add('hidden');
        UI.toggleHidden(id + '_upload', false);
        document.getElementById(id + '_remove').value = 'true';
        const input = document.getElementById(id);
        if (input) input.value = '';
    }

    function fileReset(id) {
        const img = document.getElementById(id + '_preview');
        const vid = document.getElementById(id + '_preview_video');

        if (img) { img.classList.add('hidden'); img.removeAttribute('src'); }
        if (vid) {
            try { vid.pause(); } catch (e) { /* əhəmiyyətsiz */ }
            vid.classList.add('hidden');
            vid.removeAttribute('src');
        }

        const x = document.getElementById(id + '_x');
        if (x) x.classList.add('hidden');

        UI.toggleHidden(id + '_upload', false);   // fayl götürüldü — yükləmə sahəsi geri gəlir

        const rm = document.getElementById(id + '_remove');
        if (rm) rm.value = 'false';

        const input = document.getElementById(id);
        if (input) input.value = '';

        const nameEl = document.getElementById(id + 'Name');
        if (nameEl) nameEl.textContent = nameEl.dataset.hint || '';
    }

    function initEditors(root) {
        (root || document).querySelectorAll('.admin-editor[data-for]').forEach(editor => {
            if (editor.dataset.ready === '1') return;
            editor.dataset.ready = '1';

            const field = document.getElementById(editor.dataset.for);
            const body = editor.querySelector('.admin-editor-body');
            if (!field || !body) return;

            const sync = () => { field.value = body.innerHTML.trim(); };
            body.addEventListener('input', sync);
            body.addEventListener('blur', sync);

            editor.querySelectorAll('[data-cmd]').forEach(btn => {
                // Klik zamanı seçim itməsin.
                btn.addEventListener('mousedown', (e) => e.preventDefault());
                btn.addEventListener('click', async () => {
                    const cmd = btn.dataset.cmd;
                    body.focus();
                    if (cmd === 'createLink') {
                        const url = await UI.swal({ input: 'url', title: 'Keçid', inputPlaceholder: 'https://' });
                        if (url.value) document.execCommand('createLink', false, url.value);
                    } else {
                        document.execCommand(cmd, false, btn.dataset.value || null);
                    }
                    sync();
                });
            });
        });
    }

    function syncEditors(root) {
        (root || document).querySelectorAll('.admin-editor[data-for]').forEach(editor => {
            const field = document.getElementById(editor.dataset.for);
            const body = editor.querySelector('.admin-editor-body');
            if (field && body) body.innerHTML = field.value || '';
        });
    }

    function closeSelects() {
        document.querySelectorAll('.admin-select.is-open').forEach(w => {
            w.classList.remove('is-open');
            w.querySelector('.admin-select-menu')?.classList.add('hidden');
        });
        openSelectWrap = null;
    }

    function enhanceSelect(select) {
        if (!select || select.dataset.enhanced === '1' || select.dataset.native === '1') return;
        select.dataset.enhanced = '1';

        const multiple = !!select.multiple;

        // Boş seçim üçün disabled placeholder — istifadəçi təsadüfən ilk variantı
        // seçməsin və heç bir seçim olmadığı görünsün. Əvvəlcədən seçilmiş dəyər
        // varsa (məs. cari tenant) placeholder əlavə olunmur.
        if (!multiple && !select.dataset.selected && !select.querySelector('option[value=""]')) {
            const placeholder = document.createElement('option');
            placeholder.value = '';
            placeholder.textContent = L["Common_ChooseOne"] || '—';
            placeholder.disabled = true;
            placeholder.selected = true;
            select.insertBefore(placeholder, select.firstChild);
        }

        if (multiple && !select.dataset.placeholder) {
            select.dataset.placeholder = L["Common_ChooseOne"] || '';
        }

        const wrap = document.createElement('div');
        wrap.className = 'admin-select';
        wrap.dataset.for = select.id;

        const trigger = document.createElement('button');
        trigger.type = 'button';
        trigger.className = 'admin-select-trigger';
        wrap.appendChild(trigger);

        const menu = document.createElement('div');
        menu.className = 'admin-select-menu hidden';
        menu.innerHTML = `
            <div class="admin-select-search">
                <input type="search" class="admin-field" placeholder="${L["Common_SearchPlaceholder"] || ''}" />
            </div>
            <div class="admin-select-options"></div>`;
        wrap.appendChild(menu);

        select.classList.add('hidden');
        select.parentNode.insertBefore(wrap, select.nextSibling);

        const search = menu.querySelector('input');
        const searchBox = menu.querySelector('.admin-select-search');
        const list = menu.querySelector('.admin-select-options');

        // Axtarış yalnız çox seçim olanda məna daşıyır (10-dan çox real variant).
        const withSearch = () => [...select.options].filter(o => !o.disabled).length > 10;

        const label = () => {
            const opts = [...select.options];
            if (multiple) {
                const picked = opts.filter(o => o.selected);
                // Tək seçimdə ad göstərilir: əsas dərsin avtomatik seçildiyi formada
                // istifadəçi siyahını açmadan nəyin seçili olduğunu görsün.
                if (picked.length === 1) return picked[0].text;
                return picked.length
                    ? (L["Js_SelectedCount"] || '{0}').replace('{0}', picked.length)
                    : (select.dataset.placeholder || '');
            }
            const sel = opts[select.selectedIndex];
            return sel ? sel.text : '';
        };

        // "İ" hərfi: AZ/TR mətnlərində toLowerCase gözlənilən nəticəni vermir,
        // ona görə hər iki tərəf normallaşdırılır ("İngilis dili" ≈ "ingilis").
        const norm = (v) => String(v).normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().replace(/ı/g, 'i');

        function renderOptions() {
            const q = norm(search.value || '').trim();
            const opts = [...select.options].filter(o => !q || norm(o.text).includes(q));
            if (!opts.length) {
                list.innerHTML = `<div class="admin-select-empty">${L["Js_NoResults"]}</div>`;
                return;
            }
            list.innerHTML = opts.map(o => `
                <button type="button" class="admin-select-option${(multiple ? o.selected : o.index === select.selectedIndex) ? ' is-selected' : ''}"
                        data-value="${UI.escapeHtml(o.value)}">
                    ${multiple ? '<span class="admin-select-box"></span>' : ''}
                    <span class="truncate">${UI.escapeHtml(o.text)}</span>
                </button>`).join('');
        }

        function sync() {
            trigger.textContent = label();
            trigger.classList.toggle('is-empty', !label());
        }

        // Modal gövdəsi scroll olunur və açılan siyahını kəsir — ona görə siyahı
        // düymənin koordinatlarına görə fixed yerləşdirilir.
        function place() {
            const r = trigger.getBoundingClientRect();
            const below = window.innerHeight - r.bottom;
            const height = Math.min(280, menu.scrollHeight || 280);
            menu.style.position = 'fixed';
            menu.style.width = r.width + 'px';
            menu.style.left = r.left + 'px';
            menu.style.top = (below < height + 16 && r.top > height
                ? r.top - height - 6
                : r.bottom + 6) + 'px';
        }

        function open() {
            closeSelects();
            search.value = '';
            const needsSearch = withSearch();
            searchBox.classList.toggle('hidden', !needsSearch);
            renderOptions();
            menu.classList.remove('hidden');
            wrap.classList.add('is-open');
            openSelectWrap = wrap;
            place();
            if (needsSearch) search.focus();
        }

        trigger.addEventListener('click', () => {
            if (select.disabled) return;
            wrap.classList.contains('is-open') ? closeSelects() : open();
        });

        search.addEventListener('input', renderOptions);

        // ESC yalnız siyahını bağlayır — modal bağlanmasın.
        wrap.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') { e.stopPropagation(); closeSelects(); trigger.focus(); }
        });
        search.addEventListener('keydown', (e) => { if (e.key === 'Enter') e.preventDefault(); });

        list.addEventListener('click', (e) => {
            const btn = e.target.closest('.admin-select-option');
            if (!btn) return;
            const option = [...select.options].find(o => o.value === btn.dataset.value);
            if (!option) return;

            if (multiple) {
                option.selected = !option.selected;
                btn.classList.toggle('is-selected', option.selected);
            } else {
                select.value = option.value;
                closeSelects();
            }
            sync();
            select.dispatchEvent(new Event('change', { bubbles: true }));
        });

        select.addEventListener('change', sync);

        // Kənara klik: yalnız sinfi götürmək kifayət etmir — menyu da gizlədilməlidir.
        document.addEventListener('click', (e) => {
            if (wrap.classList.contains('is-open') && !wrap.contains(e.target)) UI.closeSelects();
        });

        sync();
    }

    function enhanceSelects(root) {
        (root || document).querySelectorAll('select:not([data-enhanced]):not([data-native])').forEach(select => {
            if (select.closest('.admin-select')) return;
            enhanceSelect(select);
        });
    }


    document.addEventListener('click', (e) => {
        const btn = e.target.closest?.('[data-file-remove]');
        if (btn) UI.fileRemove(btn.dataset.fileRemove);
    });

    document.addEventListener('change', (e) => {
        const input = e.target.closest?.('[data-file-input]');
        if (input) UI.filePreview(input.dataset.fileInput);
    });

    document.addEventListener('DOMContentLoaded', () => {
        UI.enhanceSelects();
        UI.initEditors();
        UI.initTabs();
    });

    // Açıq siyahı scroll zamanı düyməni izləyir.
    window.addEventListener('scroll', () => {
        const wrap = document.querySelector('.admin-select.is-open');
        if (!wrap) return;
        const trigger = wrap.querySelector('.admin-select-trigger');
        const menu = wrap.querySelector('.admin-select-menu');
        const r = trigger.getBoundingClientRect();
        menu.style.left = r.left + 'px';
        menu.style.top = (r.bottom + 6) + 'px';
    }, true);

    Object.assign(UI, { isVideoUrl, setPreview, filePreview, fileSet, fileRemove, fileReset, initEditors, syncEditors, closeSelects, enhanceSelect, enhanceSelects });
})(window.AdminUI);
