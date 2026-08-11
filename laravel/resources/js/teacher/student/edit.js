
export default function createStudentUpdater() {
    return {
        open(student, fields) {
            fields.firstName.value = student.first_name || '';
            fields.lastName.value = student.last_name || '';
            fields.email.value = student.email || '';
            fields.password.value = '';
            fields.cityId.value = student.city_id ? String(student.city_id) : '';
            fields.birthDate.value = student.birth_date || '';
            fields.status.value = String(student.status ?? 1);
        },

        buildPayload(fields) {
            const payload = {
                first_name: fields.firstName.value.trim(),
                last_name: fields.lastName.value.trim(),
                email: fields.email.value.trim(),
                password: fields.password.value,
                city_id: fields.cityId.value ? Number(fields.cityId.value) : null,
                birth_date: fields.birthDate.value || null,
                status: Number(fields.status.value),
            };
            if (!payload.password) delete payload.password;
            return payload;
        },

        async update(fields, id) {
            const payload = this.buildPayload(fields);
            if (!payload.first_name || !payload.email) {
                window.alert('Ad və e-poçt tələb olunur.');
                return false;
            }
            try {
                await KelaApi('PUT', `/api/v1/students/${id}`, payload);
                return true;
            } catch (err) {
                window.alert(err.message);
                return false;
            }
        },
    };
}
