<?php

namespace Tests\Feature;

use App\Domain\Content\Content;
use App\Domain\Lesson\Lesson;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class LessonResourceTest extends TestCase
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

    public function test_teacher_can_list_and_create_lesson(): void
    {
        $this->actingAs($this->teacher);

        $this->get('/teacher/lessons')->assertOk();

        $this->post('/teacher/lessons', [
            'title' => 'Riyaziyyat Dərsi 1',
            'description' => 'Kəsrlərə giriş',
            'is_published' => '1',
            'order_index' => 2,
        ])->assertRedirect(route('teacher.lessons.index'));

        $this->assertDatabaseCount('contents', 1);
        $this->assertDatabaseCount('lessons', 1);

        $lesson = Lesson::with('content')->first();
        $this->assertNotNull($lesson);
        $this->assertEquals('Riyaziyyat Dərsi 1', $lesson->content->title);
        $this->assertEquals($this->teacher->id, $lesson->teacher_id);
        $this->assertEquals($this->teacher->id, $lesson->content->teacher_id);
        $this->assertEquals(Content::TYPE_LESSON, $lesson->content->type);
        $this->assertTrue($lesson->content->is_published);
        $this->assertTrue($lesson->is_published);
        $this->assertEquals(2, $lesson->order_index);
        $this->assertFalse($lesson->has_video);
    }

    public function test_teacher_only_sees_own_lessons(): void
    {
        $this->createLessonFor($this->teacher, 'Mənim dərsim');
        $this->createLessonFor($this->otherTeacher, 'Başqasının dərsi');

        $this->actingAs($this->teacher);
        $this->get('/teacher/lessons')
            ->assertOk()
            ->assertSee('Mənim dərsim')
            ->assertDontSee('Başqasının dərsi');
    }

    public function test_admin_sees_all_lessons(): void
    {
        $this->createLessonFor($this->teacher, 'Mənim dərsim');
        $this->createLessonFor($this->otherTeacher, 'Başqasının dərsi');

        $this->actingAs($this->admin);
        $this->get('/teacher/lessons')
            ->assertOk()
            ->assertSee('Mənim dərsim')
            ->assertSee('Başqasının dərsi');
    }

    public function test_teacher_cannot_edit_others_lesson(): void
    {
        $lesson = $this->createLessonFor($this->otherTeacher, 'Başqasının dərsi');

        $this->actingAs($this->teacher);
        $this->get("/teacher/lessons/{$lesson->content_id}/edit")->assertForbidden();
        $this->get("/teacher/lessons/{$lesson->content_id}")->assertForbidden();
        $this->post("/teacher/lessons/{$lesson->content_id}", ['title' => 'Hack'])->assertForbidden();
    }

    public function test_teacher_can_view_and_stream_own_lesson_video(): void
    {
        $lesson = $this->createLessonFor($this->teacher, 'Videolu dərs');
        $lesson->update(['video_path' => 'uploads/videos/test-lesson.mp4', 'duration_seconds' => 3]);

        $this->actingAs($this->teacher);

        $this->get("/teacher/lessons/{$lesson->content_id}")->assertOk();

        $this->get("/lesson/{$lesson->content_id}/stream")
            ->assertOk()
            ->assertHeader('Accept-Ranges', 'bytes')
            ->assertHeader('Content-Type', 'video/mp4');
    }

    public function test_stream_requires_auth_and_ownership(): void
    {
        $lesson = $this->createLessonFor($this->otherTeacher, 'Başqasının videolu dərsi');
        $lesson->update(['video_path' => 'uploads/videos/test-lesson.mp4']);

        $this->get("/lesson/{$lesson->content_id}/stream")->assertRedirect('/auth/login');

        $this->actingAs($this->teacher);
        $this->get("/lesson/{$lesson->content_id}/stream")->assertNotFound();
    }

    public function test_deleting_lesson_soft_deletes_content(): void
    {
        $lesson = $this->createLessonFor($this->teacher, 'Silinəcək');

        $lesson->delete();

        $this->assertSoftDeleted('contents', ['id' => $lesson->content_id]);
        $this->assertDatabaseMissing('lessons', ['content_id' => $lesson->content_id]);
    }

    public function test_teacher_can_delete_own_lesson_via_web_route(): void
    {
        $lesson = $this->createLessonFor($this->teacher, 'Silinəcək');

        $this->actingAs($this->teacher);
        $this->delete("/teacher/lessons/{$lesson->content_id}")
            ->assertRedirect(route('teacher.lessons.index'));

        $this->assertSoftDeleted('contents', ['id' => $lesson->content_id]);
    }

    private function createLessonFor(User $teacher, string $title): Lesson
    {
        $content = Content::create([
            'teacher_id' => $teacher->id,
            'title' => $title,
            'description' => null,
            'type' => Content::TYPE_LESSON,
            'is_published' => false,
        ]);

        return Lesson::create([
            'content_id' => $content->id,
            'teacher_id' => $teacher->id,
            'is_published' => false,
            'order_index' => 0,
        ]);
    }
}
