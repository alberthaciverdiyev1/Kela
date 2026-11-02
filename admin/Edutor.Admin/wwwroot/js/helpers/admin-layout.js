// Edutor admin panel — layout: sidebar, menyular və sidebar sayğacları.
(function () {
    // Hər sidebar bölməsi üçün sayğac, "yeni" zəngi və superadmin etiketi.
    async function refreshSidebar() {
        let data;
        try {
            const res = await fetch('/sidebar/summary');
            if (!res.ok) return;
            data = await res.json();
        } catch (e) {
            return;
        }

        const counts = data.counts || {};
        const news = new Set(data.newKeys || []);
        const labels = data.labels || {};

        document.querySelectorAll('#sidebar [data-sidebar-key]').forEach(host => {
            const key = host.getAttribute('data-sidebar-key');
            const count = counts[key] ?? 0;
            const isNew = news.has(key);

            const countEl = host.querySelector('.sidebar-count');
            if (countEl) {
                countEl.textContent = count > 99 ? '99+' : String(count);
                countEl.classList.toggle('hidden', count === 0);
                countEl.classList.toggle('bg-danger', isNew);
                countEl.classList.toggle('text-white', isNew);
                countEl.classList.toggle('bg-slate-100', !isNew);
                countEl.classList.toggle('text-slate-500', !isNew);
            }

            host.querySelector('.sidebar-new')?.classList.toggle('hidden', !isNew);

            const labelEl = host.querySelector('.sidebar-label');
            if (labelEl) {
                const label = labels[key] || '';
                labelEl.textContent = label;
                labelEl.classList.toggle('hidden', !label);
            }
        });
    }

    // Bölməyə baxış qeyd olunur — növbəti yükləmədə "yeni" işarəsi düşür.
    function markSeen(key) {
        fetch('/sidebar/seen/' + encodeURIComponent(key), {
            method: 'POST',
            headers: { 'RequestVerificationToken': AdminUI.token() }
        }).catch(() => { /* səssiz */ });
    }

    document.addEventListener('click', e => {
        const link = e.target.closest('#sidebar a[data-nav-key]');
        if (link) markSeen(link.getAttribute('data-nav-key'));
    });

    // Köhnə çağırışlar (səhifələr müraciət edir) — artıq vahid yeniləməyə yönəlir.
    window.refreshGroupJoinBadge = refreshSidebar;
    window.refreshTenantPendingBadge = refreshSidebar;
    refreshSidebar();
    document.addEventListener('visibilitychange', () => {
        if (document.visibilityState === 'visible') refreshSidebar();
    });

    window.setLang = function (code) {
        document.cookie = ".AspNetCore.Culture=c=" + code + "|uic=" + code + ";path=/;max-age=31536000";
        location.reload();
    };

    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('overlay');
    const toggle = document.getElementById('menuToggle');
    function openSidebar() { sidebar.classList.remove('-translate-x-full'); overlay.classList.remove('hidden'); }
    function closeSidebar() { sidebar.classList.add('-translate-x-full'); overlay.classList.add('hidden'); }
    toggle?.addEventListener('click', openSidebar);
    overlay?.addEventListener('click', closeSidebar);

    const userMenuBtn = document.getElementById('userMenuBtn');
    const userMenu = document.getElementById('userMenu');
    const langMenuBtn = document.getElementById('langMenuBtn');
    const langMenu = document.getElementById('langMenu');
    const langBackdrop = document.getElementById('langBackdrop');

    function closeMenus() {
        userMenu?.classList.add('hidden');
        langMenu?.classList.add('hidden');
        langBackdrop?.classList.add('hidden');
    }

    userMenuBtn?.addEventListener('click', (e) => { e.stopPropagation(); closeMenus(); userMenu.classList.toggle('hidden'); });
    langMenuBtn?.addEventListener('click', (e) => {
        e.stopPropagation();
        userMenu?.classList.add('hidden');
        const nowHidden = langMenu.classList.toggle('hidden');
        langBackdrop?.classList.toggle('hidden', nowHidden);
    });
    document.addEventListener('click', closeMenus);
    document.addEventListener('keydown', (e) => { if (e.key === 'Escape') closeMenus(); });
})();
