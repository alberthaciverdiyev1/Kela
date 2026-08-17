<?php

use App\Web\Controllers\AuthController;
use App\Web\Controllers\DashboardController;
use App\Web\Controllers\LessonMediaController;
use App\Web\Controllers\Teacher\AttendanceController;
use App\Web\Controllers\Student\NoteController as StudentNoteController;
use App\Web\Controllers\Teacher\HomeworkController;
use App\Web\Controllers\Teacher\LessonController;
use App\Web\Controllers\Teacher\NoteController as TeacherNoteController;
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
        Route::get('/{student}', [StudentController::class, 'show'])->name('teacher.students.show');
        Route::get('/{student}/edit', [StudentController::class, 'edit'])->name('teacher.students.edit');
        Route::put('/{student}', [StudentController::class, 'update'])->name('teacher.students.update');
        Route::post('/{student}', [StudentController::class, 'update'])->name('teacher.students.update');
        Route::delete('/{student}', [StudentController::class, 'destroy'])->name('teacher.students.destroy');
    });

    // Sual Bankı (qovluqlu kataloq)
    Route::prefix('teacher/questions')->group(function () {
        Route::get('/', [QuestionController::class, 'index'])->name('teacher.questions.index');
        Route::get('/table', [QuestionController::class, 'tableFragment'])->name('teacher.questions.table');

        // JSON əməliyyatları (JS → web controller → servis)
        Route::post('/folders/move-question', [QuestionController::class, 'moveQuestionToFolder'])->name('teacher.questions.folders.move-question');
        Route::post('/folders', [QuestionController::class, 'storeFolder'])->name('teacher.questions.folders.store');
        Route::post('/folders/{folderId}/rename', [QuestionController::class, 'renameFolder'])->name('teacher.questions.folders.rename');
        Route::post('/folders/{folderId}/move', [QuestionController::class, 'moveFolder'])->name('teacher.questions.folders.move');
        Route::delete('/folders/{folderId}', [QuestionController::class, 'destroyFolder'])->name('teacher.questions.folders.destroy');
        Route::post('/', [QuestionController::class, 'storeJson'])->name('teacher.questions.store');
        Route::put('/{question}', [QuestionController::class, 'updateJson'])->name('teacher.questions.update');
        Route::delete('/{question}', [QuestionController::class, 'destroyJson'])->name('teacher.questions.destroy');

        Route::get('/{question}', [QuestionController::class, 'show'])->name('teacher.questions.show');
    });

    // İş Sahələri
    Route::prefix('teacher/workspaces')->group(function () {
        Route::get('/', [WorkspaceController::class, 'index'])->name('teacher.workspaces.index');
        Route::post('/', [WorkspaceController::class, 'store'])->name('teacher.workspaces.store');

        // JSON əməliyyatları (JS → web controller → servis)
        Route::post('/{workspace}/folders', [WorkspaceController::class, 'storeFolder'])->name('teacher.workspaces.folders.store');
        Route::post('/{workspace}/folders/{folderId}/rename', [WorkspaceController::class, 'renameFolder'])->name('teacher.workspaces.folders.rename');
        Route::post('/{workspace}/folders/{folderId}/move', [WorkspaceController::class, 'moveFolder'])->name('teacher.workspaces.folders.move');
        Route::post('/{workspace}/folders/{folderId}/remove', [WorkspaceController::class, 'removeFolder'])->name('teacher.workspaces.folders.remove');
        Route::delete('/{workspace}/folders/{folderId}', [WorkspaceController::class, 'destroyFolder'])->name('teacher.workspaces.folders.destroy');
        Route::post('/{workspace}/students', [WorkspaceController::class, 'attachStudents'])->name('teacher.workspaces.attach-students');
        Route::delete('/{workspace}/students/{studentId}', [WorkspaceController::class, 'detachStudent'])->name('teacher.workspaces.detach-student');

        Route::get('/{workspace}', [WorkspaceController::class, 'show'])->name('teacher.workspaces.show');
        Route::get('/{workspace}/edit', [WorkspaceController::class, 'edit'])->name('teacher.workspaces.edit');
        Route::put('/{workspace}', [WorkspaceController::class, 'update'])->name('teacher.workspaces.update');
        Route::post('/{workspace}', [WorkspaceController::class, 'update'])->name('teacher.workspaces.update');
        Route::delete('/{workspace}', [WorkspaceController::class, 'destroy'])->name('teacher.workspaces.destroy');
    });

    // Workspace-folders (workspace kontekstindən kənar əməliyyatlar)
    Route::post('/teacher/workspace-folders/move-content', [WorkspaceController::class, 'moveContent'])->name('teacher.workspace-folders.move-content');
    Route::post('/teacher/workspace-folders/add-folder', [WorkspaceController::class, 'addFolder'])->name('teacher.workspace-folders.add-folder');
    Route::post('/teacher/workspace-folders/remove-content', [WorkspaceController::class, 'removeContent'])->name('teacher.workspace-folders.remove-content');

    // Davam (yoklama)
    Route::get('/teacher/attendance', [AttendanceController::class, 'index'])->name('teacher.attendance.index');
    Route::get('/teacher/attendance/month', [AttendanceController::class, 'month'])->name('teacher.attendance.month');
    Route::post('/teacher/attendance', [AttendanceController::class, 'store'])->name('teacher.attendance.store');

    Route::get('/teacher/payments', [\App\Web\Controllers\Teacher\PaymentController::class, 'index'])->name('teacher.payments.index');
    Route::post('/teacher/payments/generate', [\App\Web\Controllers\Teacher\PaymentController::class, 'generate'])->name('teacher.payments.generate');
    Route::post('/teacher/payments', [\App\Web\Controllers\Teacher\PaymentController::class, 'store'])->name('teacher.payments.store');
    Route::patch('/teacher/payments/{track}', [\App\Web\Controllers\Teacher\PaymentController::class, 'updateTrack'])->name('teacher.payments.update');

    // Qeydlər (Google Keep üslubu)
    Route::get('/teacher/notes', [TeacherNoteController::class, 'index'])->name('teacher.notes.index');

    // Dərslər
    Route::prefix('teacher/lessons')->group(function () {
        Route::get('/', [LessonController::class, 'index'])->name('teacher.lessons.index');
        Route::get('/create', [LessonController::class, 'create'])->name('teacher.lessons.create');
        Route::post('/', [LessonController::class, 'store'])->name('teacher.lessons.store');

        // JSON əməliyyatları (JS → web controller → servis)
        Route::post('/folders/move-lesson', [LessonController::class, 'moveLessonToFolder'])->name('teacher.lessons.folders.move-lesson');
        Route::post('/folders', [LessonController::class, 'storeFolder'])->name('teacher.lessons.folders.store');
        Route::post('/folders/{folderId}/rename', [LessonController::class, 'renameFolder'])->name('teacher.lessons.folders.rename');
        Route::post('/folders/{folderId}/move', [LessonController::class, 'moveFolder'])->name('teacher.lessons.folders.move');
        Route::delete('/folders/{folderId}', [LessonController::class, 'destroyFolder'])->name('teacher.lessons.folders.destroy');

        Route::get('/{lesson}', [LessonController::class, 'show'])->name('teacher.lessons.show');
        Route::get('/{lesson}/edit', [LessonController::class, 'edit'])->name('teacher.lessons.edit');
        Route::put('/{lesson}', [LessonController::class, 'update'])->name('teacher.lessons.update');
        Route::post('/{lesson}', [LessonController::class, 'update'])->name('teacher.lessons.update');
        Route::delete('/{lesson}', [LessonController::class, 'destroy'])->name('teacher.lessons.destroy');
    });

    // Quizlər
    Route::prefix('teacher/quizzes')->group(function () {
        Route::get('/', [QuizController::class, 'index'])->name('teacher.quizzes.index');
        Route::get('/create', [QuizController::class, 'create'])->name('teacher.quizzes.create');
        Route::post('/', [QuizController::class, 'store'])->name('teacher.quizzes.store');

        // JSON əməliyyatları (JS → web controller → servis)
        Route::get('/folders/picker', [QuizController::class, 'picker'])->name('teacher.quizzes.folders.picker');
        Route::post('/folders/move-quiz', [QuizController::class, 'moveQuizToFolder'])->name('teacher.quizzes.folders.move-quiz');
        Route::post('/folders', [QuizController::class, 'storeFolder'])->name('teacher.quizzes.folders.store');
        Route::post('/folders/{folderId}/rename', [QuizController::class, 'renameFolder'])->name('teacher.quizzes.folders.rename');
        Route::post('/folders/{folderId}/move', [QuizController::class, 'moveFolder'])->name('teacher.quizzes.folders.move');
        Route::delete('/folders/{folderId}', [QuizController::class, 'destroyFolder'])->name('teacher.quizzes.folders.destroy');
        Route::post('/{quiz}/questions', [QuizController::class, 'addQuestion'])->name('teacher.quizzes.add-question');
        Route::delete('/{quiz}/questions/{questionId}', [QuizController::class, 'removeQuestion'])->name('teacher.quizzes.remove-question');
        Route::post('/{quiz}/questions/{questionId}/move', [QuizController::class, 'moveQuestion'])->name('teacher.quizzes.move-question');

        Route::get('/{quiz}/questions', [QuizController::class, 'questionsFragment'])->name('teacher.quizzes.questions');
        Route::get('/{quiz}', [QuizController::class, 'show'])->name('teacher.quizzes.show');
        Route::get('/{quiz}/edit', [QuizController::class, 'edit'])->name('teacher.quizzes.edit');
        Route::put('/{quiz}', [QuizController::class, 'update'])->name('teacher.quizzes.update');
        Route::post('/{quiz}', [QuizController::class, 'update'])->name('teacher.quizzes.update');
        Route::delete('/{quiz}', [QuizController::class, 'destroy'])->name('teacher.quizzes.destroy');
    });

    // Ev Tapşırıqları
    Route::prefix('teacher/homeworks')->group(function () {
        Route::get('/', [HomeworkController::class, 'index'])->name('teacher.homeworks.index');
        Route::get('/create', [HomeworkController::class, 'create'])->name('teacher.homeworks.create');
        Route::post('/', [HomeworkController::class, 'store'])->name('teacher.homeworks.store');
        Route::get('/quiz-questions/{quiz}', [HomeworkController::class, 'quizQuestions'])->name('teacher.homeworks.quiz-questions');
        Route::get('/{homework}', [HomeworkController::class, 'show'])->name('teacher.homeworks.show');
        Route::get('/{homework}/edit', [HomeworkController::class, 'edit'])->name('teacher.homeworks.edit');
        Route::post('/{homework}', [HomeworkController::class, 'update'])->name('teacher.homeworks.update');
        Route::delete('/{homework}', [HomeworkController::class, 'destroy'])->name('teacher.homeworks.destroy');
    });
});

