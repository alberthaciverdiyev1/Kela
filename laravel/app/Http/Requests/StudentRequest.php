<?php

namespace App\Http\Requests;

use Illuminate\Validation\Rule;

/**
 * Şagird yaratma / yeniləmə — eyni qaydalar həm web, həm API üçün.
 * Yeniləmədə email unikal yoxlaması cari istifadəçini xaric edir
 * (route('student') parametri hər iki routa-da mövcuddur).
 */
class StudentRequest extends BaseRequest
{
    public function rules(): array
    {
        $studentId = $this->route('student');
        $isUpdate = $studentId !== null;

        return [
            'first_name' => ['required', 'string', 'max:255'],
            'last_name' => ['nullable', 'string', 'max:255'],
            'email' => ['required', 'email', 'max:255', Rule::unique('users', 'email')->ignore($studentId)],
            'password' => [$isUpdate ? 'nullable' : 'required', 'string', 'min:6', 'max:255'],
            'city_id' => ['nullable', 'integer', 'exists:cities,id'],
            'birth_date' => ['nullable', 'date', 'before_or_equal:today'],
            'status' => ['nullable', 'integer', 'in:1,2,3'],
        ];
    }

    protected function prepareForValidation(): void
    {
        if (! $this->has('status')) {
            $this->merge(['status' => 1]);
        }
    }
}
