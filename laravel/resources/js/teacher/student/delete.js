/**
 * DELETE — Şagirdi silmək əməliyyatı.
 *
 * Niyə ayrı fayl: "delete" ilə bağlı bütün kod (təsdiq dialoqu, API DELETE,
 * xəta idarəsi) burada saxlanılır ki, index.js-də delete funksiya kodu
 * olmasın.
 */
export default function createStudentRemover() {
    return {
        /**
         * remove(id, name) — Silmə əməliyyatını icra edir.
         *
         * Niyə confirm: silmə geri qaytarıla bilməz — istifadəçiyə şagirdin
         * adını göstərib razılıq istəyirik. Uğur → true, ləğv/xəta → false
         * (controller cədvəli yalnız uğurdan sonra təzələyir).
         */
        async remove(id, name = 'Şagird') {
            if (!window.confirm(`'${name}' silinsin?`)) return false;
            try {
                await KelaApi('DELETE', `/api/v1/students/${id}`);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },
    };
}
