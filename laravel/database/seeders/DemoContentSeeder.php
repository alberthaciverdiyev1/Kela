<?php

namespace Database\Seeders;

use App\Application\Question\QuestionService;
use App\Application\QuestionFolder\QuestionFolderService;
use App\Application\Quiz\QuizService;
use App\Application\QuizFolder\QuizFolderService;
use App\Domain\User\User;
use Illuminate\Database\Seeder;

/**
 * Demo öğretmeni (teacher@kela.local) için örnek içerik:
 * sual qovluqları + suallar, quiz qovluqları + quizlər.
 *
 * Yalnız bir dəfə işə salın (idempotent): teacher-da artıq sual varsa atlayır.
 *   php artisan db:seed --class=DemoContentSeeder
 */
class DemoContentSeeder extends Seeder
{
    public function run(): void
    {
        $teacher = User::where('email', 'teacher@kela.local')->first();
        if ($teacher === null) {
            $this->command?->warn('teacher@kela.local tapılmadı — əvvəlcə DemoUserSeeder işə salın.');

            return;
        }

        $questions = app(QuestionService::class);
        $questionFolders = app(QuestionFolderService::class);
        $quizzes = app(QuizService::class);
        $quizFolders = app(QuizFolderService::class);

        if ($questions->listForTeacher((int) $teacher->id) !== []) {
            $this->command?->warn('Demo öğretmeni üçün məzmun artıq mövcuddur — seeder atlandı.');

            return;
        }

        // ── Sual qovluqları ────────────────────────────────────────────────
        $math = $questionFolders->createFolder($teacher->id, 'Riyaziyyat');
        $algebra = $questionFolders->createFolder($teacher->id, 'Cəbr', $math->id);
        $geometry = $questionFolders->createFolder($teacher->id, 'Həndəsə', $math->id);

        $physics = $questionFolders->createFolder($teacher->id, 'Fizika');
        $mechanics = $questionFolders->createFolder($teacher->id, 'Mexanika', $physics->id);
        $electric = $questionFolders->createFolder($teacher->id, 'Elektrik', $physics->id);

        $chemistry = $questionFolders->createFolder($teacher->id, 'Kimya');
        $organic = $questionFolders->createFolder($teacher->id, 'Üzvi Kimya', $chemistry->id);

        $cs = $questionFolders->createFolder($teacher->id, 'İnformatika');

        // ── Suallar ────────────────────────────────────────────────────────
        // Cəbr
        $qAlgebra = [
            ['text' => '2x + 6 = 14 tənliyində x-in qiyməti neçədir?', 'option_a' => '3', 'option_b' => '4', 'option_c' => '5', 'option_d' => '6', 'correct_option' => 1, 'explanation' => '2x = 8 → x = 4.'],
            ['text' => '(a + b)² açılışı aşağıdakılardan hansıdır?', 'option_a' => 'a² + b²', 'option_b' => 'a² + 2ab + b²', 'option_c' => 'a² − b²', 'option_d' => '2ab + a', 'correct_option' => 1, 'explanation' => 'İki toplamanın kvadratı: a² + 2ab + b².'],
            ['text' => 'x² − 9 ifadəsinin vuruqlara ayrılmış forması hansıdır?', 'option_a' => '(x − 3)²', 'option_b' => '(x + 3)²', 'option_c' => '(x − 3)(x + 3)', 'option_d' => 'x(x − 9)', 'correct_option' => 2, 'explanation' => 'Fərqin kvadratı: (x − 3)(x + 3).'],
            ['text' => '3x − 7 = 2x + 5 tənliyində x neçədir?', 'option_a' => '10', 'option_b' => '12', 'option_c' => '14', 'option_d' => '−12', 'correct_option' => 1, 'explanation' => 'x = 12.'],
            ['text' => 'Mütləq dəyər: |−7| ifadəsinin qiyməti neçədir?', 'option_a' => '−7', 'option_b' => '7', 'option_c' => '0', 'option_d' => '−1', 'correct_option' => 1, 'explanation' => 'Mütləq dəyər həmişə mənfi deyildir.'],
        ];
        // Həndəsə
        $qGeometry = [
            ['text' => 'Düzbucaqlı üçbucaqda a=3, b=4 olarsa hipotenuz c neçədir?', 'option_a' => '5', 'option_b' => '6', 'option_c' => '7', 'option_d' => '12', 'correct_option' => 0, 'explanation' => 'Pifaqor: 3²+4² = 9+16 = 25 → c = 5.'],
            ['text' => 'Dairənin sahəsi hansı düsturla hesablanır?', 'option_a' => '2πr', 'option_b' => 'πr²', 'option_c' => 'πd', 'option_d' => '4πr²', 'correct_option' => 1, 'explanation' => 'S = πr².'],
            ['text' => 'Üçbucağın daxili bucaqlarının cəmi neçə dərəcədir?', 'option_a' => '90°', 'option_b' => '180°', 'option_c' => '270°', 'option_d' => '360°', 'correct_option' => 1, 'explanation' => 'İstənilən üçbucağın bucaqları cəmi 180°-dir.'],
            ['text' => 'Tərəfi 6 sm olan kvadratın perimetri neçə sm-dir?', 'option_a' => '12', 'option_b' => '24', 'option_c' => '36', 'option_d' => '48', 'correct_option' => 1, 'explanation' => 'P = 4·6 = 24.'],
        ];
        // Mexanika
        $qMechanics = [
            ['text' => 'Sürət vahidi hansıdır?', 'option_a' => 'm/s', 'option_b' => 'm/s²', 'option_c' => 'kg', 'option_d' => 'N', 'correct_option' => 0, 'explanation' => 'Sürət m/s ilə ölçülür.'],
            ['text' => 'F = ma düsturu hansı qanunun ifadəsidir?', 'option_a' => 'Nyutonun 1-ci qanunu', 'option_b' => 'Nyutonun 2-ci qanunu', 'option_c' => 'Nyutonun 3-cü qanunu', 'option_d' => 'Enerjinin saxlanması', 'correct_option' => 1, 'explanation' => 'Təsir qüvvəsi = kütlə × təcil.'],
            ['text' => 'Cazibə qüvvəsini hansı elm adamı kəşf etmişdir?', 'option_a' => 'Eynşteyn', 'option_b' => 'Nyuton', 'option_c' => 'Faradey', 'option_d' => 'Paskal', 'correct_option' => 1, 'explanation' => 'Ümumdünya cazibə qanunu Nyuton tərəfindən tərtib edilmişdir.'],
            ['text' => 'Enerjinin vahidi hansıdır?', 'option_a' => 'Vatt', 'option_b' => 'Joul', 'option_c' => 'Amper', 'option_d' => 'Volt', 'correct_option' => 1, 'explanation' => 'Enerji Joul (J) ilə ölçülür.'],
            ['text' => 'Sükunətdən 2 m/s² təcillə hərəkət edən cisim 4 saniyədə hansı sürətə çatır?', 'option_a' => '4 m/s', 'option_b' => '6 m/s', 'option_c' => '8 m/s', 'option_d' => '16 m/s', 'correct_option' => 2, 'explanation' => 'v = at = 2·4 = 8 m/s.'],
        ];
        // Elektrik
        $qElectric = [
            ['text' => 'Gərginlik vahidi hansıdır?', 'option_a' => 'Volt', 'option_b' => 'Amper', 'option_c' => 'Om', 'option_d' => 'Vatt', 'correct_option' => 0, 'explanation' => 'Gərginlik Volt (V) ilə ölçülür.'],
            ['text' => 'Ohm qanunu hansı düsturla ifadə olunur?', 'option_a' => 'V = IR', 'option_b' => 'F = ma', 'option_c' => 'E = mc²', 'option_d' => 'P = IV', 'correct_option' => 0, 'explanation' => 'Gərginlik = cərəyan × müqavimət.'],
            ['text' => 'Cərəyan şiddəti vahidi hansıdır?', 'option_a' => 'Volt', 'option_b' => 'Om', 'option_c' => 'Amper', 'option_d' => 'Joul', 'correct_option' => 2, 'explanation' => 'Cərəyan Amper (A) ilə ölçülür.'],
        ];
        // Üzvi Kimya
        $qOrganic = [
            ['text' => 'Metanın kimyəvi formulu hansıdır?', 'option_a' => 'CH₄', 'option_b' => 'C₂H₆', 'option_c' => 'CO₂', 'option_d' => 'C₃H₈', 'correct_option' => 0, 'explanation' => 'Metan: bir karbon + dörd hidrogen.'],
            ['text' => 'Üzvi maddələrin əsas elementi hansıdır?', 'option_a' => 'Oksigen', 'option_b' => 'Azot', 'option_c' => 'Karbon', 'option_d' => 'Kükürd', 'correct_option' => 2, 'explanation' => 'Üzvi kimyada əsas element karbondur.'],
            ['text' => 'Etil spirtinin formulu hansıdır?', 'option_a' => 'CH₃OH', 'option_b' => 'C₂H₅OH', 'option_c' => 'C₆H₁₂O₆', 'option_d' => 'CH₃COOH', 'correct_option' => 1, 'explanation' => 'Etanol: C₂H₅OH.'],
        ];
        // İnformatika
        $qCs = [
            ['text' => 'CPU nə deməkdir?', 'option_a' => 'Mərkəzi prosessor', 'option_b' => 'Əməli yaddaş', 'option_c' => 'Sabit disk', 'option_d' => 'Şəbəkə kartı', 'correct_option' => 0, 'explanation' => 'Central Processing Unit — mərkəzi prosessor.'],
            ['text' => '1 KB neçə baytdır?', 'option_a' => '512', 'option_b' => '1000', 'option_c' => '1024', 'option_d' => '2048', 'correct_option' => 2, 'explanation' => '1 Kilobayt = 1024 bayt.'],
            ['text' => 'Aşağıdakılardan hansı əməliyyat sistemidir?', 'option_a' => 'Microsoft Word', 'option_b' => 'Linux', 'option_c' => 'Chrome', 'option_d' => 'Photoshop', 'correct_option' => 1, 'explanation' => 'Linux əməliyyat sistemidir; digərləri tətbiq proqramlarıdır.'],
            ['text' => 'HTML nə üçün istifadə olunur?', 'option_a' => 'Verilənlər bazası', 'option_b' => 'Veb səhifə quruluşu', 'option_c' => 'Şəkil redaktəsi', 'option_d' => 'Şəbəkə konfiqurasiyası', 'correct_option' => 1, 'explanation' => 'HTML veb səhifələrin quruluşunu təsvir edir.'],
        ];

        // ── Quiz qovluqları ────────────────────────────────────────────────
        $examFolder = $quizFolders->createFolder($teacher->id, 'Sınaq İmtahanları');
        $testFolder = $quizFolders->createFolder($teacher->id, 'Fənn Testləri');
        $mathTestFolder = $quizFolders->createFolder($teacher->id, 'Riyaziyyat', $testFolder->id);
        $scienceTestFolder = $quizFolders->createFolder($teacher->id, 'Fənnlər', $testFolder->id);

        $qRefs = [
            'algebra' => [],
            'geometry' => [],
            'mechanics' => [],
            'electric' => [],
            'organic' => [],
            'cs' => [],
        ];

        $createQuestions = function (array $items, int $folderId) use ($questions, $teacher, &$qRefs): void {
            foreach ($items as $item) {
                $q = $questions->create($teacher->id, [
                    'text' => $item['text'],
                    'option_a' => $item['option_a'],
                    'option_b' => $item['option_b'],
                    'option_c' => $item['option_c'] ?? null,
                    'option_d' => $item['option_d'] ?? null,
                    'option_e' => $item['option_e'] ?? null,
                    'correct_option' => $item['correct_option'] ?? 0,
                    'explanation' => $item['explanation'] ?? null,
                    'folder_id' => $folderId,
                ]);
                $qRefs[$item['ref']][] = $q->id;
            }
        };

        // Hər suala ref əlavə et
        $tag = fn (array $arr, string $ref) => array_map(fn ($a) => $a + ['ref' => $ref], $arr);

        $createQuestions($tag($qAlgebra, 'algebra'), $algebra->id);
        $createQuestions($tag($qGeometry, 'geometry'), $geometry->id);
        $createQuestions($tag($qMechanics, 'mechanics'), $mechanics->id);
        $createQuestions($tag($qElectric, 'electric'), $electric->id);
        $createQuestions($tag($qOrganic, 'organic'), $organic->id);
        $createQuestions($tag($qCs, 'cs'), $cs->id);

        // ── Quizlər yarat + sualları əlavə et ─────────────────────────────
        $makeQuiz = function (string $title, string $description, ?int $folderId, array $questionIds) use ($quizzes, $teacher): int {
            $quiz = $quizzes->create($teacher->id, [
                'title' => $title,
                'description' => $description,
                'is_published' => true,
                'folder_id' => $folderId,
            ]);
            foreach ($questionIds as $qid) {
                $quizzes->addQuestion($quiz->getKey(), $qid, (int) $teacher->id);
            }

            return $quiz->getKey();
        };

        // Cəbr + Həndəsə → Riyaziyyat Sınaq
        $makeQuiz(
            'Riyaziyyat Sınaq 1',
            'Cəbr və həndəsə qarışıq sınaq.',
            $mathTestFolder->id,
            array_merge($qRefs['algebra'], $qRefs['geometry']),
        );

        // Yalnız həndəsə → Həndəsə Testi
        $makeQuiz('Həndəsə Testi', 'Dairə, üçbucaq və fiqurlar.', $mathTestFolder->id, $qRefs['geometry']);

        // Mexanika + Elektrik → Fizika Sınaq
        $makeQuiz(
            'Fizika Sınaq',
            'Mexanika və elektrik qarışıq test.',
            $scienceTestFolder->id,
            array_merge($qRefs['mechanics'], $qRefs['electric']),
        );

        // Üzvi kimya → Kimya Testi
        $makeQuiz('Kimya Testi', 'Üzvi kimyadan əsas anlayışlar.', $scienceTestFolder->id, $qRefs['organic']);

        // İnformatika → İnformatika Sınaq (kök quiz qovluğuna yox — kökə)
        $makeQuiz('İnformatika Sınaq', 'Proqram təminatı və vahidlər.', $examFolder->id, $qRefs['cs']);

        $this->command?->info('Demo məzmun yaradıldı: 6 qovluq altında '.count(array_merge(...array_values($qRefs))).' sual və 5 quiz.');
    }
}
