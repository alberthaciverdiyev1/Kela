/**
 * EDIT — Quizdəki sualın sırasını dəyiş.
 *
 * POST /teacher/quizzes/{id}/questions/{qid}/move (yuxarı/aşağı).
 *
 * Sualın məzmununu düzləndirmək Sual Bankı modulundadır (teacher/question) —
 * burada yalnız quiz daxili sıralama var.
 */
export default function createQuestionUpdater({ api }) {
    async function move(id, direction) {
        try {
            await KelaApi('POST', `${api}/questions/${id}/move`, { direction });
            return true;
        } catch (err) {
            window.alert(err.message);
            return false;
        }
    }

    return { move };
}
