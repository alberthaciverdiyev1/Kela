<?php

use App\Api\Controllers\AuthController;
use App\Api\Controllers\CityController;
use App\Api\Controllers\LessonController;
use App\Api\Controllers\QuestionController;
use App\Api\Controllers\QuizController;
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

    // Quiz → sual əlaqələri
    Route::post('/quizzes/{contentId}/questions', [QuizController::class, 'addQuestion']);
    Route::delete('/quizzes/{contentId}/questions/{questionId}', [QuizController::class, 'removeQuestion']);
    Route::post('/quizzes/{contentId}/questions/{questionId}/move', [QuizController::class, 'moveQuestion']);

    // İş sahəsi → qovluq/məzmun/şagird əlaqələri
    Route::post('/workspaces/{workspace}/folders', [WorkspaceController::class, 'createFolder']);
    Route::post('/workspaces/{workspace}/contents', [WorkspaceController::class, 'addContent']);
    Route::post('/workspaces/{workspace}/nodes/{nodeId}/move', [WorkspaceController::class, 'moveNode']);
    Route::post('/workspaces/{workspace}/nodes/{nodeId}/rename', [WorkspaceController::class, 'renameNode']);
    Route::delete('/workspaces/{workspace}/nodes/{nodeId}', [WorkspaceController::class, 'removeNode']);
    Route::post('/workspaces/{workspace}/students', [WorkspaceController::class, 'attachStudents']);
    Route::delete('/workspaces/{workspace}/students/{studentId}', [WorkspaceController::class, 'detachStudent']);

});
