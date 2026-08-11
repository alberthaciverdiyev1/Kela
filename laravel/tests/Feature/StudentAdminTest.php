<?php

namespace Tests\Feature;

use App\Application\City\CityService;
use App\Application\Student\StudentService;
use App\Domain\City\City;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class StudentAdminTest extends TestCase
{
    use RefreshDatabase;

    protected User $teacher;
    protected User $admin;

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
    }

    public function test_admin_can_visit_dashboard(): void
    {
        app(StudentService::class)->create([
            'first_name' => 'Aysel',
            'email' => 'aysel@example.com',
        ]);

        $this->actingAs($this->admin);
        $this->get('/teacher/dashboard')
            ->assertOk()
            ->assertSee('Şagirdlər');
    }

    public function test_teacher_can_list_students(): void
    {
        app(StudentService::class)->create([
            'first_name' => 'Aysel',
            'last_name' => 'Məmmədova',
            'email' => 'aysel@example.com',
        ]);

        $this->actingAs($this->teacher);
        $this->get('/teacher/students')->assertOk()->assertSee('Aysel');
    }

    public function test_teacher_can_create_student_via_form(): void
    {
        $city = City::create([
            'name_translations' => ['az' => 'Bakı', 'en' => 'Baku'],
        ]);

        $this->actingAs($this->teacher);

        $this->post('/teacher/students', [
            'first_name' => 'Elvin',
            'last_name' => 'Əliyev',
            'email' => 'elvin@example.com',
            'password' => 'Elvin123',
            'city_id' => $city->id,
            'birth_date' => '2010-06-01',
            'status' => 1,
        ])->assertRedirect(route('teacher.students.index'));

        $student = User::where('email', 'elvin@example.com')->first();
        $this->assertNotNull($student);
        $this->assertTrue($student->hasRole(User::ROLE_STUDENT));
        $this->assertEquals($city->id, $student->studentProfile->city_id);
        $this->assertEquals('2010-06-01', $student->studentProfile->birth_date->format('Y-m-d'));
    }

    public function test_teacher_can_edit_student_profile(): void
    {
        $city = City::create([
            'name_translations' => ['az' => 'Gəncə', 'en' => 'Ganja'],
        ]);

        $student = app(StudentService::class)->create([
            'first_name' => 'Nigar',
            'email' => 'nigar@example.com',
        ]);

        $this->actingAs($this->teacher);

        $this->post("/teacher/students/{$student->id}", [
            'first_name' => 'Nigar',
            'last_name' => 'Quliyeva',
            'email' => 'nigar@example.com',
            'city_id' => $city->id,
            'birth_date' => '2011-02-14',
            'status' => 1,
        ])->assertRedirect(route('teacher.students.index'));

        $student->refresh();
        $this->assertEquals('Quliyeva', $student->last_name);
        $this->assertEquals($city->id, $student->studentProfile->city_id);
    }

    public function test_create_form_validates_required_email(): void
    {
        $this->actingAs($this->teacher);

        $this->post('/teacher/students', [
            'first_name' => 'Elvin',
            'email' => '',
            'password' => 'Secret123',
            'status' => 1,
        ])->assertSessionHasErrors('email');
    }

    public function test_city_service_returns_localized_options(): void
    {
        City::create([
            'name_translations' => ['az' => 'Bakı', 'en' => 'Baku'],
        ]);
        City::create([
            'name_translations' => ['az' => 'Paris', 'en' => 'Paris'],
        ]);

        $options = app(CityService::class)->options('az');
        $this->assertContains('Bakı', $options);
        $this->assertContains('Paris', $options);
    }

    public function test_teacher_panel_has_theme_toggle(): void
    {
        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/dashboard')->assertOk()->getContent();

        // Açıq/tünd rejim düyməsi + FOUC qarşısını alan script + data-theme
        $this->assertStringContainsString('data-theme="filament"', $html);
        $this->assertStringContainsString('localStorage.getItem(\'kela-theme\')', $html);
        $this->assertStringContainsString('filament-dark', $html);
        $this->assertStringContainsString('aria-label="Tema dəyişdir"', $html);
    }
}
