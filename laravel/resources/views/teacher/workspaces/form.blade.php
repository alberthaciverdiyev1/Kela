@extends('layouts.teacher')
@section('title', $heading)
@section('content')
<div class="mx-auto max-w-2xl space-y-6">
    <x-teacher.heading :subtitle="$subtitle">
        {{ $heading }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.workspaces.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <x-teacher.card>
        <form
            method="POST"
            action="{{ $creating ? route('teacher.workspaces.store') : route('teacher.workspaces.update', $workspace['id']) }}"
            class="space-y-5"
        >
            @csrf

            <x-teacher.field label="Ad" name="name" :required="true">
                <x-teacher.input name="name" value="{{ old('name', $workspace['name'] ?? '') }}" placeholder="Məs: Sinif 3A" />
            </x-teacher.field>

            <div class="flex items-center justify-end gap-2 border-t border-base-300 pt-5">
                <x-teacher.button type="submit" icon="check">Yadda Saxla</x-teacher.button>
            </div>
        </form>
    </x-teacher.card>
</div>
@endsection
