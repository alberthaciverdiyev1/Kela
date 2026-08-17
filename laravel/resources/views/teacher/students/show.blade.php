@extends('common.layouts.teacher')
@section('title', $student['full_name'].' - Kela')
@section('content')
<div class="space-y-6">
    <x-teacher.heading subtitle="{{ $student['email'] }}">
        {{ $student['full_name'] }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.students.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
            <x-teacher.button href="{{ route('teacher.students.edit', $student['id']) }}" variant="ghost" icon="pencil-square">Redaktə</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Profil kartı --}}
    <x-teacher.card :padding="false">
        <div class="flex items-center gap-4 p-6">
            <div class="flex size-16 items-center justify-center rounded-full bg-primary/10 text-xl font-bold text-primary">
                {{ mb_substr($student['first_name'], 0, 1) }}{{ mb_substr($student['last_name'] ?? '', 0, 1) }}
            </div>
            <div>
                <h2 class="text-lg font-bold text-base-content">{{ $student['full_name'] }}</h2>
                <p class="text-sm text-base-content/60">{{ $student['email'] }}</p>
            </div>
            <div class="ms-auto">
                <x-teacher.badge :color="match ($student['status']) { 1 => 'green', 2 => 'yellow', 3 => 'red', default => 'gray' }">
                    {{ $student['status_label'] }}
                </x-teacher.badge>
            </div>
        </div>
        <div class="grid gap-4 border-t border-base-300 p-6 sm:grid-cols-2">
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Ad</p>
                <p class="mt-1 text-sm font-medium text-base-content">{{ $student['first_name'] }}</p>
            </div>
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Soyad</p>
                <p class="mt-1 text-sm font-medium text-base-content">{{ $student['last_name'] ?? '—' }}</p>
            </div>
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Şəhər</p>
                <p class="mt-1 text-sm font-medium text-base-content">{{ $student['city'] }}</p>
            </div>
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Doğum tarixi</p>
                <p class="mt-1 text-sm font-medium text-base-content">{{ $student['birth_date'] ? \Carbon\Carbon::parse($student['birth_date'])->format('d M Y') : '—' }}</p>
            </div>
        </div>
    </x-teacher.card>

    {{-- Üzv olduğu siniflər --}}
    <x-teacher.card :padding="false">
        <div class="flex items-center justify-between border-b border-base-300 p-4">
            <h3 class="text-sm font-semibold text-base-content">Üzv olduğu siniflər</h3>
            <span class="badge badge-outline badge-sm">{{ count($workspaces) }}</span>
        </div>
        @if (count($workspaces) === 0)
            <x-teacher.empty-state icon="briefcase" title="Sinif yoxdur" description="Bu şagird hələ heç bir sinifə əlavə olunmayıb." />
        @else
            <ul class="divide-y divide-base-200">
                @foreach ($workspaces as $ws)
                    <li class="flex items-center justify-between gap-3 p-4">
                        <a href="{{ route('teacher.workspaces.show', $ws['id']) }}" class="inline-flex items-center gap-2 font-medium text-base-content transition hover:text-primary">
                            <x-icon name="heroicon-o-briefcase" class="size-4 text-base-content/40" />
                            {{ $ws['name'] }}
                        </a>
                        <span class="text-xs text-base-content/50">{{ $ws['student_count'] }} şagird</span>
                    </li>
                @endforeach
            </ul>
        @endif
    </x-teacher.card>
</div>
@endsection
