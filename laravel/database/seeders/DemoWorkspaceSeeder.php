<?php

namespace Database\Seeders;

use App\Application\Lesson\LessonService;
use App\Application\Quiz\QuizService;
use App\Application\Workspace\WorkspaceService;
use App\Application\WorkspaceFolder\WorkspaceFolderService;
use App\Domain\User\User;
use Illuminate\Database\Seeder;

/**
 * Demo öğretmeni (teacher@kela.local) üçün nümunə workspace:
 * workspace = base folder — içində qovluqlar, quiz-lər və dərslər.
 *
 * İdempotent: teacher-ın artıq workspace-i varsa atlayır.
 *   php artisan db:seed --class=DemoWorkspaceSeeder
 */
class DemoWorkspaceSeeder extends Seeder
{
    public function run(): void
    {
        $teacher = User::where('email', 'teacher@kela.local')->first();
        if ($teacher === null) {
            $this->command?->warn('teacher@kela.local tapılmadı — əvvəlcə DemoUserSeeder işə salın.');

            return;
        }

        $workspaces = app(WorkspaceService::class);
        if ($workspaces->listForTeacher((int) $teacher->id) !== []) {
            $this->command?->warn('Demo öğretmeni üçün workspace artıq mövcuddur — seeder atlandı.');

            return;
        }

        $folders = app(WorkspaceFolderService::class);
        $quizzes = app(QuizService::class);
        $lessons = app(LessonService::class);
        $teacherId = (int) $teacher->id;

        // ── Workspace (base folder) ────────────────────────────────────────
        $group = $workspaces->create($teacherId, '11A Riyaziyyat Qrupu');
        $workspaceId = (int) $group->id;

        // ── Qovluqlar ──────────────────────────────────────────────────────
        $exam = $folders->createFolder($workspaceId, 'Sınaq İmtahanları', null, $teacherId);
        $homework = $folders->createFolder($workspaceId, 'Ev Tapşırıqları', null, $teacherId);

        $tests = $folders->createFolder($workspaceId, 'Fənn Testləri', null, $teacherId);
        $algebraTests = $folders->createFolder($workspaceId, 'Cəbr Testləri', $tests->id, $teacherId);
        $geometryTests = $folders->createFolder($workspaceId, 'Həndəsə Testləri', $tests->id, $teacherId);

        $videoLessons = $folders->createFolder($workspaceId, 'Video Dərslər', null, $teacherId);

        // ── Quiz-lər (workspace qovluqlarına) ──────────────────────────────
        $quizzes->create($teacherId, [
            'title' => 'Sınaq İmtahanı №1',
            'description' => '11A qrupu üçün ilk sınaq imtahanı.',
            'is_published' => true,
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $exam->id,
        ]);

        $quizzes->create($teacherId, [
            'title' => 'Sınaq İmtahanı №2',
            'description' => '11A qrupu üçün ikinci sınaq imtahanı.',
            'is_published' => true,
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $exam->id,
        ]);

        $quizzes->create($teacherId, [
            'title' => 'Cəbr — Funksiyalar',
            'description' => 'Funksiya anlayışı üzrə qısa test.',
            'is_published' => false,
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $algebraTests->id,
        ]);

        $quizzes->create($teacherId, [
            'title' => 'Həndəsə — Üçbucaq',
            'description' => 'Üçbucaqlar üzrə yoxlama testi.',
            'is_published' => true,
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $geometryTests->id,
        ]);

        $quizzes->create($teacherId, [
            'title' => 'Ev Tapşırığı — Törəmə',
            'description' => 'Həftəlik ev tapşırığı testi.',
            'is_published' => false,
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $homework->id,
        ]);

        // ── Dərslər (workspace qovluqlarına) ───────────────────────────────
        $lessons->create($teacherId, [
            'title' => 'Törəmə — Video Dərs',
            'description' => 'Törəmənin mənası və qaydaları.',
            'is_published' => true,
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $videoLessons->id,
        ]);

        $lessons->create($teacherId, [
            'title' => 'İnteqral — Video Dərs',
            'description' => 'İnteqral anlayışı və hesablanması.',
            'is_published' => true,
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $videoLessons->id,
        ]);

        $lessons->create($teacherId, [
            'title' => 'Sınaq İmtahanına Hazırlıq',
            'description' => 'Müstəqil iş üçün təkrar materialı.',
            'is_published' => false,
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $homework->id,
        ]);

        // ── Kökdə bir məzmun (qovluqsuz) ───────────────────────────────────
        $lessons->create($teacherId, [
            'title' => 'Ümumi Təkrar',
            'description' => 'Yay təkrarı üçün ümumi dərs.',
            'is_published' => true,
            'workspace_id' => $workspaceId,
            'ws_folder_id' => null,
        ]);

        $this->command?->info('Demo workspace yaradıldı: 5 qovluq altında 5 quiz + 4 dərs.');
    }
}
