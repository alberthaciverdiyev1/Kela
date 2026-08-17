@extends('common.layouts.teacher')
@section('title', $quizData['title'].' - Kela')
@section('content')
<div class="space-y-6">
    <x-teacher.heading subtitle="{{ $quizData['created_at'] ?? '' }}">
        {{ $quizData['title'] }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.quizzes.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
            <x-teacher.button href="{{ route('teacher.quizzes.edit', $contentId) }}" variant="ghost" icon="pencil-square">Redaktə</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Quiz məlumatı --}}
    <x-teacher.card :padding="false">
        <div class="grid gap-4 p-6 sm:grid-cols-2 lg:grid-cols-4">
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Yayım</p>
                <x-teacher.badge :color="$quizData['is_published'] ? 'green' : 'yellow'" class="mt-1">
                    {{ $quizData['is_published'] ? 'Yayımlandı' : 'Qaralama' }}
                </x-teacher.badge>
            </div>
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">İş sahəsi</p>
                <p class="mt-1 text-sm font-medium text-base-content">{{ $quizData['workspace'] ?? '—' }}</p>
            </div>
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Qovluq</p>
                <p class="mt-1 text-sm font-medium text-base-content">{{ $quizData['folder'] ?? 'Kök' }}</p>
            </div>
            <div>
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Suallar</p>
                <p class="mt-1 text-sm font-medium text-base-content">{{ count($questions) }}</p>
            </div>
        </div>
        @if ($quizData['description'])
            <div class="border-t border-base-300 p-6">
                <p class="text-xs font-medium uppercase tracking-wide text-base-content/50">Təsvir</p>
                <p class="mt-2 text-sm whitespace-pre-wrap text-base-content/80">{{ $quizData['description'] }}</p>
            </div>
        @endif
    </x-teacher.card>

    {{-- Suallar siyahısı --}}
    <x-teacher.card :padding="false">
        <div class="flex items-center justify-between border-b border-base-300 p-4">
            <h3 class="text-sm font-semibold text-base-content">Suallar</h3>
            <x-teacher.button href="{{ route('teacher.quizzes.edit', $contentId) }}" variant="ghost" size="sm" icon="plus">Sual əlavə et</x-teacher.button>
        </div>
        @if (count($questions) === 0)
            <x-teacher.empty-state icon="clipboard-document-list" title="Sual yoxdur" description="Redaktə et bölməsindən bu quizə sual əlavə edə bilərsiniz." />
        @else
            <ol class="divide-y divide-base-200">
                @foreach ($questions as $q)
                    <li class="p-4">
                        <div class="flex items-start gap-3">
                            <span class="mt-0.5 flex size-6 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                                {{ $q['position'] }}
                            </span>
                            <div class="min-w-0 flex-1">
                                <p class="text-sm font-medium leading-snug text-base-content">{{ $q['text'] }}</p>
                                <div class="mt-2 space-y-1">
                                    @foreach ($q['options'] as $letter => $option)
                                        @php
                                            $isCorrect = $letter === chr(65 + (int) $q['correct_option']);
                                        @endphp
                                        <p class="flex items-center gap-1.5 text-sm text-base-content/70">
                                            <span class="inline-flex size-4 items-center justify-center rounded-full border text-[10px] font-bold {{ $isCorrect ? 'border-success text-success' : 'border-base-300 text-base-content/40' }}">
                                                {{ $letter }}
                                            </span>
                                            {{ $option }}
                                            @if ($isCorrect)
                                                <span class="text-xs text-success">· doğru</span>
                                            @endif
                                        </p>
                                    @endforeach
                                </div>
                            </div>
                        </div>
                    </li>
                @endforeach
            </ol>
        @endif
    </x-teacher.card>
</div>
@endsection
