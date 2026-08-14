/**
 * Sağ-tık kontekst menyusu — "file manager" səhifələri (workspace, dərs,
 * quiz, sual bankı) üçün ortaq Alpine vəziyyəti.
 *
 * İstifadə: controller-də object spread ilə əridin:
 *
 *   import createContextMenu from '../context-menu';
 *   export default function myPage(config) {
 *       return {
 *           ...createContextMenu(),
 *           // öz state & metodlar...
 *       };
 *   }
 *
 * Blade tərəfi: resources/views/teacher/partials/_context-menu.blade.php
 * səhifənin sonunda bir dəfə əlavə olunur və ctxMenu.show olduqda
 * attendance popover üslubunda menyunu çəkir.
 *
 * Menu item strukturu:
 *   { icon, iconClass, label, divider?, danger? }
 *   { icon, iconClass, label, href, target? }            → link (açar)
 *   { icon, iconClass, label, action, danger? }          → aksiya (funksiya)
 */
export default function createContextMenu() {
    return {
        ctxMenu: {
            show: false,
            title: '',
            items: [],
            left: 0,
            top: 0,
        },

        /**
         * Sağ-tık hadisəsini cursor mövqeyində açır.
         * items: [{ icon, iconClass, label, action?, href?, target?, divider?, danger? }]
         */
        openCtxMenu(event, title, items = []) {
            event?.preventDefault();
            const width = 256;
            const rows = items.filter((i) => !i.divider).length;
            const height = 44 + rows * 38 + 8 + 8;
            this.ctxMenu = {
                show: true,
                title,
                items,
                left: Math.max(8, Math.min(event?.clientX ?? 0, window.innerWidth - width - 8)),
                top: Math.max(8, Math.min(event?.clientY ?? 0, window.innerHeight - height - 8)),
            };
        },

        closeCtxMenu() {
            this.ctxMenu.show = false;
            this.ctxMenu.items = [];
        },

        /** Menyu elementinə klik: aksiyanı işə salıb menyunu bağlayır. */
        runCtxItem(item) {
            this.closeCtxMenu();
            if (typeof item.action === 'function') {
                item.action();
            }
        },
    };
}
