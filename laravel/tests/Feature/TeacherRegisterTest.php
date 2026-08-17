<?php

namespace Tests\Feature;

use App\Application\Auth\AuthService;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class TeacherRegisterTest extends TestCase
{
    use RefreshDatabase;

    protected function setUp(): void
    {
        parent::setUp();

        foreach (User::ALL_ROLES as $role) {
            Role::firstOrCreate(['name' => $role]);
        }
    }

    public function test_register_page_renders(): void
    {
        $this->get('/auth/register')
            ->assertOk()
            ->assertSee('Kela platformasına qoşulun')
            ->assertSee('Qeydiyyatdan ke')
            ->assertSee('first_name');
    }

    public function test_teacher_can_register_and_is_logged_in(): void
    {
        $response = $this->post('/auth/register', [
            'first_name' => 'Yeni',
            'last_name' => 'Müəllim',
            'email' => 'yeni.muellim@kela.local',
            'password' => 'secret123',
            'password_confirmation' => 'secret123',
        ]);

        $response->assertRedirect(route('teacher.dashboard'));

        $user = User::where('email', 'yeni.muellim@kela.local')->first();
        $this->assertNotNull($user);
        $this->assertTrue($user->hasRole(User::ROLE_TEACHER));
        $this->assertEquals(User::STATUS_ACTIVE, $user->status);
        $this->assertAuthenticatedAs($user);
    }

    public function test_register_requires_password_confirmation(): void
    {
        $this->post('/auth/register', [
            'first_name' => 'X',
            'email' => 'x@kela.local',
            'password' => 'secret123',
            'password_confirmation' => 'different123',
        ])->assertSessionHasErrors('password');

        $this->assertNull(User::where('email', 'x@kela.local')->first());
    }

    public function test_register_rejects_duplicate_email(): void
    {
        $existing = User::factory()->create(['email' => 'tekrar@kela.local']);
        $existing->assignRole(User::ROLE_TEACHER);

        $this->post('/auth/register', [
            'first_name' => 'Y',
            'email' => 'tekrar@kela.local',
            'password' => 'secret123',
            'password_confirmation' => 'secret123',
        ])->assertSessionHasErrors('email');

        $this->assertEquals(1, User::where('email', 'tekrar@kela.local')->count());
    }

    public function test_login_page_links_to_register(): void
    {
        $this->get('/auth/login')
            ->assertOk()
            ->assertSee('Müəllim kimi qeydiyyatdan keçin')
            ->assertSee(route('auth.register'));
    }

    public function test_service_register_teacher_creates_active_teacher(): void
    {
        $user = app(AuthService::class)->registerTeacher([
            'first_name' => 'Servis',
            'last_name' => 'Müəllim',
            'email' => 'servis.muellim@kela.local',
            'password' => 'secret123',
        ]);

        $this->assertTrue($user->hasRole(User::ROLE_TEACHER));
        $this->assertEquals(User::STATUS_ACTIVE, $user->status);
        $this->assertEquals('servis.muellim@kela.local', $user->email);
    }
}
