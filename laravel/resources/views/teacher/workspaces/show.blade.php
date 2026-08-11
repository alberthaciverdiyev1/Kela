@extends('layouts.teacher')
@section('title', $workspaceName.' - Kela')
@section('content')
<div
    class="space-y-6"
    id="workspace-show"
    data-workspace-id="{{ $workspaceId }}"
    data-parent-id="{{ $parentId ?? '' }}"
    data-fragment-url="{{ route('teacher.workspaces.directory', [$workspaceId, 'parent_id' => $parentId ?? null]) }}"
>
    {{-- Header --}}
    <x-teacher.heading :subtitle="count($students).' tələbə · '.count($contents).' məzmun'">
        {{ $workspaceName }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.workspaces.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
            <x-teacher.button href="{{ route('teacher.workspaces.edit', $workspaceId) }}" variant="ghost" icon="pencil-square">Redaktə</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Toolbar --}}
    <div class="flex flex-wrap items-center gap-2">
        <button id="open-folder-dialog" type="button" class="btn btn-sm btn-ghost border border-base-300">
            <x-icon name="heroicon-o-folder-plus" class="size-4" /> Yeni Qovluq
        </button>
        <button id="open-content-dialog" type="button" class="btn btn-sm btn-primary">
            <x-icon name="heroicon-o-plus" class="size-4" /> Kitabxanadan Məzmun
        </button>
        <button id="open-student-dialog" type="button" class="btn btn-sm btn-ghost border border-base-300">
            <x-icon name="heroicon-o-user-plus" class="size-4" /> Tələbə Əlavə Et
        </button>
    </div>

    {{-- Kataloq (JS fragment ilə yenilənir) --}}
    <div id="directory" class="space-y-4">
        @include('teacher.workspaces._directory', [
            'workspaceId' => $workspaceId,
            'parentId' => $parentId,
            'folders' => $folders,
            'contents' => $contents,
            'breadcrumbs' => $breadcrumbs,
            'folderTree' => $folderTree,
        ])
    </div>

    {{-- Students --}}
    <div class="space-y-3">
        <h3 class="flex items-center gap-2 text-lg font-semibold text-base-content">
            Tələbələr
            <x-teacher.badge color="blue">{{ count($students) }}</x-teacher.badge>
        </h3>

        @if (count($students) === 0)
            <x-teacher.empty-state icon="user-group" title="Tələbə yoxdur" description="Bu workspace-ə tələbə əlavə edin." />
        @else
            <x-teacher.card :padding="false">
                <x-teacher.table :headers="['Ad', 'E-poçt', '']">
                    @foreach ($students as $student)
                        <tr>
                            <td class="font-medium text-base-content">{{ $student['name'] }}</td>
                            <td class="text-base-content/70">{{ $student['email'] }}</td>
                            <td class="text-right">
                                <button
                                    data-student-detach
                                    data-student-id="{{ $student['id'] }}"
                                    data-student-name="{{ $student['name'] }}"
                                    title="Çıxar"
                                    class="rounded-lg p-1.5 text-error/70 hover:bg-error/10 hover:text-error"
                                >
                                    <x-icon name="heroicon-o-user-minus" class="size-4" />
                                </button>
                            </td>
                        </tr>
                    @endforeach
                </x-teacher.table>
            </x-teacher.card>
        @endif
    </div>

    {{-- Rename dialog --}}
    <div id="rename-dialog" class="fixed inset-0 z-50 hidden flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Adını dəyiş</h3>
            <input
                id="rename-input"
                type="text"
                class="input input-bordered w-full text-sm"
                placeholder="Yeni ad"
            />
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" data-close-dialog="rename-dialog" class="btn btn-sm btn-ghost">Ləğv et</button>
                <button type="button" id="save-rename-btn" class="btn btn-sm btn-primary">Saxla</button>
            </div>
        </div>
    </div>

    {{-- Move dialog --}}
    <div id="move-dialog" class="fixed inset-0 z-50 hidden flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Daşı</h3>
            <select id="move-select" class="select select-bordered w-full text-sm"></select>
            <p class="mt-2 text-xs text-base-content/50">Hədəf qovluğa daşımaq üçün seçin; boş qalsa kökə daşınır.</p>
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" data-close-dialog="move-dialog" class="btn btn-sm btn-ghost">Ləğv et</button>
                <button type="button" id="save-move-btn" class="btn btn-sm btn-primary">Daşı</button>
            </div>
        </div>
    </div>

    {{-- Folder dialog --}}
    <div id="folder-dialog" class="fixed inset-0 z-50 hidden flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Yeni Qovluq</h3>
            <input
                id="folder-name-input"
                type="text"
                placeholder="Qovluq adı"
                class="input input-bordered w-full text-sm"
            />
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" data-close-dialog="folder-dialog" class="btn btn-sm btn-ghost">Ləğv et</button>
                <button type="button" id="save-folder-btn" class="btn btn-sm btn-primary">Yarat</button>
            </div>
        </div>
    </div>

    {{-- Content dialog --}}
    <div id="content-dialog" class="fixed inset-0 z-50 hidden flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Kitabxanadan Məzmun</h3>
            <select id="content-select" class="select select-bordered w-full text-sm">
                <option value="">Seçin...</option>
                @foreach ($contentOptions as $value => $label)
                    <option value="{{ $value }}">{{ $label }}</option>
                @endforeach
            </select>
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" data-close-dialog="content-dialog" class="btn btn-sm btn-ghost">Ləğv et</button>
                <button type="button" id="save-content-btn" class="btn btn-sm btn-primary">Əlavə Et</button>
            </div>
        </div>
    </div>

    {{-- Student dialog --}}
    <div id="student-dialog" class="fixed inset-0 z-50 hidden flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Tələbə Əlavə Et</h3>
            <select id="student-select" multiple class="select select-bordered h-40 w-full text-sm">
                @foreach ($availableStudents as $value => $label)
                    <option value="{{ $value }}">{{ $label }}</option>
                @endforeach
            </select>
            @if (count($availableStudents) === 0)
                <p class="mt-2 text-sm text-base-content/60">Əlavə edilə bilən tələbə yoxdur.</p>
            @endif
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" data-close-dialog="student-dialog" class="btn btn-sm btn-ghost">Ləğv et</button>
                <button type="button" id="save-students-btn" class="btn btn-sm btn-primary">Əlavə Et</button>
            </div>
        </div>
    </div>
