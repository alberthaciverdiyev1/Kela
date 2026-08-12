/**
 * ADD — Quiz-ə bankdan sual əlavə et.
 *
 * POST /api/v1/quizzes/{id}/questions  (question_id)
 *
 * Sual yaratma Sual Bankı modulundadır (teacher/question) — burada yalnız
 * mövcud bank sualını quizə bağlamaq var.
 */
export default function createQuestionAdder({ api }) {
    async function addFromBank(questionId) {
        if (!questionId) {
            window.alert('Sual seçin.');
            return false;
        }
        try {
            await KelaApi('POST', `${api}/questions`, { question_id: Number(questionId) });
            return true;
        } catch (err) {
            window.alert(err.message);
            return false;
        }
    }

    return { addFromBank };
}
