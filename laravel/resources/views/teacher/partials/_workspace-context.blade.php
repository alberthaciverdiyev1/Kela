{{--
    Workspace kontekst banneri — quiz/dərs formunda istifadə olunur.
    `$workspaceContext` null deyilsə, content müəyyən workspace-in qovluğuna əlavə olunur.

    Dəyişənlər:
      $workspaceContext: ?array  ['workspace_id', 'folder_id', 'workspace_name', 'folder_name']
--}}
@if ($workspaceContext !== null)
    <div class="flex items-center justify-between gap-3 rounded-lg border border-primary/30 bg-primary/5 px-4 py-3">
        <div class="flex items-center gap-2 text-sm">
            <x-icon name="heroicon-o-squares-2x2" class="size-4 text-primary" />
            <span class="text-base-content/70">Bu məzmun:</span>
            <span class="font-semibold text-base-content">{{ $workspaceContext['workspace_name'] }}</span>
            @if ($workspaceContext['folder_name'])
                <x-icon name="heroicon-o-chevron-right" class="size-3.5 text-base-content/40" />
                <span class="text-base-content/80">{{ $workspaceContext['folder_name'] }}</span>
            @else
                <x-teacher.badge color="gray">Kök</x-teacher.badge>
            @endif
        </div>
    </div>

    <input type="hidden" name="workspace_id" value="{{ $workspaceContext['workspace_id'] }}" />
    @if ($workspaceContext['folder_id'])
        <input type="hidden" name="ws_folder_id" value="{{ $workspaceContext['folder_id'] }}" />
    @endif
@endif
