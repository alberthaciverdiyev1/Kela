<?php

namespace App\Http\Requests;

/** Bank qovluğunu (içindəki məzmunlarla) workspace-ə əlavə et. */
class AddFolderToWorkspaceRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'folder_type' => ['required', 'string', 'in:quiz,lesson'],
            'bank_folder_id' => ['required', 'integer'],
            'workspace_id' => ['required', 'integer'],
            'folder_id' => ['nullable', 'integer'],
        ];
    }
}