</div>
@endsection

@push('scripts')
<script>
(function () {
    const root = document.getElementById('workspace-show');
    if (!root) return;

    const workspaceId = root.dataset.workspaceId;
    const parentId = root.dataset.parentId ? Number(root.dataset.parentId) : null;
    const api = '/api/v1/workspaces/' + workspaceId;
    const fragmentUrl = root.dataset.fragmentUrl;
    const dirEl = document.getElementById('directory');

    const dialogs = {
        rename: document.getElementById('rename-dialog'),
        move: document.getElementById('move-dialog'),
        folder: document.getElementById('folder-dialog'),
        content: document.getElementById('content-dialog'),
        student: document.getElementById('student-dialog'),
    };
    const show = (el) => el.classList.remove('hidden');
    const hide = (el) => el.classList.add('hidden');

    let moveTargetId = null;
    let renameTargetId = null;

    async function refreshDirectory() {
        try {
            dirEl.innerHTML = await KelaFragment(fragmentUrl);
            syncMoveOptions();
        } catch (err) { window.alert(err.message); }
    }

    function syncMoveOptions() {
        const tpl = document.getElementById('folder-tree-template');
        const select = document.getElementById('move-select');
        if (!tpl || !select) return;
        const options = Array.from(tpl.content.querySelectorAll('option')).map(o => o.cloneNode(true));
        select.innerHTML = '<option value="">Kök qovluğa</option>';
        options.forEach(o => select.appendChild(o));
    }

    // Node actions (delegasiya — fragment yenilənsə də işləyir)
    document.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-node-action]');
        if (!btn || !dirEl.contains(btn)) return;
        e.preventDefault();

        const action = btn.dataset.nodeAction;
        const id = btn.dataset.nodeId;

        if (action === 'rename') {
            renameTargetId = Number(id);
            const input = document.getElementById('rename-input');
            input.value = btn.dataset.nodeName || '';
            show(dialogs.rename);
            input.focus();
        } else if (action === 'move') {
            moveTargetId = Number(id);
            syncMoveOptions();
            show(dialogs.move);
        } else if (action === 'delete') {
            const name = btn.dataset.nodeName || 'Element';
            if (!window.confirm(`'${name}' silinsin?`)) return;
            try {
                await KelaApi('DELETE', `${api}/nodes/${id}`);
                await refreshDirectory();
            } catch (err) { window.alert(err.message); }
        }
    });

    // Tələbə çıxar (şagird siyahısı kataloqdan ayrıdır → səhifə yenilənir)
    document.addEventListener('click', async (e) => {
        const btn = e.target.closest('[data-student-detach]');
        if (!btn) return;
        if (!window.confirm(`'${btn.dataset.studentName}' workspace-dən çıxarılsın?`)) return;
        try {
            await KelaApi('DELETE', `${api}/students/${btn.dataset.studentId}`);
            window.location.reload();
        } catch (err) { window.alert(err.message); }
    });

    document.getElementById('save-rename-btn').addEventListener('click', async () => {
        const name = document.getElementById('rename-input').value.trim();
        if (!name) return;
        try {
            await KelaApi('POST', `${api}/nodes/${renameTargetId}/rename`, { name });
            hide(dialogs.rename);
            await refreshDirectory();
        } catch (err) { window.alert(err.message); }
    });

    document.getElementById('save-move-btn').addEventListener('click', async () => {
        const selected = document.getElementById('move-select').value;
        try {
            await KelaApi('POST', `${api}/nodes/${moveTargetId}/move`, { parent_id: selected ? Number(selected) : null });
            hide(dialogs.move);
            await refreshDirectory();
        } catch (err) { window.alert(err.message); }
    });

    document.getElementById('save-folder-btn').addEventListener('click', async () => {
        const name = document.getElementById('folder-name-input').value.trim();
        if (!name) return;
        try {
            await KelaApi('POST', `${api}/folders`, { name, parent_id: parentId });
            hide(dialogs.folder);
            document.getElementById('folder-name-input').value = '';
            await refreshDirectory();
        } catch (err) { window.alert(err.message); }
    });

    document.getElementById('save-content-btn').addEventListener('click', async () => {
        const select = document.getElementById('content-select');
        if (!select.value) { window.alert('Məzmun seçin.'); return; }
        try {
            await KelaApi('POST', `${api}/contents`, { content_id: Number(select.value), parent_id: parentId });
            hide(dialogs.content);
            select.value = '';
            await refreshDirectory();
        } catch (err) { window.alert(err.message); }
    });

    document.getElementById('save-students-btn').addEventListener('click', async () => {
        const select = document.getElementById('student-select');
        const ids = Array.from(select.selectedOptions).map(o => Number(o.value));
        if (!ids.length) { window.alert('Tələbə seçin.'); return; }
        try {
            await KelaApi('POST', `${api}/students`, { student_ids: ids });
            window.location.reload();
        } catch (err) { window.alert(err.message); }
    });

    document.getElementById('open-folder-dialog').addEventListener('click', () => {
        show(dialogs.folder);
        document.getElementById('folder-name-input').focus();
    });
    document.getElementById('open-content-dialog').addEventListener('click', () => show(dialogs.content));
    document.getElementById('open-student-dialog').addEventListener('click', () => show(dialogs.student));

    document.querySelectorAll('[data-close-dialog]').forEach(btn => {
        btn.addEventListener('click', () => hide(dialogs[btn.dataset.closeDialog]));
    });

    document.addEventListener('keydown', (e) => {
        if (e.key !== 'Escape') return;
        Object.values(dialogs).forEach(hide);
    });
})();
</script>
@endpush
