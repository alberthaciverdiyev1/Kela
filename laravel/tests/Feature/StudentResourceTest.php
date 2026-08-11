<?php

namespace Tests\Feature;

use App\Application\City\CityService;
use App\Application\Student\StudentService;
use App\Domain\City\City;
use App\Domain\User\User;
use App\Filament\Resources\Students\Pages\CreateStudent;
use App\Filament\Resources\Students\Pages\EditStudent;
use Filament\Facades\Filament;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Livewire\Livewire;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class StudentResourceTest extends TestCase
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

        Filament::setCurrentPanel('admin');
    }

    public function test_teacher_can_list_students(): void
    {
        app(StudentService::class)->create([
            'first_name' => 'Aysel',
            'last_name' => 'Məmmədova',
            'email' => 'aysel@example.com',
        ]);

        $this->actingAs($this->teacher);
        $this->get('/admin/students')->assertOk()->assertSee('Aysel');
    }

    public function test_teacher_can_create_student_via_filament(): void
    {
        $city = City::create([
            'name_translations' => ['az' => 'Bakı', 'en' => 'Baku'],
        ]);

        $this->actingAs($this->teacher);

        Livewire::test(CreateStudent::class)
            ->fillForm([
                'first_name' => 'Elvin',
                'last_name' => 'Əliyev',
                'email' => 'elvin@example.com',
                'password' => 'Elvin123',
                'city_id' => $city->id,
                'birth_date' => '2010-06-01',
                'status' => User::STATUS_ACTIVE,
            ])
            ->call('create')
            ->assertHasNoFormErrors();

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

        Livewire::test(EditStudent::class, ['record' => $student->id])
            ->fillForm([
                'first_name' => 'Nigar',
                'last_name' => 'Quliyeva',
                'email' => 'nigar@example.com',
                'city_id' => $city->id,
                'birth_date' => '2011-02-14',
                'status' => User::STATUS_ACTIVE,
            ])
            ->call('save')
            ->assertHasNoFormErrors();

        $student->refresh();
        $this->assertEquals('Quliyeva', $student->last_name);
        $this->assertEquals($city->id, $student->studentProfile->city_id);
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
}
