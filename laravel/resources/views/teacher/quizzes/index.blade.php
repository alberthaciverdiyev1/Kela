@extends('common.layouts.teacher')
@section('title', 'Quizlər - Kela')
@section('content')
<div class="space-y-6">
    <x-teacher.heading subtitle="Quizləri idarə et">
        Quizlər
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.quizzes.create') }}" icon="plus">Yeni Quiz</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <x-teacher.card :padding="false">
        <form method="GET" action="{{ route('teacher.quizzes.index') }}" class="flex items-center gap-3 border-b border-base-300 p-4">
            <div class="relative w-72">
                <span class="pointer-events-none absolute inset-y-0 left-3 flex items-center">
                    <x-icon name="heroicon-o-magnifying-glass" class="size-4 text-base-content/40" />
                </span>
                <x-teacher.input name="search" value="{{ $search }}" placeholder="Başlıq ilə axtar..." class="pl-9" />
            </div>
            @if ($search !== '')
                <a href="{{ route('teacher.quizzes.index') }}" class="btn btn-ghost btn-sm">Təmizlə</a>
            @endif
        </form>

        @if ($quizzes->isEmpty())
            <x-teacher.empty-state icon="clipboard-document-list" title="Quiz tapılmadı" description="Axtarışı dəyişin və ya yeni quiz əlavə edin." />
        @else
            <x-teacher.table :headers="['Başlıq', 'Sual sayı', 'Yayım', 'Yaradılıb', '']">
                @foreach ($quizzes as $quiz)
                    <tr class="transition hover:bg-base-200/50">
                        <td class="font-medium text-base-content">
                            <a href="{{ route('teacher.quizzes.edit', $quiz['content_id']) }}" class="hover:text-primary">{{ $quiz['title'] }}</a>
                            @if ($quiz['description'])
                                <p class="text-xs text-base-content/50">{{ $quiz['description'] }}</p>
                            @endif
                        </td>
                        <td>
                            <x-teacher.badge color="gray">{{ $quiz['questions_count'] }} sual</x-teacher.badge>
                        </td>
                        <td>
                            <x-teacher.badge :color="$quiz['is_published'] ? 'green' : 'yellow'">
                                {{ $quiz['is_published'] ? 'Yayımlandı' : 'Qaralama' }}
                            </x-teacher.badge>
                        </td>
                        <td class="text-base-content/70">{{ $quiz['created_at'] }}</td>
                        <td class="text-right">
                            <div class="flex items-center justify-end gap-1">
                                <x-teacher.button href="{{ route('teacher.quizzes.edit', $quiz['content_id']) }}" variant="ghost" size="sm" icon="pencil-square">Redaktə</x-teacher.button>
                                <form
                                    method="POST"
                                    action="{{ route('teacher.quizzes.destroy', $quiz['content_id']) }}"
                                    x-data="deleteForm({ url: '/api/v1/quizzes/{{ $quiz['content_id'] }}', message: 'Bu quiz silinsin?' })"
                                    @submit.prevent="submit"
                                >
                                    @csrf
                                    @method('DELETE')
                                    <x-teacher.button type="submit" variant="ghost" size="sm" icon="trash" x-bind:disabled="busy">Sil</x-teacher.button>
                                </form>
                            </div>
                        </td>
                    </tr>
                @endforeach
            </x-teacher.table>
            <x-teacher.pagination :paginator="$quizzes" />
        @endif
    </x-teacher.card>
</div>
@endsection
