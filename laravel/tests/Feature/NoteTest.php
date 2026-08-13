<?php

namespace Tests\Feature;

use App\Application\Note\NoteService;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class NoteTest extends TestCase
{
    use RefreshDatabase;

    protected User $teacher;
    protected User $student;
    protected User $otherTeacher;

    protected function setUp(): void
    {
        parent::setUp();

        foreach (User::ALL_ROLES as $role) {
            Role::firstOrCreate(['name' => $role]);
        }

        $this->teacher = User::factory()->create();
        $this->teacher->assignRole(User::ROLE_TEACHER);

        $this->student = User::factory()->create();
        $this->student->assignRole(User::ROLE_STUDENT);

        $this->otherTeacher = User::factory()->create();
        $this->otherTeacher->assignRole(User::ROLE_TEACHER);
    }

    private function notes(): NoteService
    {
        return app(NoteService::class);
    }

    public function test_teacher_notes_page_renders_keep_style_ui(): void
    {
        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/notes')->assertOk()->getContent();

        // Səhifə + Google Keep üslubu
        $this->assertStringContainsString('Qeydlər', $html);
        $this->assertStringContainsString('x-data="notesApp', $html);
        $this->assertStringContainsString('Qeyd yaz...', $html);
        $this->assertStringContainsString('Çöp qutusu', $html);

        // Interaktivlik
        $this->assertStringContainsString('openComposer', $html);
        $this->assertStringContainsString('closeComposer', $html);
        $this->assertStringContainsString('colorOf', $html);
        $this->assertStringContainsString('sortedNotes', $html);
        $this->assertStringContainsString('togglePin', $html);
        $this->assertStringContainsString('Çöpə at', $html);
    }

    public function test_student_notes_page_renders_keep_style_ui(): void
    {
        $this->actingAs($this->student);

        $html = $this->get('/student/notes')->assertOk()->getContent();

        $this->assertStringContainsString('Qeydlər', $html);
        $this->assertStringContainsString('x-data="notesApp', $html);
        $this->assertStringContainsString('Qeyd yaz...', $html);
    }

    public function test_notes_link_in_teacher_navbar(): void
    {
        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/dashboard')->assertOk()->getContent();
        $this->assertStringContainsString('/teacher/notes', $html);
    }

    public function test_api_lists_notes_pinned_first(): void
    {
        $regular = $this->notes()->store($this->teacher->id, ['title' => 'Adi qeyd']);
        $this->notes()->store($this->teacher->id, ['title' => 'Sabit qeyd', 'is_pinned' => true]);

        Sanctum::actingAs($this->teacher);

        $res = $this->getJson('/api/v1/notes')->assertOk()->json('data');
        $this->assertCount(2, $res);
        $this->assertEquals('Sabit qeyd', $res[0]['title']);
        $this->assertTrue($res[0]['is_pinned']);
        $this->assertEquals('Adi qeyd', $res[1]['title']);
        $this->assertArrayHasKey('color', $res[0]);
        $this->assertArrayHasKey('updated_at', $res[0]);
    }

    public function test_api_creates_note(): void
    {
        Sanctum::actingAs($this->teacher);

        $this->postJson('/api/v1/notes', [
            'title' => 'Yeni qeyd',
            'body' => 'Mətn',
            'color' => 'yellow',
        ])->assertStatus(201)->assertJson([
            'data' => [
                'title' => 'Yeni qeyd',
                'body' => 'Mətn',
                'color' => 'yellow',
                'is_pinned' => false,
            ],
        ]);

        $this->assertDatabaseCount('notes', 1);
    }

    public function test_api_updates_note(): void
    {
        $id = $this->notes()->store($this->teacher->id, ['title' => 'Əvvəl', 'color' => 'gray'])['id'];

        Sanctum::actingAs($this->teacher);

        $this->putJson("/api/v1/notes/{$id}", [
            'title' => 'Sonra',
            'body' => 'Gövdə mətni',
            'color' => 'green',
            'is_pinned' => true,
        ])->assertOk()->assertJson([
            'data' => [
                'id' => $id,
                'title' => 'Sonra',
                'body' => 'Gövdə mətni',
                'color' => 'green',
                'is_pinned' => true,
            ],
        ]);
    }

    public function test_api_soft_deletes_and_restores(): void
    {
        $id = $this->notes()->store($this->teacher->id, ['title' => 'Silinəcək'])['id'];

        Sanctum::actingAs($this->teacher);

        // Sil: aktiv siyahıdan düşür, çöp qutusunda görünür.
        $this->deleteJson("/api/v1/notes/{$id}")->assertOk();
        $this->getJson('/api/v1/notes')->assertOk()->assertJson(['data' => []]);
        $this->getJson('/api/v1/notes/trashed')->assertOk()->assertJson([
            'data' => [['id' => $id, 'title' => 'Silinəcək']],
        ]);

        // Bərpa: çöp qutusundan çıxır, aktiv siyahıya dönür.
        $this->postJson("/api/v1/notes/{$id}/restore")->assertOk();
        $this->getJson('/api/v1/notes')->assertOk()->assertJson([
            'data' => [['id' => $id, 'title' => 'Silinəcək']],
        ]);
        $this->getJson('/api/v1/notes/trashed')->assertOk()->assertJson(['data' => []]);
    }

    public function test_notes_are_personal(): void
    {
        $this->notes()->store($this->teacher->id, ['title' => 'Müəllimin qeydi']);

        Sanctum::actingAs($this->student);

        $this->getJson('/api/v1/notes')->assertOk()->assertJson(['data' => []]);
    }

    public function test_api_owner_cannot_touch_other_users_note(): void
    {
        $id = $this->notes()->store($this->teacher->id, ['title' => 'Məxfi'])['id'];

        Sanctum::actingAs($this->otherTeacher);

        $this->putJson("/api/v1/notes/{$id}", ['title' => 'Oğurluq'])->assertStatus(403);
        $this->deleteJson("/api/v1/notes/{$id}")->assertStatus(403);
        $this->postJson("/api/v1/notes/{$id}/restore")->assertStatus(403);
        $this->assertDatabaseHas('notes', ['title' => 'Məxfi']);
    }

    public function test_api_rejects_invalid_color(): void
    {
        Sanctum::actingAs($this->teacher);

        $this->postJson('/api/v1/notes', [
            'title' => 'Rəngsiz',
            'color' => 'neon',
        ])->assertStatus(422);

        $this->assertDatabaseCount('notes', 0);
    }
}
