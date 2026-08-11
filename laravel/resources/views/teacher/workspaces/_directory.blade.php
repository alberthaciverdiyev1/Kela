{{-- Server-rendered kataloq fragmenti: breadcrumb + qovluq/məzmun cədvəli. --}}
<nav class="flex flex-wrap items-center gap-1 text-sm">
    <a href="{{ route('teacher.workspaces.show', $workspaceId) }}" class="inline-flex items-center gap-1 rounded px-2 py-1 font-medium text-primary hover:bg-primary/10">
        <x-icon name="heroicon-o-home" class="size-4" />
        Kök
    </a>
    @foreach ($breadcrumbs as $crumb)
        <span class="text-base-content/30">/</span>
        <a href="{{ route('teacher.workspaces.show', [$workspaceId, 'parent_id' => $crumb['id']]) }}" class="rounded px-2 py-1 font-medium text-base-content/70 hover:bg-base-200">
            {{ $crumb['name'] }}
        </a>
    @endforeach
</nav>

@if (count($folders) === 0 && count($contents) === 0)
    <x-teacher.empty-state icon="folder-open" title="Bu workspace boşdur" description="Yeni qovluq açın və ya kitabxanadan məzmun əlavə edin." />
@endif

@if (count($folders) > 0 || count($contents) > 0)
    <x-teacher.card :padding="false">
        <x-teacher.table :headers="['Ad', 'Tip / Status', '']">
            @foreach ($folders as $folder)
                <tr>
                    <td class="font-medium">
                        <a href="{{ route('teacher.workspaces.show', [$workspaceId, 'parent_id' => $folder['node_id']]) }}" class="inline-flex items-center gap-2 text-primary hover:underline">
                            <x-icon name="heroicon-o-folder" class="size-4 opacity-60" />
                            {{ $folder['name'] }}
                        </a>
                    </td>
                    <td><x-teacher.badge color="gray">Qovluq</x-teacher.badge></td>
                    <td class="text-right"><x-workspace-node-actions :node-id="$folder['node_id']" :name="$folder['name']" :is-folder="true" /></td>
                </tr>
            @endforeach

            @foreach ($contents as $content)
                <tr>
                    <td class="font-medium">
                        @php
                            $openUrl = \App\Domain\Content\Values\ContentType::isLesson($content['type'])
                                ? route('teacher.lessons.show', $content['content_id'])
                                : (\App\Domain\Content\Values\ContentType::isQuiz($content['type'])
                                    ? route('teacher.quizzes.edit', $content['content_id'])
                                    : null);
                        @endphp
                        @if ($openUrl)
                            <a href="{{ $openUrl }}" class="inline-flex items-center gap-2 text-base-content hover:text-primary">
                                <x-icon name="heroicon-o-{{ \App\Domain\Content\Values\ContentType::isLesson($content['type']) ? 'academic-cap' : 'clipboard-document-list' }}" class="size-4 opacity-60" />
                                {{ $content['title'] }}
                            </a>
                        @else
                            <span class="inline-flex items-center gap-2 text-base-content">
                                <x-icon name="heroicon-o-document-text" class="size-4 opacity-60" />
                                {{ $content['title'] }}
                            </span>
                        @endif
                    </td>
                    <td>
                        <div class="flex flex-wrap items-center gap-1.5">
                            <x-teacher.badge color="blue">{{ $content['type_label'] }}</x-teacher.badge>
                            @if ($content['has_video'])
                                <x-teacher.badge color="green">
                                    <span class="inline-flex items-center gap-1"><x-icon name="heroicon-o-video-camera" class="size-3" /> {{ $content['duration_label'] }}</span>
                                </x-teacher.badge>
                            @endif
                            @if ($content['question_count'] > 0)
                                <x-teacher.badge color="gray">{{ $content['question_count'] }} sual</x-teacher.badge>
                            @endif
                        </div>
                    </td>
                    <td class="text-right"><x-workspace-node-actions :node-id="$content['node_id']" :name="$content['title']" :is-folder="false" /></td>
                </tr>
            @endforeach
        </x-teacher.table>
    </x-teacher.card>
@endif

{{-- Move dialoqu üçün qovluq ağacı (fragment yeniləndikdə təzələnir). --}}
<template id="folder-tree-template">
    @foreach ($folderTree as $f)
        <option value="{{ $f['id'] }}">{{ str_repeat('&nbsp;&nbsp;', $f['depth']) }}{{ $f['name'] }}</option>
    @endforeach
</template>
