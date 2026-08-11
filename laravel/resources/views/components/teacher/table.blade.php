@props([
    'headers' => [],
])
<div class="overflow-x-auto">
    <table class="table w-full text-sm">
        <thead>
            <tr class="border-b border-base-300 bg-base-200/50">
                @foreach ($headers as $header)
                    <th class="px-4 py-3 text-xs font-semibold uppercase tracking-wide text-base-content/60">{{ $header }}</th>
                @endforeach
            </tr>
        </thead>
        <tbody class="divide-y divide-base-300">
            {{ $slot }}
        </tbody>
    </table>
</div>
