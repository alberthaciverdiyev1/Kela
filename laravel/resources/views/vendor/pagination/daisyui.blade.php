@if ($paginator->hasPages())
    <nav role="navigation" aria-label="Səhifələmə" class="flex items-center gap-1">
        {{-- Əvvəlki --}}
        @if ($paginator->onFirstPage())
            <span class="btn btn-sm btn-disabled join-item"><x-icon name="heroicon-o-chevron-left" class="size-4" /></span>
        @else
            <a href="{{ $paginator->previousPageUrl() }}" class="btn btn-sm btn-ghost join-item"><x-icon name="heroicon-o-chevron-left" class="size-4" /></a>
        @endif

        @foreach ($elements as $element)
            @if (is_string($element))
                <span class="btn btn-sm btn-disabled join-item">{{ $element }}</span>
            @endif
            @if (is_array($element))
                @foreach ($element as $page => $url)
                    @if ($page == $paginator->currentPage())
                        <span class="btn btn-sm btn-primary join-item">{{ $page }}</span>
                    @else
                        <a href="{{ $url }}" class="btn btn-sm btn-ghost join-item">{{ $page }}</a>
                    @endif
                @endforeach
            @endif
        @endforeach

        {{-- Sonrakı --}}
        @if ($paginator->hasMorePages())
            <a href="{{ $paginator->nextPageUrl() }}" class="btn btn-sm btn-ghost join-item"><x-icon name="heroicon-o-chevron-right" class="size-4" /></a>
        @else
            <span class="btn btn-sm btn-disabled join-item"><x-icon name="heroicon-o-chevron-right" class="size-4" /></span>
        @endif
    </nav>
@endif
