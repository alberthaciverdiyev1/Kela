<?php

namespace Tests\Feature;

use App\Application\Workspace\WorkspaceService;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class WorkspaceAdminTest extends TestCase
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

    public function test_teacher_can_list_workspaces(): void
    {
        app(WorkspaceService::class)->create($this->teacher->id, 'Sinif 3A');

        $this->actingAs($this->teacher);
        $this->get('/teacher/workspaces')->assertOk()->assertSee('Sinif 3A');
    }

    public function test_teacher_only_sees_own_workspaces(): void
    {
        app(WorkspaceService::class)->create($this->teacher->id, 'Mənim Workspace');
        app(WorkspaceService::class)->create($this->otherTeacher->id, 'Başqasının Workspace');

        $this->actingAs($this->teacher);
        $this->get('/teacher/workspaces')
            ->assertOk()
            ->assertSee('Mənim Workspace')
            ->assertDontSee('Başqasının Workspace');
    }

    public function test_admin_sees_all_workspaces(): void
    {
        app(WorkspaceService::class)->create($this->teacher->id, 'A Workspace');
        app(WorkspaceService::class)->create($this->otherTeacher->id, 'B Workspace');

        $this->actingAs($this->admin);
        $this->get('/teacher/workspaces')
            ->assertOk()
            ->assertSee('A Workspace')
            ->assertSee('B Workspace');
    }

    public function test_teacher_can_create_workspace(): void
    {
        $this->actingAs($this->teacher);

        $this->post('/teacher/workspaces', ['name' => 'Yeni Sinif'])
            ->assertRedirect();

        $this->assertDatabaseHas('workspaces', [
            'name' => 'Yeni Sinif',
            'teacher_id' => $this->teacher->id,
        ]);
    }

    public function test_create_workspace_validates_name(): void
    {
        $this->actingAs($this->teacher);

        $this->post('/teacher/workspaces', ['name' => ''])
            ->assertSessionHasErrors('name');
    }

    public function test_teacher_can_rename_own_workspace(): void
    {
        $ws = app(WorkspaceService::class)->create($this->teacher->id, 'Köhnə Ad');

        $this->actingAs($this->teacher);

        $this->post("/teacher/workspaces/{$ws->id}", ['name' => 'Yeni Ad'])
            ->assertRedirect(route('teacher.workspaces.show', $ws->id));

        $this->assertDatabaseHas('workspaces', ['id' => $ws->id, 'name' => 'Yeni Ad']);
    }

    public function test_teacher_cannot_rename_others_workspace(): void
    {
        $ws = app(WorkspaceService::class)->create($this->otherTeacher->id, 'Başqasının');

        $this->actingAs($this->teacher);

        $this->post("/teacher/workspaces/{$ws->id}", ['name' => 'Hack'])
            ->assertForbidden();

        $this->assertDatabaseHas('workspaces', ['id' => $ws->id, 'name' => 'Başqasının']);
    }
}
