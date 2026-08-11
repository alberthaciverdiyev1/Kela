<?php

use App\Http\Controllers\AuthController;
use App\Http\Controllers\DashboardController;
use Illuminate\Support\Facades\Route;

// --- Auth ---
Route::get('/auth/login', [AuthController::class, 'showLogin'])->name('auth.login');
Route::post('/auth/login', [AuthController::class, 'login'])->name('auth.login.post');
Route::post('/auth/logout', [AuthController::class, 'logout'])->name('auth.logout')->middleware('auth');

Route::get('/', fn () => redirect('/auth/login'));

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

// --- Blocked / fallback ---
Route::get('/blocked', [DashboardController::class, 'blocked'])->name('blocked');
