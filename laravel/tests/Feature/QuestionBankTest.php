<?php

namespace Tests\Feature;

use App\Application\Question\QuestionService;
use App\Application\QuestionFolder\QuestionFolderService;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class QuestionBankTest extends TestCase
{
    use RefreshDatabase;

    protected User $teacher;
    protected User $admin;
    protected User $otherTeacher;

    protected function setUp(): void
    {
        parent::setUp();

        foreach (User::ALL_ROLES as $role) {
            Role::firstOrCreate(['name' => $role]);
        }

        $this->teacher = User::factory()->create();
        $this->teacher->assignRole(User::ROLE_TEACHER);

        $this->admin = User::factory()->create();
        $this->admin->assignRole(User::ROLE_ADMIN);

        $this->otherTeacher = User::factory()->create();
        $this->otherTeacher->assignRole(User::ROLE_TEACHER);
    }

    private function folders(): QuestionFolderService
    {
        return app(QuestionFolderService::class);
    }

    private function questions(): QuestionService
    {
        return app(QuestionService::class);
    }

    private function makeQuestion(int $teacherId, array $overrides = []): int
    {
        return $this->questions()->create($teacherId, array_merge([
            'text' => '2+2=?',
            'option_a' => '3',
            'option_b' => '4',
            'correct_option' => 1,
        ], $overrides))->id;
    }

    public function test_question_bank_page_renders(): void
    {
        $this->actingAs($this->teacher);

        $this->get('/teacher/questions')
            ->assertOk()
            ->assertSee('Sual Bankı')
            ->assertSee('Yeni Qovluq')
            ->assertSee('Yeni Sual');
    }

    public function test_folder_and_question_appear_in_directory(): void
    {
        $this->actingAs($this->teacher);

        $folder = $this->folders()->createFolder($this->teacher->id, 'Riyaziyyat');
        $this->questions()->create($this->teacher->id, [
            'text' => 'İkinci dərəcəli tənlik',
            'option_a' => 'x',
            'option_b' => 'y',
            'correct_option' => 0,
            'folder_id' => $folder->id,
        ]);

        // Kökdə qovluq görünür; soru qovluğun içindədir (kökdə görünmür).
        $this->get('/teacher/questions')
            ->assertOk()
            ->assertSee('Riyaziyyat')
            ->assertDontSee('İkinci dərəcəli tənlik');

        // Qovluğa girəndə soru görünür.
        $this->get('/teacher/questions?folder_id='.$folder->id)
            ->assertOk()
            ->assertSee('İkinci dərəcəli tənlik');
    }

    public function test_folder_navigation_breadcrumbs(): void
    {
        $this->actingAs($this->teacher);

        $parent = $this->folders()->createFolder($this->teacher->id, 'Ana');
        $child = $this->folders()->createFolder($this->teacher->id, 'Alt', $parent->id);

        $this->get('/teacher/questions?folder_id='.$child->id)
            ->assertOk()
            ->assertSee('Ana')
            ->assertSee('Alt');
    }

    public function test_api_creates_folder_and_question_in_folder(): void
    {
        Sanctum::actingAs($this->teacher);

        // Qovluq yarat
        $created = $this->postJson('/api/v1/question-folders', ['name' => 'Fizika'])
            ->assertStatus(201)
            ->json('data');
        $folderId = $created['id'];

        // Sualı qovluğa yerləşdir
        $this->postJson('/api/v1/questions', [
            'text' => 'Sürət = ?',
            'option_a' => 'a',
            'option_b' => 'b',
            'correct_option' => 0,
            'folder_id' => $folderId,
        ])->assertStatus(201);

        $dir = $this->folders()->directory($this->teacher->id, $folderId);
        $this->assertCount(1, $dir['questions']);
        $this->assertEquals('Sürət = ?', $dir['questions'][0]['text']);
    }

    public function test_api_moves_question_to_folder_and_root(): void
    {
        Sanctum::actingAs($this->teacher);

        $folder = $this->folders()->createFolder($this->teacher->id, 'Cəbr');
        $qid = $this->makeQuestion($this->teacher->id);

        // Kökdədir (root)
        $root = $this->folders()->directory($this->teacher->id);
        $this->assertCount(1, $root['questions']);

        // Qovluğa daşı
        $this->postJson('/api/v1/question-folders/move-question', [
            'question_id' => $qid,
            'folder_id' => $folder->id,
        ])->assertOk();

        $this->assertCount(0, $this->folders()->directory($this->teacher->id)['questions']);
        $this->assertCount(1, $this->folders()->directory($this->teacher->id, $folder->id)['questions']);

        // Kökə geri daşı
        $this->postJson('/api/v1/question-folders/move-question', [
            'question_id' => $qid,
            'folder_id' => null,
        ])->assertOk();

        $this->assertCount(1, $this->folders()->directory($this->teacher->id)['questions']);
    }

    public function test_api_renames_and_moves_folder(): void
    {
        Sanctum::actingAs($this->teacher);

        $parent = $this->folders()->createFolder($this->teacher->id, 'Kök Qovluq');
        $child = $this->folders()->createFolder($this->teacher->id, 'Köhnə Ad');

        // Ad dəyiş
        $this->postJson("/api/v1/question-folders/{$child->id}/rename", ['name' => 'Yeni Ad'])
            ->assertOk();
        $this->assertEquals('Yeni Ad', $this->folders()->find($child->id)->name);

        // Qovluğa daşı
        $this->postJson("/api/v1/question-folders/{$child->id}/move", ['parent_id' => $parent->id])
            ->assertOk();
        $this->assertEquals($parent->id, $this->folders()->find($child->id)->parent_id);
    }

    public function test_deleting_folder_moves_questions_to_root(): void
    {
        Sanctum::actingAs($this->teacher);

        $folder = $this->folders()->createFolder($this->teacher->id, 'Silinəcək');
        $qid = $this->makeQuestion($this->teacher->id, ['folder_id' => $folder->id]);

        $this->deleteJson("/api/v1/question-folders/{$folder->id}")->assertOk();

        $this->assertNull($this->folders()->find($folder->id));
        $this->assertCount(1, $this->folders()->directory($this->teacher->id)['questions']);
        $this->assertEquals($qid, $this->folders()->directory($this->teacher->id)['questions'][0]['id']);
    }

    public function test_folder_ownership_is_enforced(): void
    {
        Sanctum::actingAs($this->otherTeacher);

        $folder = $this->folders()->createFolder($this->teacher->id, 'Başqasının Qovluğu');

        $this->postJson("/api/v1/question-folders/{$folder->id}/rename", ['name' => 'Çalma'])
            ->assertStatus(403);

        $this->deleteJson("/api/v1/question-folders/{$folder->id}")
            ->assertStatus(403);
    }

    public function test_teacher_only_sees_own_folders_and_questions(): void
    {
        $this->actingAs($this->teacher);

        $this->folders()->createFolder($this->teacher->id, 'Mənim Qovluq');
        $this->makeQuestion($this->teacher->id, ['text' => 'Mənim Sualım']);
        $this->folders()->createFolder($this->otherTeacher->id, 'Başqasının Qovluğu');
        $this->makeQuestion($this->otherTeacher->id, ['text' => 'Başqasının Sualı']);

        $this->get('/teacher/questions')
            ->assertOk()
            ->assertSee('Mənim Qovluq')
            ->assertSee('Mənim Sualım')
            ->assertDontSee('Başqasının Qovluğu')
            ->assertDontSee('Başqasının Sualı');
    }
}
