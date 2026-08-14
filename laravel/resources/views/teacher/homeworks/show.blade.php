@extends('common.layouts.teacher')
@section('title', $homework->title.' - Kela')
@section('content')
@php
    $total = count($questions);
@endphp
<div class="mx-auto max-w-3xl space-y-6">
    <x-teacher.heading :subtitle="$homework->description">
        {{ $homework->title }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.homeworks.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
            <x-teacher.button href="{{ route('teacher.homeworks.edit', $homework->id) }}" variant="ghost" icon="pencil-square">Redaktə</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <div class="flex flex-wrap items-center gap-2">
        <x-teacher.badge color="{{ $homework->is_published ? 'green' : 'yellow' }}">
            {{ $homework->is_published ? 'Yayımlandı' : 'Qaralama' }}
        </x-teacher.badge>
        <x-teacher.badge color="blue">{{ $total }} sual</x-teacher.badge>
        <span class="text-sm text-base-content/50">Yaradılıb: {{ $homework->created_at?->format('d M Y H:i') }}</span>
    </div>

    @if ($total === 0)
        <x-teacher.empty-state icon="clipboard-document" title="Hələ sual yoxdur" description="Ev tapşırığına sual əlavə etmək üçün Redaktə edin." />
    @else
        <x-teacher.card :padding="false">
            <div class="divide-y divide-base-200">
                @foreach ($questions as $index => $q)
                    @php
                        $isQuiz = $q['type'] === \App\Domain\Homework\Values\HomeworkQuestionType::QUIZ;
                    @endphp
                    <div class="flex items-start gap-4 px-5 py-4">
                        <span class="mt-0.5 flex size-7 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-bold text-primary">
                            {{ $index + 1 }}
                        </span>
                        <div class="min-w-0 flex-1">
                            <div class="flex flex-wrap items-center gap-1.5">
                                <x-teacher.badge :color="$isQuiz ? 'blue' : 'gray'">{{ $isQuiz ? 'Quiz sualı' : 'Tapşırıq' }}</x-teacher.badge>
                                @if ($isQuiz && $q['correct_option'] !== null)
                                    <x-teacher.badge color="green">Doğru: {{ chr(65 + $q['correct_option']) }}</x-teacher.badge>
                                @endif
                            </div>
                            <div class="rich-preview mt-2 text-sm text-base-content">{!! $q['text'] !!}</div>
                            @if ($isQuiz && count($q['options']) > 0)
                                <div class="mt-3 space-y-1.5">
                                    @foreach ($q['options'] as $letter => $option)
                                        <div class="flex items-center gap-2 rounded-lg border border-base-200 bg-base-50 px-3 py-2 text-sm text-base-content/80">
                                            <span class="flex size-5 shrink-0 items-center justify-center rounded-full bg-base-200 text-xs font-bold text-base-content/70">{{ $letter }}</span>
                                            {{ $option }}
                                        </div>
                                    @endforeach
                                </div>
                            @endif
                        </div>
                    </div>
                @endforeach
            </div>
        </x-teacher.card>
    @endif
</div>
@endsection
