// Sidebar-ı istifadəçiyə uyğunlaşdırma: hər sətir fərdi olaraq gizlədilir/açılır və serverə yazılır.
(function () {
    const sidebar = document.getElementById('sidebar');
    const editBtn = document.getElementById('navEditBtn');
    if (!sidebar || !editBtn || !window.AdminUI) return;

    const nav = sidebar.querySelector('nav');
    if (!nav) return;

    const links = [...nav.querySelectorAll('a[data-nav-key]')];
    let editing = false;

    const EYE = '<svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M2.25 12C3.75 8 7.5 5.25 12 5.25S20.25 8 21.75 12c-1.5 4-5.25 6.75-9.75 6.75S3.75 16 2.25 12Z"/><circle cx="12" cy="12" r="3"/></svg>';
    const EYE_OFF = '<svg fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8"><path stroke-linecap="round" stroke-linejoin="round" d="M3 3l18 18M10.6 10.7a3 3 0 0 0 4.2 4.2M6.5 6.7C4.6 8 3.2 9.8 2.25 12c1.5 4 5.25 6.75 9.75 6.75 1.5 0 2.9-.35 4.15-.95M9.9 5.6A10.6 10.6 0 0 1 12 5.25c4.5 0 8.25 2.75 9.75 6.75-.5 1.35-1.25 2.6-2.2 3.6"/></svg>';

    const isHidden = link => link.classList.contains('hidden');

    const eyeFor = link => link.querySelector('.nav-eye');

    function paint(link) {
        const eye = eyeFor(link);
        if (!eye) return;
        const hidden = isHidden(link);
        eye.innerHTML = hidden ? EYE_OFF : EYE;
        eye.title = hidden ? L['Sidebar_ShowItem'] : L['Sidebar_HideItem'];
        eye.setAttribute('aria-label', eye.title);
        eye.setAttribute('aria-pressed', hidden ? 'false' : 'true');
    }

    function refreshSections() {
        nav.querySelectorAll(':scope > div').forEach(section => {
            const items = [...section.querySelectorAll('a[data-nav-key]')];
            if (!items.length) return;
            const visible = editing || items.some(link => !isHidden(link));
            section.classList.toggle('hidden', !visible);
            section.classList.toggle('all-hidden', items.every(isHidden));
        });
    }

    function setEditing(on) {
        editing = on;
        sidebar.dataset.editing = on ? 'true' : 'false';
        editBtn.title = on ? L['Sidebar_CustomizeDone'] : L['Sidebar_Customize'];
        editBtn.setAttribute('aria-label', editBtn.title);
        editBtn.setAttribute('aria-pressed', String(on));
        refreshSections();
    }

    async function save() {
        const fd = new FormData();
        links.filter(isHidden).forEach(link => fd.append('hidden', link.dataset.navKey));
        const res = await AdminUI.putForm('/profile/sidebar', fd);
        if (!res.ok) AdminUI.error(AdminUI.errText(res));
    }

    links.forEach(link => {
        const eye = document.createElement('span');
        eye.className = 'nav-eye';
        eye.setAttribute('aria-hidden', 'true');
        link.appendChild(eye);
        paint(link);

        link.addEventListener('click', e => {
            if (!editing) return;
            e.preventDefault();
            link.classList.toggle('hidden');
            paint(link);
            refreshSections();
            save();
        });
    });

    editBtn.addEventListener('click', () => setEditing(!editing));
    refreshSections();
})();
