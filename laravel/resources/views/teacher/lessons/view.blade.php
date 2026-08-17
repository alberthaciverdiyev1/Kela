@extends('common.layouts.teacher')
@section('title', $lessonData['title'].' - Kela')
@section('content')
<div class="space-y-6">
    <x-teacher.heading :subtitle="$lessonData['created_at'] ?? ''">
        {{ $lessonData['title'] }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.lessons.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
            <x-teacher.button href="{{ route('teacher.lessons.edit', $contentId) }}" variant="ghost" icon="pencil-square">Redaktə</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <x-teacher.card :padding="false">
        <div class="p-6">
            <x-teacher.video-player :has-video="$hasVideo" :stream-url="$streamUrl" :thumb-url="$thumbUrl" />
        </div>
        <div class="grid gap-4 border-t border-base-300 p-6 sm:grid-cols-3">
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Yayım</p>
                <x-teacher.badge :color="$lessonData['is_published'] ? 'green' : 'yellow'" class="mt-1">
                    {{ $lessonData['is_published'] ? 'Yayımlandı' : 'Qaralama' }}
                </x-teacher.badge>
            </div>
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Müddət</p>
                <p class="mt-1 text-sm font-medium text-base-content">{{ $lessonData['duration_label'] }}</p>
            </div>
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Sıra</p>
                <p class="mt-1 text-sm font-medium text-base-content">{{ $lessonData['order_index'] }}</p>
            </div>
        </div>
        @if ($lessonData['description'])
            <div class="border-t border-base-300 p-6">
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Təsvir</p>
                <p class="mt-2 text-sm whitespace-pre-wrap text-base-content/80">{{ $lessonData['description'] }}</p>
            </div>
        @endif
    </x-teacher.card>
</div>
@endsection
