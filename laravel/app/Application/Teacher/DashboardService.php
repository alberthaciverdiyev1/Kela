<?php

namespace App\Application\Teacher;

use App\Application\Homework\HomeworkService;
use App\Application\Lesson\LessonService;
use App\Application\Quiz\QuizService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\User\Enums\UserRole;
use App\Domain\User\UserRepository;

/**
 * Admin panel özet veriləri.
 */
class DashboardService
{
    public function __construct(
        private readonly UserRepository $users,
        private readonly LessonService $lessons,
        private readonly QuizService $quizzes,
        private readonly WorkspaceService $workspaces,
        private readonly HomeworkService $homeworks,
    ) {
    }

    public function counts(int $actingUserId): array
    {
        return [
            'teachers' => $this->users->roleCount(UserRole::TEACHER->value),
            'students' => $this->users->roleCount(UserRole::STUDENT->value),
            'lessons' => $this->lessons->paginate($actingUserId, null, 0, 1)->total(),
            'quizzes' => $this->quizzes->paginate($actingUserId, null, 0, 1)->total(),
            'workspaces' => count($this->workspaces->listForTeacher($actingUserId)),
            'homeworks' => $this->homeworks->paginate($actingUserId, null, 0, 1)->total(),
        ];
    }
}
