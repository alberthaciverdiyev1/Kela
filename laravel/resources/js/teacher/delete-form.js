/**
 * İndeks səhifələrində silmə formaları üçün Alpine komponenti.
 *
 * İstifadə:
 *   <form method="POST" action="...destroy"
 *         x-data="deleteForm({ url: '/teacher/students/1', message: 'Silmək istəyirsiniz?' })"
 *         @submit.prevent="submit">
 *       @csrf
 *       @method('DELETE')
 *       <button type="submit">Sil</button>
 *   </form>
 *
 * JS aktiv olduqda confirm + DELETE edir (web controller endpoint); JS yoxdursa
 * form normal POST edir (progressive enhancement — server-də DELETE routuna gedir).
 */
export default function deleteForm(config) {
    return {
        url: config.url,
        message: config.message || 'Silmək istəyirsiniz?',
        busy: false,

        async submit() {
            if (!window.confirm(this.message)) return;
            this.busy = true;
            try {
                await KelaApi('DELETE', this.url);
                window.location.reload();
            } catch (err) {
                window.alert(err.message);
                this.busy = false;
            }
        },
    };
}
