/**
 * ADD — Quiz-ə sual əlavə et.
 *
 * Yeni sual: POST /api/v1/questions (sual bankına), sonra
 * POST /api/v1/quizzes/{id}/questions (quizə bağla).
 * Bankdan: POST /api/v1/quizzes/{id}/questions.
 */
export default function createQuestionAdder({ api, getPayload }) {
    async function add() {
        const payload = getPayload();
        if (!payload.text.trim() || !payload.option_a || !payload.option_b) {
            window.alert('Sual mətni və ən azı A, B seçimləri tələb olunur.');
            return false;
        }
        try {
            const created = await KelaApi('POST', '/api/v1/questions', payload);
            await KelaApi('POST', `${api}/questions`, { question_id: created.data.id });
            return true;
        } catch (err) {
            window.alert(err.message);
            return false;
        }
    }

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

    return { add, addFromBank };
}
