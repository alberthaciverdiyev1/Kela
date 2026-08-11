/**
 * EDIT — Mövcud sualı düzləndir.
 *
 * PUT /api/v1/questions/{id} (məzmun) və ya sıralama:
 * POST /api/v1/quizzes/{id}/questions/{qid}/move (yuxarı/aşağı).
 */
export default function createQuestionUpdater({ api, getPayload }) {
    async function update(id) {
        const payload = getPayload();
        if (!payload.text.trim() || !payload.option_a || !payload.option_b) {
            window.alert('Sual mətni və ən azı A, B seçimləri tələb olunur.');
            return false;
        }
        try {
            await KelaApi('PUT', `/api/v1/questions/${id}`, payload);
            return true;
        } catch (err) {
            window.alert(err.message);
            return false;
        }
    }

    async function move(id, direction) {
        try {
            await KelaApi('POST', `${api}/questions/${id}/move`, { direction });
            return true;
        } catch (err) {
            window.alert(err.message);
            return false;
        }
    }

    return { update, move };
}
