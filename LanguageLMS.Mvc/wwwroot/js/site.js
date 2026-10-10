const sidebar = document.getElementById("adminSidebar");
const sidebarToggle = document.getElementById("sidebarToggle");
const sidebarOverlay = document.getElementById("sidebarOverlay");

function setSidebarOpen(isOpen) {
    if (!sidebar || !sidebarOverlay) {
        return;
    }

    sidebar.classList.toggle("-translate-x-full", !isOpen);
    sidebarOverlay.classList.toggle("hidden", !isOpen);
}

sidebarToggle?.addEventListener("click", () => setSidebarOpen(true));
sidebarOverlay?.addEventListener("click", () => setSidebarOpen(false));

window.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
        setSidebarOpen(false);
    }
});
