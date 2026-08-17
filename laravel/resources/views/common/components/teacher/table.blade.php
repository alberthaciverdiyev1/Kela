@props([
    'headers' => [],
])
<div class="overflow-x-auto rounded-2xl border border-base-300/80 bg-base-100 shadow-sm shadow-base-300/20">
    <table class="table w-full text-sm">
        <thead>
            <tr class="border-b border-base-300 bg-gradient-to-r from-base-200/80 via-base-100 to-base-200/40">
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
