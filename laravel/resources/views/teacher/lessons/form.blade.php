@extends('layouts.teacher')
@section('title', $heading)
@section('content')
<div class="mx-auto max-w-2xl space-y-6">
    <x-teacher.heading :subtitle="$subtitle">
        {{ $heading }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.lessons.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <x-teacher.card>
        <form
            method="POST"
            action="{{ $creating ? route('teacher.lessons.store') : route('teacher.lessons.update', $lesson['content_id']) }}"
            enctype="multipart/form-data"
            class="grid gap-5"
        >
            @csrf

            <x-teacher.field label="Başlıq" name="title" :required="true">
                <x-teacher.input name="title" value="{{ old('title', $lesson['title'] ?? '') }}" />
            </x-teacher.field>

            <x-teacher.field label="Təsvir" name="description">
                <x-teacher.textarea name="description" rows="4">{{ old('description', $lesson['description'] ?? '') }}</x-teacher.textarea>
            </x-teacher.field>

            <x-teacher.field
                label="Video faylı"
                name="video"
                hint="Maksimum 512 MB — MP4, WebM, MKV, AVI, MOV. Yüklənməsə video olmadan saxlanır."
            >
                <input
                    type="file"
                    name="video"
                    accept="video/mp4,video/webm,video/ogg,video/quicktime,video/x-m4v,video/x-matroska,video/x-msvideo,video/mpeg"
                    class="file-input file-input-bordered w-full text-sm"
                />
                @if (! $creating && ! empty($lesson['video_path']))
                    <p class="mt-2 flex items-center gap-1 text-xs text-base-content/60">
                        <x-icon name="heroicon-o-video-camera" class="size-3.5" />
                        Mövcud video qorunur — yalnız yeni fayl seçilsə dəyişir.
                    </p>
                @endif
            </x-teacher.field>

            <div class="grid gap-5 sm:grid-cols-2">
                <x-teacher.field label="Sıra" name="order_index" hint="Listede sıralama üçün.">
                    <x-teacher.input name="order_index" type="number" min="0" value="{{ old('order_index', $lesson['order_index'] ?? 0) }}" />
                </x-teacher.field>

                <x-teacher.field label="Yayımlandı" name="is_published">
                    <label class="flex items-center gap-2 pt-2">
                        <input
                            type="checkbox"
                            name="is_published"
                            value="1"
                            @checked(old('is_published', $lesson['is_published'] ?? false))
                            class="toggle toggle-primary"
                        />
                        <span class="text-sm text-base-content/70">Yayımlanmış dərs kimi işarələ</span>
                    </label>
                </x-teacher.field>
            </div>

            <div class="flex items-center justify-end gap-2 border-t border-base-300 pt-5">
                <x-teacher.button type="submit" icon="check">Yadda Saxla</x-teacher.button>
            </div>
        </form>
    </x-teacher.card>
</div>
@endsection
