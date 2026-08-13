@extends('common.layouts.teacher')
@section('title', 'Qeydlər - Kela')
@section('content')
    @include('notes._index')
@endsection

@push('scripts')
    @vite('resources/js/notes/controller.js')
@endpush
