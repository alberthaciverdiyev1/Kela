@extends('common.layouts.teacher')
@section('title', 'Sual Detayı - Kela')
@section('content')
@php
    $options = $question->options();
    $correct = chr(65 + $question->correct_option);
@endphp
<div class="space-y-6">
    {{-- Başlıq --}}
    <x-teacher.heading subtitle="{{ $question->created_at?->format('d.m.Y H:i') }}">
        Sual Detayı
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.questions.index', ['folder_id' => $question->folder_id ?? null]) }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <div class="grid gap-6 xl:grid-cols-3">
        {{-- Sol: sual kartı --}}
        <div class="space-y-6 xl:col-span-2">
            <x-teacher.card>
                <div class="mb-4 flex flex-wrap items-center justify-between gap-2">
                    <div class="flex flex-wrap items-center gap-2">
                        <x-teacher.badge color="primary">Sual #{{ $question->id }}</x-teacher.badge>
                        @if ($question->folder)
                            <x-teacher.badge color="gray">
                                <span class="inline-flex items-center gap-1">
                                    <x-icon name="heroicon-o-folder" class="size-3" />
                                    {{ $question->folder->name }}
                                </span>
                            </x-teacher.badge>
                        @else
                            <x-teacher.badge color="gray">Kök</x-teacher.badge>
                        @endif
                        @if ($usedInQuizzes > 0)
                            <x-teacher.badge color="blue">{{ $usedInQuizzes }} quiz</x-teacher.badge>
                        @endif
                    </div>
                </div>

                {{-- Sual mətni --}}
                <div class="rich-preview rounded-xl border border-base-300 bg-base-200/40 p-5 text-base leading-relaxed text-base-content">
                    {!! $question->text !!}
                </div>

                {{-- Seçimlər --}}
                <div class="mt-5 space-y-2.5">
                    @foreach ($options as $letter => $option)
                        @php
                            $isCorrect = $letter === $correct;
                        @endphp
                        <div @class([
                            'flex items-start gap-3 rounded-xl border p-3.5 transition',
                            'border-success/40 bg-success/10' => $isCorrect,
                            'border-base-300 bg-base-100' => ! $isCorrect,
                        ])>
                            <span @class([
                                'flex size-7 shrink-0 items-center justify-center rounded-full text-sm font-bold',
                                'bg-success text-success-content' => $isCorrect,
                                'bg-base-200 text-base-content/70' => ! $isCorrect,
                            ])>
                                {{ $letter }}
                            </span>
                            <span class="pt-0.5 text-sm text-base-content/90">
                                {{ $option }}
                                @if ($isCorrect)
                                    <span class="ml-2 inline-flex items-center gap-1 rounded-full bg-success/15 px-2 py-0.5 text-xs font-semibold text-success">
                                        <x-icon name="heroicon-o-check-circle" class="size-3.5" /> Doğru cavab
                                    </span>
                                @endif
                            </span>
                        </div>
                    @endforeach
                </div>

                {{-- İzah --}}
                @if ($question->explanation)
                    <div class="mt-5 flex items-start gap-3 rounded-xl border border-warning/30 bg-warning/10 p-4">
                        <x-icon name="heroicon-o-light-bulb" class="mt-0.5 size-5 shrink-0 text-warning" />
                        <div>
                            <p class="text-sm font-semibold text-warning">İzah</p>
                            <p class="mt-1 text-sm leading-relaxed text-base-content/80">{{ $question->explanation }}</p>
                        </div>
                    </div>
                @endif
            </x-teacher.card>
        </div>

        {{-- Sağ: yan panel --}}
        <div class="space-y-6">
            <x-teacher.card>
                <h3 class="text-sm font-semibold text-base-content">Məlumat</h3>
                <dl class="mt-4 space-y-3 text-sm">
                    <div class="flex items-center justify-between">
                        <dt class="text-base-content/60">Doğru cavab</dt>
                        <dd>
                            <x-teacher.badge color="green">{{ $correct }}</x-teacher.badge>
                        </dd>
                    </div>
                    <div class="flex items-center justify-between">
                        <dt class="text-base-content/60">Seçim sayı</dt>
                        <dd class="font-semibold text-base-content">{{ count($options) }}</dd>
                    </div>
                    <div class="flex items-center justify-between">
                        <dt class="text-base-content/60">İstifadə olunan quiz</dt>
                        <dd class="font-semibold text-base-content">{{ $usedInQuizzes }}</dd>
                    </div>
                    <div class="flex items-center justify-between">
                        <dt class="text-base-content/60">Yaradılıb</dt>
                        <dd class="text-base-content/80">{{ $question->created_at?->format('d.m.Y H:i') }}</dd>
                    </div>
                </dl>
            </x-teacher.card>

            @if ($usedInQuizzes > 0)
                <x-teacher.card>
                    <h3 class="text-sm font-semibold text-base-content">İstifadə olunduğu quizlər</h3>
                    <ul class="mt-3 space-y-2">
                        @foreach ($question->quizzes()->orderBy('created_at', 'desc')->get() as $quiz)
                            <li>
                                <a href="{{ route('teacher.quizzes.show', $quiz->content_id) }}" class="flex items-center gap-2 rounded-lg px-2 py-1.5 text-sm text-base-content/75 transition hover:bg-primary/10 hover:text-primary">
                                    <x-icon name="heroicon-o-clipboard-document-list" class="size-4 shrink-0 opacity-70" />
                                    {{ $quiz->title }}
                                </a>
                            </li>
                        @endforeach
                    </ul>
                </x-teacher.card>
            @endif
        </div>
    </div>
</div>
@endsection
