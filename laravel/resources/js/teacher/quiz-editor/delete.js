/**
 * DELETE — Sualı quizdən çıxar.
 *
 * DELETE /api/v1/quizzes/{id}/questions/{questionId}
 * (sual bankında qalır, yalnız quiz-dən çıxarılır).
 */
export default function createQuestionRemover({ api }) {
    async function remove(id) {
        if (!window.confirm('Sual quizdən çıxarılsın?')) return false;
        try {
            await KelaApi('DELETE', `${api}/questions/${id}`);
            return true;
        } catch (err) {
            window.alert(err.message);
            return false;
        }
    }

    return { remove };
}
