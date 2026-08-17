<?php

namespace App\Http\Requests;

use App\Application\Workspace\WorkspaceService;

/**
 * Aylıq davam (yoklama) qeydlərini kütləvi yaz.
 * workspace_id routa parametridir — bədənə daxil deyil, buna görə qaydada yoxdur.
 * Yetki yoxlaması (workspace sahibliyi) FormRequest::authorize() içindədir ki,
 * doğrulamadan əvvəl 403 qayıtsın.
 */
class SaveMonthAttendanceRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'month' => ['required', 'string'],
            'days' => ['required', 'array'],
            'days.*.*' => ['integer', 'min:0', 'max:4'],
            'note' => ['nullable', 'string'],
        ];
    }

    public function authorize(): bool
    {
        $workspace = app(WorkspaceService::class)->find((int) $this->route('workspace'));
        if ($workspace === null) {
            abort(404);
        }

        $user = $this->user();
        if ($user === null) {
            return false;
        }
        if ($user->isAdmin()) {
            return true;
        }

        return $workspace->teacher_id === (int) $user->id;
    }
}
