@extends('layouts.teacher')
@section('title', 'İş Sahələri - Kela')
@section('content')
<div class="space-y-6">
    <x-teacher.heading subtitle="İş sahələrini idarə et">
        İş Sahələri
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.workspaces.create') }}" icon="plus">Yeni Workspace</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

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
            <x-teacher.empty-state icon="squares-2x2" title="Workspace tapılmadı" description="Axtarışı dəyişin və ya yeni workspace yaradın." />
        @else
            <x-teacher.table :headers="['Ad', 'Tələbə', 'Məzmun', 'Yaradılıb', '']">
                @foreach ($workspaces as $ws)
                    <tr class="transition hover:bg-base-200/50">
                        <td class="font-medium text-base-content">
                            <a href="{{ route('teacher.workspaces.show', $ws['id']) }}" class="hover:text-primary">{{ $ws['name'] }}</a>
                        </td>
                        <td class="text-base-content/70">
                            <x-teacher.badge color="blue">{{ $ws['student_count'] }} şagird</x-teacher.badge>
                        </td>
                        <td class="text-base-content/70">{{ $ws['content_count'] }} məzmun</td>
                        <td class="text-base-content/70">{{ $ws['created_at'] ?? '—' }}</td>
                        <td class="text-right">
                            <div class="flex items-center justify-end gap-1">
                                <x-teacher.button href="{{ route('teacher.workspaces.show', $ws['id']) }}" variant="ghost" size="sm" icon="eye">Aç</x-teacher.button>
                                <x-teacher.button href="{{ route('teacher.workspaces.edit', $ws['id']) }}" variant="ghost" size="sm" icon="pencil-square">Redaktə</x-teacher.button>
                                <form
                                    method="POST"
                                    action="{{ route('teacher.workspaces.destroy', $ws['id']) }}"
                                    data-api-delete="/api/v1/workspaces/{{ $ws['id'] }}"
                                    data-confirm="Bu workspace silinsin?"
                                >
                                    @csrf
                                    @method('DELETE')
                                    <x-teacher.button type="submit" variant="ghost" size="sm" icon="trash">Sil</x-teacher.button>
                                </form>
                            </div>
                        </td>
                    </tr>
                @endforeach
            </x-teacher.table>
            <x-teacher.pagination :paginator="$workspaces" />
        @endif
    </x-teacher.card>
</div>
@endsection
