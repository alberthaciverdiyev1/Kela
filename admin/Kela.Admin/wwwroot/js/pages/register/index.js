document.addEventListener('change', (e) => {
    const input = e.target.closest?.('[data-logo-input]');
    if (!input) return;
    const label = document.getElementById('logoName');
    if (label) label.textContent = input.files[0] ? input.files[0].name : (input.dataset.empty || '');
});
