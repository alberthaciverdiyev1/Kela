<x-filament-panels::page>
    {{-- Quiz form --}}
    {{ $this->form }}

    {{-- Question list --}}
    <div class="mt-8">
        <div class="mb-4 flex items-center justify-between">
            <h3 class="text-lg font-semibold text-gray-900">
                Quiz Sualları
                <span class="ml-2 rounded-full bg-gray-100 px-2.5 py-0.5 text-sm font-medium text-gray-600">
                    {{ count($this->getQuestionsDataProperty()) }}
                </span>
            </h3>
        </div>

        @php $questions = $this->getQuestionsDataProperty(); @endphp

        @if (count($questions) === 0)
            <div class="flex flex-col items-center justify-center gap-3 rounded-xl border border-dashed border-gray-300 bg-gray-50 py-12 text-center">
                <x-heroicon-o-clipboard-document-list class="h-10 w-10 text-gray-300" />
                <p class="text-sm text-gray-500">
                    Quizdə hələ sual yoxdur. Yuxarıdakı "Sual Əlavə Et" düyməsi ilə yeni sual yaradın.
                </p>
            </div>
        @else
            <div class="overflow-hidden rounded-xl border border-gray-200 bg-white">
                <div class="overflow-x-auto">
                    <table class="min-w-full divide-y divide-gray-200">
                        <thead class="bg-gray-50">
                            <tr>
                                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500 w-8">#</th>
                                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Sual</th>
                                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Seçimlər</th>
                                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Doğru</th>
                                <th class="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-gray-500 w-1"></th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-200">
                            @foreach ($questions as $q)
                                <tr class="hover:bg-gray-50">
                                    <td class="px-4 py-3 text-sm font-medium text-gray-500">{{ $q['position'] }}</td>
                                    <td class="px-4 py-3 text-sm font-medium text-gray-800">{{ $q['text'] }}</td>
                                    <td class="px-4 py-3">
                                        <div class="flex flex-wrap gap-1">
                                            @foreach ($q['options'] as $letter => $option)
                                                <span class="rounded bg-gray-100 px-1.5 py-0.5 text-xs text-gray-600">
                                                    <span class="font-semibold">{{ $letter }}.</span> {{ $option }}
                                                </span>
                                            @endforeach
                                        </div>
                                    </td>
                                    <td class="px-4 py-3">
                                        <span class="inline-flex h-6 w-6 items-center justify-center rounded-full bg-green-100 text-xs font-bold text-green-700">
                                            {{ chr(65 + $q['correct_option']) }}
                                        </span>
                                    </td>
                                    <td class="whitespace-nowrap px-4 py-3 text-right">
                                        <button
                                            wire:confirm="Sual quizdən çıxarılsın?"
                                            wire:click="removeQuestion({{ $q['question_id'] }})"
                                            title="Çıxar"
                                            class="rounded-lg p-1.5 text-red-500 hover:bg-red-50 hover:text-red-700"
                                        >
                                            <x-heroicon-o-x-circle class="h-4 w-4" />
                                        </button>
                                    </td>
                                </tr>
                            @endforeach
                        </tbody>
                    </table>
                </div>
            </div>
        @endif
    </div>
</x-filament-panels::page>
