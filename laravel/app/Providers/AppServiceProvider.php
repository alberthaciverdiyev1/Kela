<?php

namespace App\Providers;

use App\Domain\City\CityRepository;
use App\Domain\Content\ContentRepository;
use App\Domain\Course\CourseRepository;
use App\Domain\Lesson\LessonRepository;
use App\Domain\Node\NodeRepository;
use App\Domain\Question\QuestionRepository;
use App\Domain\QuestionFolder\QuestionFolderRepository;
use App\Domain\Quiz\QuizRepository;
use App\Domain\Student\StudentRepository;
use App\Domain\User\UserRepository;
use App\Domain\Workspace\WorkspaceRepository;
use App\Infrastructure\Persistence\Repositories\EloquentCityRepository;
use App\Infrastructure\Persistence\Repositories\EloquentContentRepository;
use App\Infrastructure\Persistence\Repositories\EloquentLessonRepository;
use App\Infrastructure\Persistence\Repositories\EloquentNodeRepository;
use App\Infrastructure\Persistence\Repositories\EloquentQuestionFolderRepository;
use App\Infrastructure\Persistence\Repositories\EloquentQuestionRepository;
use App\Infrastructure\Persistence\Repositories\EloquentQuizRepository;
use App\Infrastructure\Persistence\Repositories\EloquentStudentRepository;
use App\Infrastructure\Persistence\Repositories\EloquentUserRepository;
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
        $this->app->bind(ContentRepository::class, EloquentContentRepository::class);
        $this->app->bind(StudentRepository::class, EloquentStudentRepository::class);
        $this->app->bind(UserRepository::class, EloquentUserRepository::class);
        $this->app->bind(CityRepository::class, EloquentCityRepository::class);
        $this->app->bind(NodeRepository::class, EloquentNodeRepository::class);
        $this->app->bind(WorkspaceRepository::class, EloquentWorkspaceRepository::class);
        $this->app->bind(QuizRepository::class, EloquentQuizRepository::class);
        $this->app->bind(QuestionRepository::class, EloquentQuestionRepository::class);
        $this->app->bind(QuestionFolderRepository::class, EloquentQuestionFolderRepository::class);
        // Course hələlik cədvəlsizdir; istifadə edilərsə ayrıca implementasiya tələb olunur.
        $this->app->bind(CourseRepository::class, fn () => throw new \LogicException(
            'Course cədvəli hələ mövcud deyil. EloquentCourseRepository əlavə olunmalıdır.'
        ));
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
