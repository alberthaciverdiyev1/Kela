/**
 * ADD — Yeni şagird yaratmaq əməliyyatı.
 *
 * Niyə ayrı fayl: "add" ilə bağlı bütün kod (form sıfırlama, payload qurma,
 * validasiya, POST /teacher/students web controller-i) burada saxlanılır ki,
 * index.js-də add funksiya kodu olmasın.
 *
 * Modul heç bir şəxsi reaktiv vəziyyət saxlamır. Form sahə elementlərini
 * parametr kimi alır (controller $refs-dən toplayıb verir) — beləcə modul
 * DOM-dan asılı deyil və tək başına test edilə bilər.
 */
export default function createStudentAdder() {
    return {
        /**
         * open(fields) — "Yeni Şagird" dialoqu açılanda formu təmizləyir.
         *
         * Niyə lazımdır: əvvəlki redaktədən qalan dəyərlər yeni əlavə
         * dialoqunda görünməməlidir. Hər sahə sıfırlanır, status "1" (Aktiv)
         * defolt olur.
         */
        open(fields) {
            fields.firstName.value = '';
            fields.lastName.value = '';
            fields.email.value = '';
            fields.password.value = '';
            fields.cityId.value = '';
            fields.birthDate.value = '';
            fields.status.value = '1';
        },

        /**
         * buildPayload(fields) — Form sahələrini API gözlədiyi formata yığır.
         *
         * Niyə burada: payload qurmaq "add" əməliyyatının məsuliyyətidir.
         * Növlər burada düzəldilir: city_id/status rəqəmə çevrilir, boş
         * tarix/şəhər null olur. Formaya yeni sahə əlavə etsək, dəyişiklik
         * yalnız bu moduldadır — index.js-ə toxunulmur.
         */
        buildPayload(fields) {
            return {
                first_name: fields.firstName.value.trim(),
                last_name: fields.lastName.value.trim(),
                email: fields.email.value.trim(),
                password: fields.password.value,
                city_id: fields.cityId.value ? Number(fields.cityId.value) : null,
                birth_date: fields.birthDate.value || null,
                status: Number(fields.status.value),
            };
        },

        /**
         * add(fields) — Yeni şagirdi API-ə göndərir.
         *
         * Niyə validasiya burada: "add" yalnız öz məlumatını qəbul edə bilər;
         * ad, e-poçt və şifrə tələb olunur. Uğur → true, uğursuz → false
         * (xəta mesajı alert-də göstərilir). Controller save() bunu yoxlayıb
         * dialoqu bağlamaq/açıq saxlamaq qərarını verir.
         */
        async add(fields) {
            const payload = this.buildPayload(fields);
            if (!payload.first_name || !payload.email || !payload.password) {
                window.alert('Ad, e-poçt və şifrə tələb olunur.');
                return false;
            }
            try {
                await KelaApi('POST', '/teacher/students', payload);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },
    };
}
