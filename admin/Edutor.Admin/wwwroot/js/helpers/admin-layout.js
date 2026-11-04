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
        const alerts = new Set(data.alertKeys || []);
        const labels = data.labels || {};

        document.querySelectorAll('#sidebar [data-sidebar-key]').forEach(host => {
            const key = host.getAttribute('data-sidebar-key');
            const count = counts[key] ?? 0;
            const isAlert = alerts.has(key) && count > 0;

            // Sadə sayğac: mətnin yanında boz etiket.
            const countEl = host.querySelector('.sidebar-count');
            if (countEl) {
                countEl.textContent = count > 99 ? '99+' : String(count);
                countEl.classList.toggle('hidden', count === 0 || isAlert);
            }

            // Müraciət/mesaj/bildiriş: zəng işarəsi + qırmızı sayğac.
            host.querySelector('.sidebar-new')?.classList.toggle('hidden', !isAlert);
            const alertEl = host.querySelector('.sidebar-alert-count');
            if (alertEl) {
                alertEl.textContent = count > 99 ? '99+' : String(count);
                alertEl.classList.toggle('hidden', !isAlert);
            }

            const labelEl = host.querySelector('.sidebar-label');
            if (labelEl) {
                labelEl.textContent = labels[key] || '';
                labelEl.classList.toggle('hidden', !labels[key]);
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

    // Sidebar bölmələri yığıla bilər. Vəziyyət cookie-də saxlanılır; ilk render-də
    // server onu tətbiq etdiyi üçün məzmun açılıb sonra yox olmur.
    function initSidebarSections() {
        const nav = document.querySelector('#sidebar .sidebar-nav');
        if (!nav) return;

        const collapsed = new Set(
            [...nav.querySelectorAll('.sidebar-section[data-collapsed]')].map(s => s.dataset.section));

        function saveState() {
            document.cookie = 'sidebar_collapsed=' + [...collapsed].join(',') +
                ';path=/;max-age=31536000;samesite=lax';
        }

        nav.querySelectorAll('[data-section-toggle]').forEach(btn => {
            btn.addEventListener('click', () => {
                const section = btn.closest('.sidebar-section');
                const nowCollapsed = !section.hasAttribute('data-collapsed');
                section.toggleAttribute('data-collapsed', nowCollapsed);
                if (nowCollapsed) collapsed.add(section.dataset.section);
                else collapsed.delete(section.dataset.section);
                section.querySelector('.sidebar-chevron')?.classList.toggle('rotate-180', !nowCollapsed);
                saveState();
            });
        });

        // Uyğunlaşdırma rejimində bütün bölmələr açılır — gizlədiləcək sətirlər görünməlidir.
        document.getElementById('navEditBtn')?.addEventListener('click', () => {
            setTimeout(() => {
                if (document.getElementById('sidebar').dataset.editing !== 'true') return;
                nav.querySelectorAll('.sidebar-section[data-collapsed]').forEach(section => {
                    section.removeAttribute('data-collapsed');
                    section.querySelector('.sidebar-chevron')?.classList.add('rotate-180');
                });
            }, 0);
        });
    }
    initSidebarSections();

    // "Abunəliyim" və "Limitlərim" eyni səhifəyə aparır — aktiv görünən hash ilə seçilir.
    function syncSubnavActive() {
        const items = document.querySelectorAll('#sidebar [data-subnav]');
        if (!items.length) return;
        const onPage = location.pathname.replace(/\/+$/, '') === '/subscription';
        const current = location.hash === '#limits' ? 'limits' : 'plan';
        items.forEach(el => {
            const active = onPage && el.dataset.subnav === current;
            el.classList.toggle('bg-primary-light', active);
            el.classList.toggle('text-primary', active);
            el.classList.toggle('text-slate-600', !active);
            el.classList.toggle('hover:bg-surface-secondary', !active);
            el.classList.toggle('hover:text-slate-900', !active);
        });
    }
    // Scroll konteyneri <main>-dir; brauzerin ilk hash-sürüşdürməsi burada işləmir.
    function scrollToSubnav() {
        const id = location.hash.replace('#', '');
        if (id !== 'plan' && id !== 'limits') return;
        document.getElementById(id)?.scrollIntoView({ block: 'start', behavior: 'instant' });
    }

    syncSubnavActive();
    scrollToSubnav();
    // Şəkillər/şriftlər yükləndikcə hündürlük dəyişir — səhifə tam hazır olanda təkrarla.
    window.addEventListener('load', scrollToSubnav);
    window.addEventListener('hashchange', () => {
        syncSubnavActive();
        scrollToSubnav();
    });

    // Köhnə çağırışlar (səhifələr müraciət edir) — artıq vahid yeniləməyə yönəlir.
    window.refreshGroupJoinBadge = refreshSidebar;
    window.refreshTenantPendingBadge = refreshSidebar;
    // Sayğaclar serverdə çəkilirsə ilk sorğu lazımsızdır — nişanlar yanıb-sönməsin.
    if (!document.getElementById('sidebar')?.dataset.sidebarRendered) refreshSidebar();
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
