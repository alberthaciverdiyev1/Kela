@extends('common.layouts.teacher')
@section('title', $heading)
@section('content')
<div class="mx-auto max-w-2xl space-y-6">
    <x-teacher.heading :subtitle="$subtitle">
        {{ $heading }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.quizzes.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <x-teacher.card>
        <form
            method="POST"
            action="{{ $creating ? route('teacher.quizzes.store') : route('teacher.quizzes.update', $quiz['content_id']) }}"
            class="grid gap-5"
        >
            @csrf

            <x-teacher.field label="Başlıq" name="title" :required="true">
                <x-teacher.input name="title" value="{{ old('title', $quiz['title'] ?? '') }}" />
            </x-teacher.field>

            <x-teacher.field label="Təsvir" name="description">
                <x-teacher.textarea name="description" rows="3">{{ old('description', $quiz['description'] ?? '') }}</x-teacher.textarea>
            </x-teacher.field>

            <x-teacher.field label="Yayımlandı" name="is_published">
                <label class="flex items-center gap-2 pt-2">
                    <input
                        type="checkbox"
                        name="is_published"
                        value="1"
                        @checked(old('is_published', $quiz['is_published'] ?? false))
                        class="toggle toggle-primary"
                    />
                    <span class="text-sm text-base-content/70">Yayımlanmış quiz kimi işarələ</span>
                </label>
            </x-teacher.field>

            <div class="flex items-center justify-end gap-2 border-t border-base-300 pt-5">
                <x-teacher.button type="submit" icon="check">Yadda Saxla</x-teacher.button>
            </div>
        </form>
    </x-teacher.card>
</div>
@endsection
