@extends('layouts.app')
@section('title', 'Student Panel - Kela')
@section('content')
    <h1 class="text-2xl font-bold">Xoş gəldin, {{ auth()->user()->full_name }}!</h1>
    <p class="text-base-content/60">Şagird paneli</p>
@endsection
