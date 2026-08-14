<?php

namespace App\Providers;

use App\Domain\Attendance\AttendanceRepository;
use App\Domain\City\CityRepository;
use App\Domain\Homework\HomeworkRepository;
use App\Domain\Content\ContentRepository;
use App\Domain\Lesson\LessonRepository;
use App\Domain\LessonFolder\LessonFolderRepository;
use App\Domain\Note\NoteRepository;
use App\Domain\Question\QuestionRepository;
use App\Domain\QuestionFolder\QuestionFolderRepository;
use App\Domain\Quiz\QuizRepository;
use App\Domain\QuizFolder\QuizFolderRepository;
use App\Domain\Student\StudentRepository;
use App\Domain\User\UserRepository;
use App\Domain\Workspace\WorkspaceRepository;
use App\Domain\WorkspaceFolder\WorkspaceFolderRepository;
use App\Infrastructure\Persistence\Repositories\EloquentAttendanceRepository;
use App\Infrastructure\Persistence\Repositories\EloquentCityRepository;
use App\Infrastructure\Persistence\Repositories\EloquentHomeworkRepository;
use App\Infrastructure\Persistence\Repositories\EloquentContentRepository;
use App\Infrastructure\Persistence\Repositories\EloquentLessonFolderRepository;
use App\Infrastructure\Persistence\Repositories\EloquentLessonRepository;
use App\Infrastructure\Persistence\Repositories\EloquentNoteRepository;
use App\Infrastructure\Persistence\Repositories\EloquentQuestionFolderRepository;
use App\Infrastructure\Persistence\Repositories\EloquentQuestionRepository;
use App\Infrastructure\Persistence\Repositories\EloquentQuizFolderRepository;
use App\Infrastructure\Persistence\Repositories\EloquentQuizRepository;
use App\Infrastructure\Persistence\Repositories\EloquentStudentRepository;
use App\Infrastructure\Persistence\Repositories\EloquentUserRepository;
use App\Infrastructure\Persistence\Repositories\EloquentWorkspaceFolderRepository;
use App\Infrastructure\Persistence\Repositories\EloquentWorkspaceRepository;
use Illuminate\Database\Eloquent\Factories\Factory;
use Illuminate\Support\Facades\Blade;
use Illuminate\Support\ServiceProvider;

class AppServiceProvider extends ServiceProvider
{
    /**
     * Register any application services.
     */
    public function register(): void
    {
        // Domain kontraktları -> Infrastructure (Eloquent) implementasiyaları.
        $this->app->bind(LessonRepository::class, EloquentLessonRepository::class);
        $this->app->bind(LessonFolderRepository::class, EloquentLessonFolderRepository::class);
        $this->app->bind(NoteRepository::class, EloquentNoteRepository::class);
        $this->app->bind(ContentRepository::class, EloquentContentRepository::class);
        $this->app->bind(StudentRepository::class, EloquentStudentRepository::class);
        $this->app->bind(UserRepository::class, EloquentUserRepository::class);
        $this->app->bind(CityRepository::class, EloquentCityRepository::class);
        $this->app->bind(WorkspaceRepository::class, EloquentWorkspaceRepository::class);
        $this->app->bind(WorkspaceFolderRepository::class, EloquentWorkspaceFolderRepository::class);
        $this->app->bind(AttendanceRepository::class, EloquentAttendanceRepository::class);
        $this->app->bind(QuizRepository::class, EloquentQuizRepository::class);
        $this->app->bind(QuestionRepository::class, EloquentQuestionRepository::class);
        $this->app->bind(QuestionFolderRepository::class, EloquentQuestionFolderRepository::class);
        $this->app->bind(QuizFolderRepository::class, EloquentQuizFolderRepository::class);
        $this->app->bind(HomeworkRepository::class, EloquentHomeworkRepository::class);
    }

    /**
     * Bootstrap any application services.
     */
    public function boot(): void
    {
        // App\Domain\User\User -> Database\Factories\UserFactory
        // App\Domain\Student\StudentProfile -> Database\Factories\StudentProfileFactory
        Factory::guessFactoryNamesUsing(
            fn (string $modelName): string => 'Database\\Factories\\'.class_basename($modelName).'Factory'
        );

        // Ortaq UI komponentləri resources/views/common/components/teacher-da saxlanır.
        // x-teacher.card nöqtə sintaksisi alt-qovluq kimi çözülür: common/components/teacher/card.
        Blade::anonymousComponentPath(resource_path('views/common/components'));

        // Sanctum session auth (JS → /api/v1) üçün stateful host lazımdır.
        // Cari istifadə olunan host (localhost:8080, 127.0.0.1:8080 və s.)
        // avtomatik əlavə olunur ki, hansı host/port ilə girilsə də işləsin.
        if (! $this->app->runningInConsole()) {
            config(['sanctum.stateful' => array_values(array_unique([
                ...config('sanctum.stateful'),
                request()->getHttpHost(),
            ]))]);
        }
    }
}
