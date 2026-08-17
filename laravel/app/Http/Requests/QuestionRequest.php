<?php

namespace App\Http\Requests;

/**
 * Sual yaratma / yeniləmə — eyni qaydalar həm web, həm API üçün.
 * Sual mətni rich text HTML ola bilər — boşluq yoxlaması düz mətnə görədir.
 */
class QuestionRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'text' => ['required', 'string', $this->nonEmptyRichText()],
            'option_a' => ['required', 'string'],
            'option_b' => ['required', 'string'],
            'option_c' => ['nullable', 'string'],
            'option_d' => ['nullable', 'string'],
            'option_e' => ['nullable', 'string'],
            'correct_option' => ['required', 'integer', 'min:0', 'max:4'],
            'explanation' => ['nullable', 'string'],
            'folder_id' => ['nullable', 'integer'],
        ];
    }

    private function nonEmptyRichText(): \Closure
    {
        return function (string $attribute, $value, $fail): void {
            if (trim(strip_tags((string) $value)) === '') {
                $fail('Sual mətni boş ola bilməz.');
            }
        };
    }
}
