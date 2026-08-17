{{-- Server-rendered kataloq fragmenti: breadcrumb + qovluq/sual cədvəli. --}}
<nav class="flex flex-wrap items-center gap-1 text-sm">
    <a href="{{ route('teacher.questions.index') }}" class="inline-flex items-center gap-1 rounded px-2 py-1 font-medium text-primary hover:bg-primary/10">
        <x-icon name="heroicon-o-home" class="size-4" />
        Kök
    </a>
    @foreach ($breadcrumbs as $crumb)
        <span class="text-base-content/30">/</span>
        <a href="{{ route('teacher.questions.index', ['folder_id' => $crumb['id']]) }}" class="rounded px-2 py-1 font-medium text-base-content/70 hover:bg-base-200">
            {{ $crumb['name'] }}
        </a>
    @endforeach
</nav>

@if (count($folders) === 0 && count($questions) === 0)
    <x-teacher.empty-state icon="folder-open" title="Burada hələ heç nə yoxdur" description="Yeni qovluq açın və ya sual əlavə edin." />
@endif

@if (count($folders) > 0 || count($questions) > 0)
    <x-teacher.card :padding="false">
        <x-teacher.table :headers="['Ad', 'Tip / Seçimlər']">
            @foreach ($folders as $folder)
                <tr
                    class="cursor-context-menu transition hover:bg-base-200/50"
                    data-kind="folder"
                    data-folder-id="{{ $folder['id'] }}"
                    data-folder-name="{{ $folder['name'] }}"
                >
                    <td class="font-medium">
                        <a href="{{ route('teacher.questions.index', ['folder_id' => $folder['id']]) }}" class="inline-flex items-center gap-2 text-primary hover:underline">
                            <x-icon name="heroicon-o-folder" class="size-4 opacity-60" />
                            {{ $folder['name'] }}
                        </a>
                    </td>
                    <td>
                        <x-teacher.badge color="gray">Qovluq · {{ $folder['question_count'] }} sual</x-teacher.badge>
                    </td>
                </tr>
            @endforeach

            @foreach ($questions as $q)
                @php
                    $qJson = [
                        'text' => $q['text'],
                        'option_a' => $q['options']['A'] ?? '',
                        'option_b' => $q['options']['B'] ?? '',
                        'option_c' => $q['options']['C'] ?? '',
                        'option_d' => $q['options']['D'] ?? '',
                        'option_e' => $q['options']['E'] ?? '',
                        'correct_option' => $q['correct_option'],
                        'explanation' => $q['explanation'],
                    ];
                @endphp
                <tr
                    class="cursor-context-menu transition hover:bg-base-200/50"
                    data-kind="question"
                    data-question-id="{{ $q['id'] }}"
                    data-question='@json($qJson, JSON_HEX_APOS | JSON_HEX_QUOT | JSON_HEX_TAG)'
                    data-question-text="{{ Str::limit(strip_tags($q['text']), 50) }}"
                >
                    <td class="max-w-md">
                        <a href="{{ route('teacher.questions.show', $q['id']) }}" class="group inline-flex items-start gap-2 text-base-content">
                            <x-icon name="heroicon-o-question-mark-circle" class="mt-0.5 size-4 shrink-0 opacity-60 transition group-hover:text-primary" />
                            <span class="min-w-0">
                                <span class="rich-preview block font-medium transition group-hover:text-primary">{!! $q['text'] !!}</span>
                                @if ($q['explanation'])
                                    <span class="mt-0.5 flex items-center gap-1 text-xs text-base-content/50">
                                        <x-icon name="heroicon-o-light-bulb" class="size-3.5 shrink-0 text-amber-500" />
                                        {{ Str::limit($q['explanation'], 90) }}
                                    </span>
                                @endif
                            </span>
                        </a>
                    </td>
                    <td>
                        <div class="flex flex-wrap items-center gap-1">
                            @foreach ($q['options'] as $letter => $option)
                                <span class="rounded bg-base-200 px-1.5 py-0.5 text-xs text-base-content/70">
                                    <span class="font-semibold">{{ $letter }}.</span> {{ $option }}
                                </span>
                            @endforeach
                            <span class="inline-flex size-5 items-center justify-center rounded-full bg-green-100 text-[10px] font-bold text-green-700" title="Doğru cavab">
                                {{ chr(65 + $q['correct_option']) }}
                            </span>
                            @if ($q['used_in_quizzes'] > 0)
                                <x-teacher.badge color="blue">{{ $q['used_in_quizzes'] }} quiz</x-teacher.badge>
                            @endif
                        </div>
                    </td>
                </tr>
            @endforeach
        </x-teacher.table>
    </x-teacher.card>
@endif
