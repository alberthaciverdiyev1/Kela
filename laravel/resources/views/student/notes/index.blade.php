@extends('common.layouts.app')
@section('title', 'Qeydlər - Kela')
@section('content')
    @include('notes._index')
@endsection

@push('scripts')
    @vite('resources/js/notes/controller.js')
@endpush
