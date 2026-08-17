import Alpine from 'alpinejs';
import createStudentList from './list';
import createStudentAdder from './add';
import createStudentUpdater from './edit';
import createStudentRemover from './delete';

export default function studentManager(config) {
    // CRUD modulları — hər biri öz əməliyyatını bilir, bir-birindən asılı deyil.
    const list = createStudentList(config.fragmentUrl);
    const adder = createStudentAdder();
    const updater = createStudentUpdater();
    const remover = createStudentRemover();

    return {
        showForm: false,
        showGenerate: false,
        formTitle: 'Yeni Şagird',
        editingId: null,

        cities: config.cities || {},
        statuses: config.statuses || {},

        getFields() {
            return {
                firstName: this.$refs.firstName,
                lastName: this.$refs.lastName,
                email: this.$refs.email,
                password: this.$refs.password,
                cityId: this.$refs.cityId,
                birthDate: this.$refs.birthDate,
                status: this.$refs.status,
            };
        },

        openAdd() {
            this.editingId = null;
            this.formTitle = 'Yeni Şagird';
            adder.open(this.getFields());
            this.showForm = true;
        },

        openEdit(btn) {
            const student = JSON.parse(btn.dataset.student || '{}');
            this.editingId = Number(btn.dataset.studentId);
            this.formTitle = 'Şagirdi Redaktə Et';
            updater.open(student, this.getFields());
            this.showForm = true;
        },

        async save() {
            const ok = this.editingId
                ? await updater.update(this.getFields(), this.editingId)
                : await adder.add(this.getFields());
            if (!ok) return;
            this.showForm = false;
            await list.refresh(this.$refs.table);
        },

        async remove(btn) {
            const ok = await remover.remove(btn.dataset.studentId, btn.dataset.studentName);
            if (ok) await list.refresh(this.$refs.table);
        },


        onTableClick(e) {
            const editBtn = e.target.closest('[data-student-edit]');
            if (editBtn) { this.openEdit(editBtn); return; }
            const delBtn = e.target.closest('[data-student-delete]');
            if (delBtn) this.remove(delBtn);
        },
    };
}

Alpine.data('studentManager', studentManager);
Alpine.start();
