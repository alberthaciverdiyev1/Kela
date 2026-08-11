<?php

use App\Infrastructure\Http\Controllers\AuthController;
use App\Infrastructure\Http\Controllers\DashboardController;
use App\Infrastructure\Http\Controllers\LessonMediaController;
use Illuminate\Support\Facades\Route;

// --- Auth ---
Route::get('/auth/login', [AuthController::class, 'showLogin'])->name('auth.login');
Route::post('/auth/login', [AuthController::class, 'login'])->name('auth.login.post');
Route::post('/auth/logout', [AuthController::class, 'logout'])->name('auth.logout')->middleware('auth');

// Varsayılan giriş noktası Filament panelidir.
Route::get('/', fn () => redirect('/admin'));

// --- Admin ---
Route::prefix('admin')->middleware(['auth', 'role:Admin'])->group(function () {
    Route::get('/dashboard', [DashboardController::class, 'admin'])->name('admin.dashboard');
});

// --- Teacher ---
Route::prefix('teacher')->middleware(['auth', 'role:Teacher,Admin'])->group(function () {
    Route::get('/dashboard', [DashboardController::class, 'teacher'])->name('teacher.dashboard');
});

// --- Student ---
Route::prefix('student')->middleware(['auth', 'role:Student'])->group(function () {
    Route::get('/dashboard', [DashboardController::class, 'student'])->name('student.dashboard');
});

// --- Parent ---
Route::prefix('parent')->middleware(['auth', 'role:Parent'])->group(function () {
    Route::get('/dashboard', [DashboardController::class, 'parent'])->name('parent.dashboard');
});

// --- Ders medyası (video akışı + thumbnail) ---
Route::prefix('lesson')->middleware('auth')->group(function () {
    Route::get('/{contentId}/stream', [LessonMediaController::class, 'stream'])->name('lesson.video.stream');
    Route::get('/{contentId}/thumbnail', [LessonMediaController::class, 'thumbnail'])->name('lesson.thumbnail');
});

// --- Blocked / fallback ---
Route::get('/blocked', [DashboardController::class, 'blocked'])->name('blocked');
