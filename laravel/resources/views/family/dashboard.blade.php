@extends('common.layouts.app')
@section('title', 'Parent Panel - Kela')
@section('content')
    <h1 class="text-2xl font-bold">Xoş gəldin, {{ auth()->user()->full_name }}!</h1>
    <p class="text-base-content/60">Valideyn paneli</p>
@endsection
