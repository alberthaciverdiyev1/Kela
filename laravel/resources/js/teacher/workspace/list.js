/**
 * LIST — Workspace kataloqunu yeniləmə əməliyyatı.
 *
 * Niyə ayrı fayl: "list" ilə bağlı bütün kod (server-rendered fragment-i
 * çəkmək və DOM-da əvəz etmək) burada saxlanılır ki, controller.js-də
 * list funksiya kodu olmasın.
 *
 * Bu modul yalnız fragment URL-ə bağlıdır; kataloq elementi refresh zamanı
 * parametr kimi verilir (controller $refs-dən toplayıb ötürür).
 */
export default function createDirectoryList(fragmentUrl) {
    return {
        /**
         * refresh(dirEl) — Kataloq hissəsini yenidən çəkir.
         *
         * Niyə fragment: əməliyyatdan sonra tam səhifə reload etmək əvəzinə
         * yalnız kataloq serverdən təzə fragment olaraq gətirilib yerində
         * əvəz olunur. Tam reload yolu/axtarış vəziyyətini sıfırlayır və
         * yavaşdır — fragment üsulu hər ikisini qoruyur.
         */
        async refresh(dirEl) {
            try {
                dirEl.innerHTML = await KelaFragment(fragmentUrl);
            } catch (err) {
                window.alert(err.message);
            }
        },
    };
}
