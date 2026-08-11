@props([
    'paginator' => null,
])
@if ($paginator && $paginator->hasPages())
    <div class="flex items-center justify-between gap-4 border-t border-base-300 px-6 py-3">
        <p class="text-xs text-base-content/60">
            {{ $paginator->firstItem() ?? 0 }}–{{ $paginator->lastItem() ?? 0 }} / {{ $paginator->total() }}
        </p>
        <div class="join">
            {{ $paginator->onEachSide(1)->links('vendor.pagination.daisyui') }}
        </div>
    </div>
@endif
