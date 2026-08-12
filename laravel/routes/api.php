<?php

use App\Api\Controllers\AuthController;
use App\Api\Controllers\CityController;
use App\Api\Controllers\LessonController;
use App\Api\Controllers\QuestionController;
use App\Api\Controllers\QuestionFolderController;
use App\Api\Controllers\QuizController;
use App\Api\Controllers\QuizFolderController;
use App\Api\Controllers\StudentController;
use App\Api\Controllers\WorkspaceController;
use App\Web\Controllers\LessonMediaController;
use Illuminate\Support\Facades\Route;

/*
|--------------------------------------------------------------------------
| API (Backend) — v1
|--------------------------------------------------------------------------
|
| Frontend (server-rendered Blade) ayrıdır: routes/web.php + app/Web + resources/views.
| JS səhifənin yalnız lazım olan hissəsini bu JSON endpointləri ilə yeniləyir.
| Bütün əməliyyatlar Application servisləri üzərindən aparılır — modellərə birbaşa toxunulmur.
|
| Auth: POST /auth/login → { token } (Bearer). Qalan bütün endpointlər
| `Authorization: Bearer <token>` tələb edir.
|
*/

// --- Açıq (token tələb etmir) ---
Route::post('/auth/login', [AuthController::class, 'login']);

// --- Doğrulanmış istifadəçi (Bearer token) ---
Route::middleware('auth:sanctum')->group(function () {
    Route::post('/auth/logout', [AuthController::class, 'logout']);
    Route::get('/auth/me', [AuthController::class, 'me']);

    // Lüğət məlumatı (istənilən doğrulanmış istifadəçi)
    Route::get('/cities', [CityController::class, 'index']);

    // Media — rol məntiqi LessonMediaController daxilindədir
    // (sahibi müəllim/admin hər zaman; şagird yalnız yayımlanan dərsi izləyə bilər).
    Route::get('/lessons/{contentId}/stream', [LessonMediaController::class, 'stream']);
    Route::get('/lessons/{contentId}/thumbnail', [LessonMediaController::class, 'thumbnail']);
});

// --- Admin / Müəllim resursları ---
Route::middleware(['auth:sanctum', 'role_api:Admin,Teacher'])->group(function () {
    Route::apiResource('students', StudentController::class)->parameters(['students' => 'student']);
    Route::apiResource('lessons', LessonController::class)->parameters(['lessons' => 'contentId']);
    Route::apiResource('quizzes', QuizController::class)->parameters(['quizzes' => 'contentId']);
    Route::apiResource('questions', QuestionController::class)->parameters(['questions' => 'question']);
    Route::apiResource('workspaces', WorkspaceController::class)->parameters(['workspaces' => 'workspace']);

    // Sual bankı qovluqları (workspace node-larından müstəqil)
    Route::get('/question-folders/directory', [QuestionFolderController::class, 'directory']);
    Route::post('/question-folders', [QuestionFolderController::class, 'store']);
    Route::post('/question-folders/{folderId}/rename', [QuestionFolderController::class, 'rename']);
    Route::post('/question-folders/{folderId}/move', [QuestionFolderController::class, 'move']);
    Route::delete('/question-folders/{folderId}', [QuestionFolderController::class, 'destroy']);
    Route::post('/question-folders/move-question', [QuestionFolderController::class, 'moveQuestion']);

    // Quiz qovluqları
    Route::get('/quiz-folders/directory', [QuizFolderController::class, 'directory']);
    Route::post('/quiz-folders', [QuizFolderController::class, 'store']);
    Route::post('/quiz-folders/{folderId}/rename', [QuizFolderController::class, 'rename']);
    Route::post('/quiz-folders/{folderId}/move', [QuizFolderController::class, 'move']);
    Route::delete('/quiz-folders/{folderId}', [QuizFolderController::class, 'destroy']);
    Route::post('/quiz-folders/move-quiz', [QuizFolderController::class, 'moveQuiz']);

    // Quiz → sual əlaqələri
    Route::post('/quizzes/{contentId}/questions', [QuizController::class, 'addQuestion']);
    Route::delete('/quizzes/{contentId}/questions/{questionId}', [QuizController::class, 'removeQuestion']);
    Route::post('/quizzes/{contentId}/questions/{questionId}/move', [QuizController::class, 'moveQuestion']);

    // İş sahəsi → şagird əlaqələri
    Route::post('/workspaces/{workspace}/students', [WorkspaceController::class, 'attachStudents']);
    Route::delete('/workspaces/{workspace}/students/{studentId}', [WorkspaceController::class, 'detachStudent']);

});
