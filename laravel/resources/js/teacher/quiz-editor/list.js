/**
 * LIST — Quiz sual siyahısı.
 *
 * Server-rendered fragment-i (GET /teacher/quizzes/{id}/questions) yenidən
 * çəkib DOM-da əvəz edir və sual sayını yeniləyir.
 */
export default function createQuestionList({ fragmentUrl, getListEl, setCount }) {
    async function refresh() {
        try {
            const html = await KelaFragment(fragmentUrl);
            getListEl().innerHTML = html;
            setCount(getListEl().querySelectorAll('tbody tr').length);
        } catch (err) {
            window.alert(err.message);
        }
    }

    return { refresh };
}
