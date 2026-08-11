@extends('layouts.teacher')
@section('title', 'Şagirdlər - Kela')
@section('content')
<div class="space-y-6">
    <x-teacher.heading subtitle="Şagirdləri idarə et">
        Şagirdlər
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.students.create') }}" icon="plus">Yeni Şagird</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <x-teacher.card :padding="false">
        <form method="GET" action="{{ route('teacher.students.index') }}" class="flex items-center gap-3 border-b border-base-300 p-4">
            <div class="relative w-72">
                <span class="pointer-events-none absolute inset-y-0 left-3 flex items-center">
                    <x-icon name="heroicon-o-magnifying-glass" class="size-4 text-base-content/40" />
                </span>
                <x-teacher.input name="search" value="{{ $search }}" placeholder="Ad, soyad, e-poçt..." class="pl-9" />
            </div>
            @if ($search !== '')
                <a href="{{ route('teacher.students.index') }}" class="btn btn-ghost btn-sm">Təmizlə</a>
            @endif
        </form>

        @if ($students->isEmpty())
            <x-teacher.empty-state icon="user-group" title="Şagird tapılmadı" description="Axtarışı dəyişin və ya yeni şagird əlavə edin." />
        @else
            <x-teacher.table :headers="['Ad Soyad', 'E-poçt', 'Şəhər', 'Doğum', 'Status', '']">
                @foreach ($students as $student)
                    <tr class="transition hover:bg-base-200/50">
                        <td class="font-medium text-base-content">{{ $student['full_name'] }}</td>
                        <td class="text-base-content/70">{{ $student['email'] }}</td>
                        <td class="text-base-content/70">{{ $student['city'] ?? '—' }}</td>
                        <td class="text-base-content/70">{{ $student['birth_date'] ?? '—' }}</td>
                        <td>
                            <x-teacher.badge :color="match ($student['status']) { 1 => 'green', 2 => 'yellow', 3 => 'red', default => 'gray' }">
                                {{ match ($student['status']) { 1 => 'Aktiv', 2 => 'Deaktiv', 3 => 'Dayandırılmış', default => $student['status'] } }}
                            </x-teacher.badge>
                        </td>
                        <td class="text-right">
                            <div class="flex items-center justify-end gap-1">
                                <x-teacher.button href="{{ route('teacher.students.edit', $student['id']) }}" variant="ghost" size="sm" icon="pencil-square">Redaktə</x-teacher.button>
                                <form
                                    method="POST"
                                    action="{{ route('teacher.students.destroy', $student['id']) }}"
                                    data-api-delete="/api/v1/students/{{ $student['id'] }}"
                                    data-confirm="Bu şagirdi silmək istəyirsiniz?"
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
            <x-teacher.pagination :paginator="$students" />
        @endif
    </x-teacher.card>
</div>
@endsection
