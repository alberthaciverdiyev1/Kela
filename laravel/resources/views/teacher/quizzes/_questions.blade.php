@if (count($questions) === 0)
    <x-teacher.empty-state
        icon="clipboard-document-list"
        title="Quizdə hələ sual yoxdur"
        description="Sual yaratmaq üçün Sual Bankına gedin, sonra buradan 'Bankdan Seç' ilə əlavə edin."
    />
@else
    <x-teacher.card :padding="false">
        <x-teacher.table :headers="['#', 'Sual', 'Seçimlər', 'Doğru', '']">
            @foreach ($questions as $q)
                @php
                    $isFirst = $q['position'] <= 1;
                    $isLast = $q['position'] >= count($questions);
                @endphp
                <tr>
                    <td class="font-medium text-base-content/50">{{ $q['position'] }}</td>
                    <td class="font-medium text-base-content rich-preview">{!! $q['text'] !!}</td>
                    <td>
                        <div class="flex flex-wrap gap-1">
                            @foreach ($q['options'] as $letter => $option)
                                <span class="rounded bg-base-200 px-1.5 py-0.5 text-xs text-base-content/70">
                                    <span class="font-semibold">{{ $letter }}.</span> {{ $option }}
                                </span>
                            @endforeach
                        </div>
                    </td>
                    <td>
                        <span class="inline-flex size-6 items-center justify-center rounded-full bg-green-100 text-xs font-bold text-green-700">
                            {{ chr(65 + $q['correct_option']) }}
                        </span>
                    </td>
                    <td class="text-right">
                        <div class="flex items-center justify-end gap-0.5">
                            <button
                                data-question-action="move"
                                data-question-id="{{ $q['question_id'] }}"
                                data-direction="up"
                                @disabled($isFirst)
                                title="Yuxarı"
                                class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200 hover:text-base-content disabled:opacity-30 disabled:hover:bg-transparent disabled:hover:text-base-content/50"
                            >
                                <x-icon name="heroicon-o-chevron-up" class="size-4" />
                            </button>
                            <button
                                data-question-action="move"
                                data-question-id="{{ $q['question_id'] }}"
                                data-direction="down"
                                @disabled($isLast)
                                title="Aşağı"
                                class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200 hover:text-base-content disabled:opacity-30 disabled:hover:bg-transparent disabled:hover:text-base-content/50"
                            >
                                <x-icon name="heroicon-o-chevron-down" class="size-4" />
                            </button>
                            {{-- Sualı düzləndirmək Sual Bankı modulundadır (teacher/questions) — burada yoxdur. --}}
                            <button
                                data-question-action="remove"
                                data-question-id="{{ $q['question_id'] }}"
                                data-confirm="Sual quizdən çıxarılsın?"
                                title="Çıxar"
                                class="rounded-lg p-1.5 text-error/70 hover:bg-error/10 hover:text-error"
                            >
                                <x-icon name="heroicon-o-x-circle" class="size-4" />
                            </button>
                        </div>
                    </td>
                </tr>
            @endforeach
        </x-teacher.table>
    </x-teacher.card>
@endif
