// Edutor admin panel — layout: sidebar, menyular və sidebar badge-ləri.
(function () {
    // Gözləyən qoşulma müraciətləri — sidebar-dakı Qruplar yanında qırmızı badge.
    async function refreshGroupJoinBadge() {
        try {
            const res = await fetch('/group/join-request-count');
            if (!res.ok) return;
            const data = await res.json();
            const badge = document.getElementById('groupJoinBadge');
            if (!badge) return;
            badge.textContent = data.count;
            badge.style.display = data.count > 0 ? 'inline-flex' : 'none';
        } catch (e) { /* səssiz */ }
    }

    async function refreshTenantPendingBadge() {
        const badge = document.getElementById('tenantPendingBadge');
        if (!badge) return;
        try {
            const res = await fetch('/tenant/pending-count');
            if (!res.ok) return;
            const data = await res.json();
            badge.textContent = data.count;
            badge.style.display = data.count > 0 ? 'inline-flex' : 'none';
        } catch (e) { /* səssiz */ }
    }

    window.refreshGroupJoinBadge = refreshGroupJoinBadge;
    window.refreshTenantPendingBadge = refreshTenantPendingBadge;
    refreshGroupJoinBadge();
    refreshTenantPendingBadge();

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
