@extends('common.layouts.teacher')
@section('title', 'Ev Tapşırıqları - Kela')
@section('content')
<div class="space-y-6">
    <x-teacher.heading subtitle="Sualları quizlərdən seçin və ya əl ilə tapşırıq yazın">
        Ev Tapşırıqları
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.homeworks.create') }}" icon="plus">Yeni Ev Tapşırığı</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <x-teacher.card :padding="false">
        <form method="GET" action="{{ route('teacher.homeworks.index') }}" class="flex items-center gap-3 border-b border-base-300 p-4">
            <div class="relative w-72">
                <span class="pointer-events-none absolute inset-y-0 left-3 flex items-center">
                    <x-icon name="heroicon-o-magnifying-glass" class="size-4 text-base-content/40" />
                </span>
                <x-teacher.input name="search" value="{{ $search }}" placeholder="Başlıq ilə axtar..." class="pl-9" />
            </div>
            @if ($search !== '')
                <a href="{{ route('teacher.homeworks.index') }}" class="btn btn-ghost btn-sm">Təmizlə</a>
            @endif
        </form>

        @if ($homeworks->isEmpty())
            <x-teacher.empty-state icon="clipboard-document-list" title="Ev tapşırığı tapılmadı" description="Yeni ev tapşırığı yaradaraq sualları əlavə edin." />
        @else
            <x-teacher.table :headers="['Başlıq', 'Sual sayı', 'Yayım', 'Yaradılıb']">
                @foreach ($homeworks as $homework)
                    <tr class="transition hover:bg-base-200/50">
                        <td class="font-medium text-base-content">
                            <a href="{{ route('teacher.homeworks.show', $homework['id']) }}" class="hover:text-primary">{{ $homework['title'] }}</a>
                            @if ($homework['description'])
                                <p class="text-xs text-base-content/50">{{ $homework['description'] }}</p>
                            @endif
                        </td>
                        <td>
                            <x-teacher.badge color="blue">{{ $homework['questions_count'] }} sual</x-teacher.badge>
                        </td>
                        <td>
                            <x-teacher.badge :color="$homework['is_published'] ? 'green' : 'yellow'">
                                {{ $homework['is_published'] ? 'Yayımlandı' : 'Qaralama' }}
                            </x-teacher.badge>
                        </td>
                        <td class="text-base-content/70">{{ $homework['created_at'] }}</td>
                    </tr>
                @endforeach
            </x-teacher.table>
            <x-teacher.pagination :paginator="$homeworks" />
        @endif
    </x-teacher.card>
</div>
@endsection