// --- Şəxsi qeydlər (Google Keep üslubu) — istənilən doğrulanmış istifadəçi ---
Route::middleware('auth')->prefix('notes')->group(function () {
    Route::get('/', [TeacherNoteController::class, 'indexJson'])->name('notes.index');
    Route::get('/trashed', [TeacherNoteController::class, 'trashedJson'])->name('notes.trashed');
    Route::post('/', [TeacherNoteController::class, 'storeJson'])->name('notes.store');
    Route::put('/{note}', [TeacherNoteController::class, 'updateJson'])->name('notes.update');
    Route::delete('/{note}', [TeacherNoteController::class, 'destroyJson'])->name('notes.destroy');
    Route::post('/{note}/restore', [TeacherNoteController::class, 'restoreJson'])->name('notes.restore');
});

// --- Profil (Bütün rollar üçün) ---
Route::middleware('auth')->group(function () {
    Route::get('/profile', [App\Web\Controllers\ProfileController::class, 'edit'])->name('profile.edit');
    Route::put('/profile', [App\Web\Controllers\ProfileController::class, 'update'])->name('profile.update');
});

// --- Student / Parent / Blocked dashboards ---
Route::prefix('student')->middleware(['auth', 'role:Student'])->group(function () {
    Route::get('/dashboard', [DashboardController::class, 'student'])->name('student.dashboard');
    Route::get('/notes', [StudentNoteController::class, 'index'])->name('student.notes.index');
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
