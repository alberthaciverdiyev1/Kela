@extends('common.layouts.teacher')
@section('title', 'Siniflər - Kela')
@section('content')
<div class="space-y-6" x-data="workspaceList({{ \Illuminate\Support\Js::from(['autoCreate' => (bool) request('create')]) }})" @keydown.escape.window="closeAll()">
    <x-teacher.heading subtitle="İş sahələrini idarə et">
        Siniflər
        <x-slot:actions>
            <x-teacher.button icon="plus" @click="openCreate()">Yeni Sinif</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Görünüm keçidi: List / Grid --}}
    <div class="flex flex-wrap items-center justify-between gap-3">
        <div></div>
        <div class="inline-flex items-center rounded-lg border border-base-300 bg-base-100 p-0.5">
            <button
                type="button"
                @click="setViewMode('list')"
                class="inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition"
                :class="viewMode === 'list' ? 'bg-primary/10 text-primary' : 'text-base-content/60 hover:text-base-content'"
                title="Liste görünümü"
            >
                <x-icon name="heroicon-o-list-bullet" class="size-4" /> List
            </button>
            <button
                type="button"
                @click="setViewMode('grid')"
                class="inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition"
                :class="viewMode === 'grid' ? 'bg-primary/10 text-primary' : 'text-base-content/60 hover:text-base-content'"
                title="Grid görünümü"
            >
                <x-icon name="heroicon-o-squares-2x2" class="size-4" /> Grid
            </button>
        </div>
    </div>

    <x-teacher.card :padding="false">
        <form method="GET" action="{{ route('teacher.workspaces.index') }}" class="flex items-center gap-3 border-b border-base-300 p-4">
            <div class="relative w-72">
                <span class="pointer-events-none absolute inset-y-0 left-3 flex items-center">
                    <x-icon name="heroicon-o-magnifying-glass" class="size-4 text-base-content/40" />
                </span>
                <x-teacher.input name="search" value="{{ $search }}" placeholder="Ad ilə axtar..." class="pl-9" />
            </div>
            @if ($search !== '')
                <a href="{{ route('teacher.workspaces.index') }}" class="btn btn-ghost btn-sm">Təmizlə</a>
            @endif
        </form>

        @if ($workspaces->isEmpty())
            <x-teacher.empty-state icon="building-office-2" title="Sinif tapılmadı" description="Axtarışı dəyişin və ya yeni sinif yaradın." />
        @else
            {{-- List görünümü (tablo) --}}
            <div x-show="viewMode === 'list'">
                <x-teacher.table :headers="['Ad', 'Tələbə', 'Yaradılıb']">
                    @foreach ($workspaces as $ws)
                        <tr
                            class="cursor-context-menu transition hover:bg-base-200/50"
                            data-workspace-id="{{ $ws['id'] }}"
                            data-workspace-name="{{ $ws['name'] }}"
                            data-open-url="{{ route('teacher.workspaces.show', $ws['id']) }}"
                            data-edit-url="{{ route('teacher.workspaces.edit', $ws['id']) }}"
                            @contextmenu.prevent="openWorkspaceContextMenu($event, $el)"
                        >
                            <td class="font-medium text-base-content">
                                <a href="{{ route('teacher.workspaces.show', $ws['id']) }}" class="hover:text-primary">{{ $ws['name'] }}</a>
                            </td>
                            <td class="text-base-content/70">
                                <x-teacher.badge color="blue">{{ $ws['student_count'] }} şagird</x-teacher.badge>
                            </td>
                            <td class="text-base-content/70">{{ dash($ws['created_at']) }}</td>
                        </tr>
                    @endforeach
                </x-teacher.table>
            </div>

            {{-- Grid görünümü (dikey kartlar — 6 sütun, kompakt, böyük ikon) --}}
            <div x-show="viewMode === 'grid'" class="grid grid-cols-6 gap-2 p-3">
                @foreach ($workspaces as $ws)
                    <div
                        class="group relative flex cursor-context-menu flex-col items-center rounded-lg border border-base-300 bg-base-100 p-2 text-center transition hover:border-primary/40 hover:shadow-sm"
                        data-workspace-id="{{ $ws['id'] }}"
                        data-workspace-name="{{ $ws['name'] }}"
                        data-open-url="{{ route('teacher.workspaces.show', $ws['id']) }}"
                        data-edit-url="{{ route('teacher.workspaces.edit', $ws['id']) }}"
                        @contextmenu.prevent="openWorkspaceContextMenu($event, $el)"
                    >
                        <a href="{{ route('teacher.workspaces.show', $ws['id']) }}" class="relative flex size-20 items-center justify-center rounded-xl bg-primary/10 text-primary transition group-hover:bg-primary/15">
                            <x-icon name="heroicon-o-building-office-2" class="size-[66px]" />
                            @if ($ws['student_count'] > 0)
                                <span class="absolute -top-2.5 -right-2.5 flex size-7 items-center justify-center rounded-full bg-primary text-xs font-bold leading-none text-primary-content shadow-sm ring-2 ring-base-100">
                                    {{ $ws['student_count'] }}
                                </span>
                            @endif
                        </a>
                        <a href="{{ route('teacher.workspaces.show', $ws['id']) }}" class="mt-1.5 block w-full truncate text-xs font-medium leading-tight text-base-content transition hover:text-primary" title="{{ $ws['name'] }}">
                            {{ $ws['name'] }}
                        </a>
                    </div>
                @endforeach
            </div>
            <x-teacher.pagination :paginator="$workspaces" />
        @endif
    </x-teacher.card>

    {{-- Yeni Sinif dialog --}}
    <x-teacher.modal show="showCreate" title="Yeni Sinif" maxWidth="md">
        <form method="POST" action="{{ route('teacher.workspaces.store') }}" id="create-workspace-form">
            @csrf
            <div class="flex items-start gap-4">
                <span class="flex size-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
                    <x-icon name="heroicon-o-building-office-2" class="size-6" />
                </span>
                <div class="flex-1">
                    <x-teacher.field label="Ad" name="name" :required="true" hint="Məs: Sinif 3A, Riyaziyyat qrupu...">
                        <x-teacher.input name="name" placeholder="Sinif adı" required autofocus />
                    </x-teacher.field>
                </div>
            </div>
        </form>
        <x-slot:footer>
            <button type="button" class="btn btn-ghost" @click="showCreate = false">Ləğv et</button>
            <x-teacher.button type="submit" form="create-workspace-form" icon="plus">Yarat</x-teacher.button>
        </x-slot:footer>
    </x-teacher.modal>

    {{-- Sağ-tık kontekst menyusu --}}
    @include('teacher.partials._context-menu')
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/workspace/controller.js')
@endpush
