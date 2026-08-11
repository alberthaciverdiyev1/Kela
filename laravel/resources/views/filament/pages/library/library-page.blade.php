<x-filament-panels::page>
    <div class="space-y-6">
        {{-- Type tabs --}}
        <div class="flex flex-wrap items-center gap-2">
            @php
                $tabType = fn (?int $t) => $this->type === $t;
            @endphp
            <button
                wire:click="setType(null)"
                class="fi-btn rounded-lg px-4 py-2 text-sm font-medium {{ $this->type === null ? 'bg-primary-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200' }}"
            >
                Hamısı ({{ array_sum($counts) }})
            </button>
            <button
                wire:click="setType(0)"
                class="rounded-lg px-4 py-2 text-sm font-medium {{ $this->type === 0 ? 'bg-primary-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200' }}"
            >
                Dərslər ({{ $counts[0] ?? 0 }})
            </button>
            <button
                wire:click="setType(1)"
                class="rounded-lg px-4 py-2 text-sm font-medium {{ $this->type === 1 ? 'bg-primary-600 text-white' : 'bg-gray-100 text-gray-700 hover:bg-gray-200' }}"
            >
                Quizlər ({{ $counts[1] ?? 0 }})
            </button>
        </div>

        {{-- Breadcrumb --}}
        <nav class="flex flex-wrap items-center gap-1 text-sm">
            <button wire:click="openRoot" class="inline-flex items-center gap-1 rounded px-2 py-1 font-medium text-primary-600 hover:bg-primary-50 hover:underline">
                <x-heroicon-o-home class="h-4 w-4" />
                Kök
            </button>
            @foreach ($breadcrumbs as $crumb)
                <span class="text-gray-300">/</span>
                <button wire:click="goToBreadcrumb({{ $crumb['id'] }})" class="rounded px-2 py-1 font-medium text-gray-700 hover:bg-gray-100 hover:underline">
                    {{ $crumb['name'] }}
                </button>
            @endforeach
        </nav>

        {{-- Empty state --}}
        @if (count($folders) === 0 && count($contents) === 0)
            <div class="flex flex-col items-center justify-center gap-3 rounded-xl border border-dashed border-gray-300 bg-gray-50 py-16 text-center">
                <x-heroicon-o-folder-open class="h-12 w-12 text-gray-300" />
                <p class="text-sm text-gray-500">Bu qovluq boşdur. "Yeni Qovluq" və ya "Yeni Məzmun" ilə başlayın.</p>
            </div>
        @endif

        {{-- File list --}}
        @if (count($folders) > 0 || count($contents) > 0)
            <div class="overflow-hidden rounded-xl border border-gray-200 bg-white">
                <div class="overflow-x-auto">
                    <table class="min-w-full divide-y divide-gray-200">
                        <thead class="bg-gray-50">
                            <tr>
                                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Ad</th>
                                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Tip / Status</th>
                                <th class="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wider text-gray-500 w-1"></th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-200">
                            @foreach ($folders as $folder)
                                <tr class="hover:bg-gray-50">
                                    <td class="px-4 py-3">
                                        <button wire:click="openFolder({{ $folder['node_id'] }})" class="inline-flex items-center gap-2 font-medium text-primary-600 hover:underline">
                                            <x-heroicon-o-folder class="h-4 w-4 opacity-60" />
                                            {{ $folder['name'] }}
                                        </button>
                                    </td>
                                    <td class="px-4 py-3">
                                        <span class="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-600">Qovluq</span>
                                    </td>
                                    <td class="whitespace-nowrap px-4 py-3 text-right">
                                        <x-library-node-actions :node-id="$folder['node_id']" :name="$folder['name']" :is-folder="true" />
                                    </td>
                                </tr>
                            @endforeach

                            @foreach ($contents as $content)
                                <tr class="hover:bg-gray-50">
                                    <td class="px-4 py-3">
                                        <button wire:click="editContent({{ $content['content_id'] }}, {{ $content['type'] }})" class="inline-flex items-center gap-2 font-medium text-gray-800 hover:text-primary-600">
                                            @php
                                                $icon = match ($content['type']) {
                                    0 => 'heroicon-o-book-open',
                                    1 => 'heroicon-o-clipboard-document-list',
                                    2 => 'heroicon-o-document-text',
                                    3 => 'heroicon-o-video-camera',
                                    default => 'heroicon-o-link',
                                };
                                            @endphp
                                            <x-dynamic-component :component="$icon" class="h-4 w-4 opacity-60" />
                                            {{ $content['title'] }}
                                        </button>
                                    </td>
                                    <td class="px-4 py-3">
                                        <div class="flex flex-wrap items-center gap-1.5">
                                            <span class="inline-flex items-center rounded-full bg-blue-50 px-2.5 py-0.5 text-xs font-medium text-blue-700">
                                                {{ $this->getTypeLabel($content['type']) }}
                                            </span>
                                            @if ($content['is_published'])
                                                <span class="inline-flex items-center rounded-full bg-green-50 px-2.5 py-0.5 text-xs font-medium text-green-700">Yayımlandı</span>
                                            @else
                                                <span class="inline-flex items-center rounded-full bg-amber-50 px-2.5 py-0.5 text-xs font-medium text-amber-700">Qaralama</span>
                                            @endif
                                            @if ($content['type'] === 0)
                                                @if ($content['has_video'])
                                                    <span class="inline-flex items-center gap-1 rounded-full bg-green-50 px-2.5 py-0.5 text-xs font-medium text-green-700">
                                                        <x-heroicon-o-video-camera class="h-3 w-3" /> {{ $content['duration_label'] }}
                                                    </span>
                                                @else
                                                    <span class="inline-flex items-center rounded-full bg-amber-50 px-2.5 py-0.5 text-xs font-medium text-amber-700">Video yoxdur</span>
                                                @endif
                                            @endif
                                            @if ($content['type'] === 1 && $content['question_count'] > 0)
                                                <span class="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-600">
                                                    {{ $content['question_count'] }} sual
                                                </span>
                                            @endif
                                        </div>
                                    </td>
                                    <td class="whitespace-nowrap px-4 py-3 text-right">
                                        <div class="flex items-center justify-end gap-1">
                                            @if ($content['type'] !== 2 && $content['type'] !== 3 && $content['type'] !== 4)
                                                <button
                                                    wire:click="togglePublish({{ $content['content_id'] }})"
                                                    title="{{ $content['is_published'] ? 'Qaralamaya çevir' : 'Yayımla' }}"
                                                    class="rounded-lg p-1.5 text-gray-500 hover:bg-gray-100 hover:text-gray-700"
                                                >
                                                    <x-dynamic-component :component="$content['is_published'] ? 'heroicon-o-eye-slash' : 'heroicon-o-eye'" class="h-4 w-4" />
                                                </button>
                                            @endif
                                            <x-library-node-actions :node-id="$content['node_id']" :name="$content['title']" :is-folder="false" />
                                        </div>
                                    </td>
                                </tr>
                            @endforeach
                        </tbody>
                    </table>
                </div>
            </div>
        @endif
    </div>

    {{-- Rename dialog (Alpine) --}}
    <div
        x-data="{
            open: false,
            nodeId: null,
            name: '',
            init() {
                Livewire.on('openRenameDialog', (payload) => {
                    this.nodeId = payload.nodeId;
                    this.name = payload.name;
                    this.open = true;
                    this.$nextTick(() => this.$refs.input.focus());
                });
            },
            save() {
                if (!this.name.trim()) return;
                @this.submitRename(this.nodeId, this.name.trim());
                this.open = false;
            }
        }"
        x-show="open"
        x-cloak
        class="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/40"
    >
        <div class="w-full max-w-md rounded-xl bg-white p-6 shadow-xl" @keydown.escape.window="open = false">
            <h3 class="mb-4 text-lg font-semibold">Adını dəyiş</h3>
            <input
                x-ref="input"
                x-model="name"
                type="text"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 focus:border-primary-500 focus:ring-primary-500"
                @keydown.enter.prevent="save()"
            />
            <div class="mt-4 flex justify-end gap-2">
                <button @click="open = false" class="rounded-lg bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200">Ləğv et</button>
                <button @click="save()" class="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white hover:bg-primary-700">Saxla</button>
            </div>
        </div>
    </div>

    {{-- Move dialog (Alpine) --}}
    <div
        x-data="{
            open: false,
            nodeId: null,
            folders: [],
            selected: null,
            init() {
                Livewire.on('openMoveDialog', (payload) => {
                    this.nodeId = payload.nodeId;
                    this.folders = payload.folders || [];
                    this.selected = this.folders.length ? String(this.folders[0].id) : null;
                    this.open = true;
                });
            },
            save() {
                @this.submitMove(this.nodeId, this.selected ? Number(this.selected) : null);
                this.open = false;
            }
        }"
        x-show="open"
        x-cloak
        class="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/40"
    >
        <div class="w-full max-w-md rounded-xl bg-white p-6 shadow-xl" @keydown.escape.window="open = false">
            <h3 class="mb-4 text-lg font-semibold">Daşı</h3>
            @if (count($folders) > 0)
                <select x-model="selected" class="w-full rounded-lg border border-gray-300 px-3 py-2 focus:border-primary-500">
                    <option :value="null">Kök qovluğa</option>
                    <template x-for="f in folders" :key="f.id">
                        <option :value="String(f.id)" x-text="'&nbsp;'.repeat(f.depth * 3) + f.name"></option>
                    </template>
                </select>
            @else
                <p class="text-sm text-gray-500">Daşımaq üçün qovluq yoxdur. Element kök qovluğa daşınacaq.</p>
            @endif
            <div class="mt-4 flex justify-end gap-2">
                <button @click="open = false" class="rounded-lg bg-gray-100 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-200">Ləğv et</button>
                <button @click="save()" class="rounded-lg bg-primary-600 px-4 py-2 text-sm font-medium text-white hover:bg-primary-700">Daşı</button>
            </div>
        </div>
    </div>
</x-filament-panels::page>
