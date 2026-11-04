(function () {
    const frame = document.getElementById('studentViewFrame');
    const tabs = document.getElementById('viewSizeTabs');
    if (!frame || !tabs) return;

    tabs.addEventListener('click', (e) => {
        const tab = e.target.closest('[data-view-size]');
        if (!tab) return;

        tabs.querySelectorAll('[data-view-size]').forEach(t => t.classList.toggle('is-active', t === tab));
        frame.classList.toggle('is-mobile', tab.dataset.viewSize === 'mobile');
    });

    document.getElementById('reloadFrame')?.addEventListener('click', () => {
        frame.src = frame.src;
    });
})();
