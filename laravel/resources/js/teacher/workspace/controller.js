/**
 * Controller — Workspace səhifəsinin giriş nöqtəsi və "orkestr"i.
 *
 * Workspace artıq sadə quruluşdur: ad + tələbələr. Node/qovluq/məzmun
 * ağacı yoxdur. Burada yalnız tələbə əlavə et / çıxar dialoqu idarə olunur:
 *
 *   add.js    → tələbə əlavə et (addStudents)
 *   delete.js → tələbə çıxar     (detachStudent)
 *
 * Əsl HTTP çağırışı add/delete modullarındadır. Controller yalnız dialoq
 * vəziyyətini saxlayır, modullardan { ok } gəlincə səhifəni təzələyir.
 */
import Alpine from 'alpinejs';
import createWorkspaceAdder from './add';
import createWorkspaceRemover from './delete';

export default function workspaceManager(config) {
    const api = '/api/v1/workspaces/' + config.workspaceId;
    const adder = createWorkspaceAdder({ api });
    const remover = createWorkspaceRemover({ api });

    return {
        // ── Konfiqurasiya ──────────────────────────────────────────────────
        workspaceId: config.workspaceId,
        api,

        // ── UI vəziyyəti ───────────────────────────────────────────────────
        showStudent: false,

        /**
         * saveStudents() — Tələbə dialoqundakı "Əlavə Et" düyməsi basılanda.
         *
         * Çoxlu seçim (multi-select) dəyərlərini DOM-dan toplayıb add moduluna
         * verir. Niyə reload: tələbə siyahısı server-rendered bölmədir —
         * təzə render ən sadə yoldur.
         */
        async saveStudents() {
            const select = this.$refs.studentSelect;
            const ids = Array.from(select.selectedOptions).map((o) => Number(o.value));
            const ok = await adder.addStudents(ids);
            if (!ok) return;
            window.location.reload();
        },

        /**
         * detachStudent(btn) — Tələbə sətirindəki "Çıxar" düyməsi basılanda.
         */
        async detachStudent(btn) {
            const ok = await remover.detachStudent(btn.dataset.studentId, btn.dataset.studentName);
            if (ok) window.location.reload();
        },

        /**
         * closeAll() — Escape düyməsi basılanda bütün dialoqları bağlayır.
         */
        closeAll() {
            this.showStudent = false;
        },
    };
}

// Alpine-də qeydiyyat: blade-də x-data="workspaceManager(...)" işləyə bilsin.
Alpine.data('workspaceManager', workspaceManager);
Alpine.start();
