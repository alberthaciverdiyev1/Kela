@extends('common.layouts.teacher')
@section('title', $lessonData['title'].' - Kela')
@section('content')
<div class="space-y-6 sm:space-y-8">
    <x-teacher.heading :subtitle="$lessonData['created_at'] ?? ''">
        {{ $lessonData['title'] }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.lessons.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
            <x-teacher.button href="{{ route('teacher.lessons.edit', $contentId) }}" variant="outline" icon="pencil-square">Redaktə</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <div class="grid grid-cols-1 items-start gap-6 lg:grid-cols-3 lg:gap-8">
        {{-- Sol sütün: Video və Təsvir --}}
        <div class="space-y-6 lg:col-span-2">
            <div class="overflow-hidden rounded-2xl shadow-xl ring-1 ring-base-300">
                <x-teacher.video-player :has-video="$hasVideo" :stream-url="$streamUrl" :thumb-url="$thumbUrl" />
            </div>

            @if ($lessonData['description'])
                <x-teacher.card>
                    <h3 class="mb-4 flex items-center gap-2 text-lg font-bold tracking-tight text-base-content">
                        <x-icon name="heroicon-o-document-text" class="size-5 text-primary" />
                        Dərs Təsviri
                    </h3>
                    <div class="whitespace-pre-wrap text-sm leading-relaxed text-base-content/80">
                        {{ $lessonData['description'] }}
                    </div>
                </x-teacher.card>
            @endif
        </div>

        {{-- Sağ sütün: Məlumatlar --}}
        <div class="space-y-6 lg:col-span-1">
            <x-teacher.card>
                <h3 class="mb-5 flex items-center gap-2 text-base font-bold tracking-tight text-base-content">
                    <x-icon name="heroicon-o-information-circle" class="size-5 text-primary" />
                    Məlumatlar
                </h3>
                
                <dl class="space-y-4 text-sm">
                    <div class="flex items-center justify-between border-b border-base-200 pb-4">
                        <dt class="font-medium text-base-content/60">Status</dt>
                        <dd>
                            <x-teacher.badge :color="$lessonData['is_published'] ? 'green' : 'yellow'">
                                {{ $lessonData['is_published'] ? 'Yayımlandı' : 'Qaralama' }}
                            </x-teacher.badge>
                        </dd>
                    </div>
                    
                    <div class="flex items-center justify-between border-b border-base-200 pb-4">
                        <dt class="font-medium text-base-content/60">Müddət</dt>
                        <dd class="font-medium text-base-content">{{ $lessonData['duration_label'] }}</dd>
                    </div>
                    
                    <div class="flex items-center justify-between">
                        <dt class="font-medium text-base-content/60">Sıra</dt>
                        <dd class="font-medium text-base-content">{{ $lessonData['order_index'] }}</dd>
                    </div>
                </dl>
            </x-teacher.card>
        </div>
    </div>
</div>
@endsection
