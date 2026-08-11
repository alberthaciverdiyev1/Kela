@extends('common.layouts.teacher')
@section('title', 'Dərslər - Kela')
@section('content')
<div class="space-y-6">
    <x-teacher.heading subtitle="Dərsləri idarə et">
        Dərslər
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.lessons.create') }}" icon="plus">Yeni Dərs</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <x-teacher.card :padding="false">
        <form method="GET" action="{{ route('teacher.lessons.index') }}" class="flex items-center gap-3 border-b border-base-300 p-4">
            <div class="relative w-72">
                <span class="pointer-events-none absolute inset-y-0 left-3 flex items-center">
                    <x-icon name="heroicon-o-magnifying-glass" class="size-4 text-base-content/40" />
                </span>
                <x-teacher.input name="search" value="{{ $search }}" placeholder="Başlıq ilə axtar..." class="pl-9" />
            </div>
            @if ($search !== '')
                <a href="{{ route('teacher.lessons.index') }}" class="btn btn-ghost btn-sm">Təmizlə</a>
            @endif
        </form>

        @if ($lessons->isEmpty())
            <x-teacher.empty-state icon="academic-cap" title="Dərs tapılmadı" description="Axtarışı dəyişin və ya yeni dərs əlavə edin." />
        @else
            <x-teacher.table :headers="['Başlıq', 'Video', 'Müddət', 'Yayım', 'Sıra', 'Yaradılıb', '']">
                @foreach ($lessons as $lesson)
                    <tr class="transition hover:bg-base-200/50">
                        <td class="font-medium text-base-content">
                            <a href="{{ route('teacher.lessons.show', $lesson['content_id']) }}" class="hover:text-primary">
                                {{ $lesson['title'] }}
                            </a>
                            @if ($lesson['description'])
                                <p class="text-xs text-base-content/50">{{ $lesson['description'] }}</p>
                            @endif
                        </td>
                        <td>
                            @if ($lesson['has_video'])
                                <x-teacher.badge color="green">
                                    <span class="inline-flex items-center gap-1"><x-icon name="heroicon-o-video-camera" class="size-3" /> Video</span>
                                </x-teacher.badge>
                            @else
                                <x-teacher.badge>Video yoxdur</x-teacher.badge>
                            @endif
                        </td>
                        <td class="text-base-content/70">{{ $lesson['has_video'] ? $lesson['duration_label'] : '—' }}</td>
                        <td>
                            <x-teacher.badge :color="$lesson['is_published'] ? 'green' : 'yellow'">
                                {{ $lesson['is_published'] ? 'Yayımlandı' : 'Qaralama' }}
                            </x-teacher.badge>
                        </td>
                        <td class="text-base-content/70">{{ $lesson['order_index'] }}</td>
                        <td class="text-base-content/70">{{ $lesson['created_at'] }}</td>
                        <td class="text-right">
                            <div class="flex items-center justify-end gap-1">
                                <x-teacher.button href="{{ route('teacher.lessons.show', $lesson['content_id']) }}" variant="ghost" size="sm" icon="eye">Aç</x-teacher.button>
                                <x-teacher.button href="{{ route('teacher.lessons.edit', $lesson['content_id']) }}" variant="ghost" size="sm" icon="pencil-square">Redaktə</x-teacher.button>
                                <form
                                    method="POST"
                                    action="{{ route('teacher.lessons.destroy', $lesson['content_id']) }}"
                                    data-api-delete="/api/v1/lessons/{{ $lesson['content_id'] }}"
                                    data-confirm="Bu dərsi silmək istəyirsiniz?"
                                >
                                    @csrf
                                    @method('DELETE')
                                    <x-teacher.button type="submit" variant="ghost" size="sm" icon="trash">Sil</x-teacher.button>
                                </form>
                            </div>
                        </td>
                    </tr>
                @endforeach
            </x-teacher.table>
            <x-teacher.pagination :paginator="$lessons" />
        @endif
    </x-teacher.card>
</div>
@endsection
