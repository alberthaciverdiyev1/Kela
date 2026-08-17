@extends('common.layouts.teacher')
@section('title', $heading)
@section('content')
<div class="space-y-6">
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

            <x-teacher.field
                label="Aylıq ödəniş (AZN)"
                name="monthly_price"
                :hint="'Sinif üzrə standart aylıq qiymət. Şagirdə xüsusi qiymət fərqləndirilə bilər; boş qalsa qaimədə 0 göstərilir.'"
            >
                <x-teacher.input name="monthly_price" type="number" step="0.01" min="0"
                                 value="{{ old('monthly_price', $workspace['monthly_price'] ?? '') }}"
                                 placeholder="Məs: 50.00" />
            </x-teacher.field>

            <div class="flex items-center justify-end gap-2 border-t border-base-300 pt-5">
                <x-teacher.button type="submit" icon="check">Yadda Saxla</x-teacher.button>
            </div>
        </form>
    </x-teacher.card>
</div>
@endsection
