<?php

use App\Web\Controllers\AuthController;
use App\Web\Controllers\DashboardController;
use App\Web\Controllers\LessonMediaController;
use App\Web\Controllers\Teacher\LessonController;
use App\Web\Controllers\Teacher\QuestionController;
use App\Web\Controllers\Teacher\QuizController;
use App\Web\Controllers\Teacher\StudentController;
use App\Web\Controllers\Teacher\WorkspaceController;
use Illuminate\Support\Facades\Route;

// --- Auth ---
Route::get('/auth/login', [AuthController::class, 'showLogin'])->name('auth.login');
Route::post('/auth/login', [AuthController::class, 'login'])->name('auth.login.post');
Route::get('/auth/register', [AuthController::class, 'showRegister'])->name('auth.register');
Route::post('/auth/register', [AuthController::class, 'register'])->name('auth.register.post');
Route::post('/auth/logout', [AuthController::class, 'logout'])->name('auth.logout')->middleware('auth');

// Varsayılan giriş noktası teacher panelidir (server-rendered Blade).
Route::get('/', fn () => redirect('/teacher/dashboard'));

// --- Teacher paneli (server-rendered Blade) ---
Route::middleware(['auth', 'role:Admin,Teacher'])->group(function () {
    Route::get('/teacher/dashboard', [DashboardController::class, 'teacher'])->name('teacher.dashboard');

    // Şagirdlər
    Route::prefix('teacher/students')->group(function () {
        Route::get('/', [StudentController::class, 'index'])->name('teacher.students.index');
        Route::get('/create', [StudentController::class, 'create'])->name('teacher.students.create');
        Route::post('/', [StudentController::class, 'store'])->name('teacher.students.store');
        Route::get('/table', [StudentController::class, 'tableFragment'])->name('teacher.students.table');
        Route::get('/{student}/edit', [StudentController::class, 'edit'])->name('teacher.students.edit');
        Route::post('/{student}', [StudentController::class, 'update'])->name('teacher.students.update');
        Route::delete('/{student}', [StudentController::class, 'destroy'])->name('teacher.students.destroy');
    });

    // Sual Bankı (qovluqlu kataloq)
    Route::prefix('teacher/questions')->group(function () {
        Route::get('/', [QuestionController::class, 'index'])->name('teacher.questions.index');
        Route::get('/table', [QuestionController::class, 'tableFragment'])->name('teacher.questions.table');
    });

    // İş Sahələri
    Route::prefix('teacher/workspaces')->group(function () {
        Route::get('/', [WorkspaceController::class, 'index'])->name('teacher.workspaces.index');
        Route::get('/create', [WorkspaceController::class, 'create'])->name('teacher.workspaces.create');
        Route::post('/', [WorkspaceController::class, 'store'])->name('teacher.workspaces.store');
        Route::get('/{workspace}', [WorkspaceController::class, 'show'])->name('teacher.workspaces.show');
        Route::get('/{workspace}/edit', [WorkspaceController::class, 'edit'])->name('teacher.workspaces.edit');
        Route::post('/{workspace}', [WorkspaceController::class, 'update'])->name('teacher.workspaces.update');
        Route::get('/{workspace}/directory', [WorkspaceController::class, 'directoryFragment'])->name('teacher.workspaces.directory');
        Route::delete('/{workspace}', [WorkspaceController::class, 'destroy'])->name('teacher.workspaces.destroy');
    });

    // Dərslər
    Route::prefix('teacher/lessons')->group(function () {
        Route::get('/', [LessonController::class, 'index'])->name('teacher.lessons.index');
        Route::get('/create', [LessonController::class, 'create'])->name('teacher.lessons.create');
        Route::post('/', [LessonController::class, 'store'])->name('teacher.lessons.store');
        Route::get('/{lesson}', [LessonController::class, 'show'])->name('teacher.lessons.show');
        Route::get('/{lesson}/edit', [LessonController::class, 'edit'])->name('teacher.lessons.edit');
        Route::post('/{lesson}', [LessonController::class, 'update'])->name('teacher.lessons.update');
        Route::delete('/{lesson}', [LessonController::class, 'destroy'])->name('teacher.lessons.destroy');
    });

    // Quizlər
    Route::prefix('teacher/quizzes')->group(function () {
        Route::get('/', [QuizController::class, 'index'])->name('teacher.quizzes.index');
        Route::get('/create', [QuizController::class, 'create'])->name('teacher.quizzes.create');
        Route::post('/', [QuizController::class, 'store'])->name('teacher.quizzes.store');
        Route::get('/{quiz}/questions', [QuizController::class, 'questionsFragment'])->name('teacher.quizzes.questions');
        Route::get('/{quiz}/edit', [QuizController::class, 'edit'])->name('teacher.quizzes.edit');
        Route::post('/{quiz}', [QuizController::class, 'update'])->name('teacher.quizzes.update');
        Route::delete('/{quiz}', [QuizController::class, 'destroy'])->name('teacher.quizzes.destroy');
    });
});

// --- Student / Parent / Blocked dashboards ---
Route::prefix('student')->middleware(['auth', 'role:Student'])->group(function () {
    Route::get('/dashboard', [DashboardController::class, 'student'])->name('student.dashboard');
});
Route::prefix('parent')->middleware(['auth', 'role:Parent'])->group(function () {
    Route::get('/dashboard', [DashboardController::class, 'parent'])->name('parent.dashboard');
});
Route::get('/blocked', [DashboardController::class, 'blocked'])->name('blocked');

// --- Dərs medyası (video akışı + thumbnail) ---
Route::prefix('lesson')->middleware('auth')->group(function () {
    Route::get('/{contentId}/stream', [LessonMediaController::class, 'stream'])->name('lesson.video.stream');
    Route::get('/{contentId}/thumbnail', [LessonMediaController::class, 'thumbnail'])->name('lesson.thumbnail');
});
