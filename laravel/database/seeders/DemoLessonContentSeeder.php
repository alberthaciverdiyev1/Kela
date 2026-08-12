<?php

namespace Database\Seeders;

use App\Application\Lesson\LessonService;
use App\Application\LessonFolder\LessonFolderService;
use App\Domain\User\User;
use Illuminate\Database\Seeder;

/**
 * Demo öğretmeni (teacher@kela.local) için örnek dərs məzmunu:
 * dərs qovluqları + dərslər.
 *
 * Yalnız bir dəfə işə salın (idempotent): teacher-da artıq dərs varsa atlayır.
 *   php artisan db:seed --class=DemoLessonContentSeeder
 */
class DemoLessonContentSeeder extends Seeder
{
    public function run(): void
    {
        $teacher = User::where('email', 'teacher@kela.local')->first();
        if ($teacher === null) {
            $this->command?->warn('teacher@kela.local tapılmadı — əvvəlcə DemoUserSeeder işə salın.');

            return;
        }

        $lessons = app(LessonService::class);
        $folders = app(LessonFolderService::class);

        if ($lessons->paginate((int) $teacher->id, null, 0, 1)->total() > 0) {
            $this->command?->warn('Demo öğretmeni üçün dərs məzmunu artıq mövcuddur — seeder atlandı.');

            return;
        }

        // ── Dərs qovluqları ────────────────────────────────────────────────
        $math = $folders->createFolder($teacher->id, 'Riyaziyyat');
        $algebra = $folders->createFolder($teacher->id, 'Cəbr', $math->id);
        $geometry = $folders->createFolder($teacher->id, 'Həndəsə', $math->id);

        $physics = $folders->createFolder($teacher->id, 'Fizika');
        $mechanics = $folders->createFolder($teacher->id, 'Mexanika', $physics->id);

        $informatics = $folders->createFolder($teacher->id, 'İnformatika');

        // ── Dərslər ────────────────────────────────────────────────────────
        $makeLesson = function (string $title, string $description, ?int $folderId) use ($lessons, $teacher): void {
            $lessons->create($teacher->id, [
                'title' => $title,
                'description' => $description,
                'is_published' => true,
                'order_index' => 0,
                'folder_id' => $folderId,
            ]);
        };

        $makeLesson('Cəbrə Giriş', 'Tənliklər və dəyişənlər.', $algebra->id);
        $makeLesson('Kvadrat Tənliklər', 'x² tənliklərinin həlli.', $algebra->id);
        $makeLesson('Üçbucaq Həndəsəsi', 'Bucaqlar, tərəflər və Pifaqor teoremi.', $geometry->id);
        $makeLesson('Dairə və Fiqurlar', 'Dairənin sahəsi və perimetri.', $geometry->id);
        $makeLesson('Nyuton Qanunları', 'Hərəkət və qüvvə anlayışları.', $mechanics->id);
        $makeLesson('Enerji və İş', 'Kinetik və potensial enerji.', $mechanics->id);
        $makeLesson('İnformatikaya Giriş', 'Kompüter arxitekturası və vahidlər.', $informatics->id);
        $makeLesson('Alqoritmlər əsasları', 'Alqoritm anlayışı və sadə nümunələr.', $informatics->id);
        $makeLesson('Riyaziyyat Təkrar', 'Ümumi təkrar — qovluqsuz (kök).', null);

        $this->command?->info('Demo dərs məzmunu yaradıldı: 6 qovluq altında 9 dərs.');
    }
}
