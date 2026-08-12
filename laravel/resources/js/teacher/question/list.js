/**
 * LIST — Sual bankı kataloqu yeniləmə.
 *
 * Server-rendered fragment-i (GET /teacher/questions/table) çəkib DOM-da
 * əvəz edir ki, controller-də list funksiya kodu olmasın.
 */
export default function createBankList(fragmentUrl) {
    return {
        async refresh(dirEl) {
            try {
                dirEl.innerHTML = await KelaFragment(fragmentUrl);
            } catch (err) {
                window.alert(err.message);
            }
        },
    };
}
