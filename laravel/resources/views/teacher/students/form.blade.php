@extends('common.layouts.teacher')
@section('title', $heading)
@section('content')
<div class="space-y-6">
    <x-teacher.heading :subtitle="$subtitle">
        {{ $heading }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.students.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <x-teacher.card>
        <form
            method="POST"
            action="{{ $creating ? route('teacher.students.store') : route('teacher.students.update', $student['id']) }}"
            class="grid gap-5 sm:grid-cols-2"
        >
            @csrf

            <x-teacher.field label="Ad" name="first_name" :required="true">
                <x-teacher.input name="first_name" value="{{ old('first_name', $student['first_name'] ?? '') }}" />
            </x-teacher.field>

            <x-teacher.field label="Soyad" name="last_name">
                <x-teacher.input name="last_name" value="{{ old('last_name', $student['last_name'] ?? '') }}" />
            </x-teacher.field>

            <x-teacher.field label="E-poçt" name="email" :required="true" class="sm:col-span-2">
                <x-teacher.input name="email" type="email" value="{{ old('email', $student['email'] ?? '') }}" />
            </x-teacher.field>

            <x-teacher.field
                label="Şifrə"
                name="password"
                :required="$creating"
                :hint="$creating ? null : 'Boş buraxılsa dəyişməz.'"
                class="sm:col-span-2"
            >
                <x-teacher.input name="password" type="password" autocomplete="new-password" />
            </x-teacher.field>

            <x-teacher.field label="Şəhər" name="city_id">
                <x-teacher.select name="city_id" :options="$cities" placeholder="Seçin..." :selected="old('city_id', $student['city_id'] ?? '')" />
            </x-teacher.field>

            <x-teacher.field label="Doğum tarixi" name="birth_date">
                <x-teacher.input name="birth_date" type="date" value="{{ old('birth_date', $student['birth_date'] ?? '') }}" />
            </x-teacher.field>

            <x-teacher.field label="Status" name="status">
                <x-teacher.select name="status" :options="$statuses" :selected="old('status', $student['status'] ?? 1)" />
            </x-teacher.field>

            <div class="flex items-center justify-end gap-2 border-t border-base-300 pt-5 sm:col-span-2">
                <x-teacher.button type="submit" icon="check">Yadda Saxla</x-teacher.button>
            </div>
        </form>
    </x-teacher.card>
</div>
@endsection
