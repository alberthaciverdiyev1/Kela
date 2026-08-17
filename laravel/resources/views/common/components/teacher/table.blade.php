@props([
    'headers' => [],
])
<div class="overflow-x-auto rounded-xl border border-base-300 bg-base-100 shadow-sm">
    <table class="table w-full text-sm">
        <thead>
            <tr class="border-b border-base-300 bg-base-200/50">
                @foreach ($headers as $header)
                    <th class="whitespace-nowrap px-5 py-3.5 text-left text-xs font-semibold uppercase tracking-wide text-base-content/60">{{ $header }}</th>
                @endforeach
            </tr>
        </thead>
        <tbody class="divide-y divide-base-200/60 bg-base-100">
            {{ $slot }}
        </tbody>
    </table>
</div>
