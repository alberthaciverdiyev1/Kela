<?php

namespace Tests\Feature;

use App\Application\Student\StudentService;
use App\Domain\City\City;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class StudentServiceTest extends TestCase
{
    use RefreshDatabase;

    protected function setUp(): void
    {
        parent::setUp();

        foreach (User::ALL_ROLES as $role) {
            Role::firstOrCreate(['name' => $role]);
        }
    }

    public function test_create_student_creates_user_with_student_role(): void
    {
        $student = app(StudentService::class)->create([
            'first_name' => 'Aysel',
            'last_name' => 'Məmmədova',
            'email' => 'aysel@example.com',
            'password' => 'Secret123',
        ]);

        $this->assertInstanceOf(User::class, $student);
        $this->assertTrue($student->hasRole(User::ROLE_STUDENT));
        $this->assertEquals('Aysel', $student->first_name);
        $this->assertEquals(1, $student->status);
    }

    public function test_create_student_with_profile_creates_student_profile(): void
    {
        $city = City::create([
            'name' => 'Bakı',
            'name_translations' => ['az' => 'Bakı', 'en' => 'Baku'],
        ]);

        $student = app(StudentService::class)->create([
            'first_name' => 'Elvin',
            'email' => 'elvin@example.com',
            'city_id' => $city->id,
            'birth_date' => '2010-05-15',
        ]);

        $this->assertNotNull($student->studentProfile);
        $this->assertEquals($city->id, $student->studentProfile->city_id);
        $this->assertEquals('2010-05-15', $student->studentProfile->birth_date->format('Y-m-d'));
    }

    public function test_list_returns_only_students(): void
    {
        app(StudentService::class)->create([
            'first_name' => 'Nigar',
            'email' => 'nigar@example.com',
        ]);

        // Teacher kullanıcısı listeye karışmamalı.
        $teacher = User::factory()->create();
        $teacher->assignRole(User::ROLE_TEACHER);

        $students = app(StudentService::class)->list();

        $this->assertCount(1, $students);
        $this->assertEquals('Nigar', $students->first()->first_name);
    }

    public function test_delete_student_removes_user_and_soft_deletes_profile(): void
    {
        $student = app(StudentService::class)->create([
            'first_name' => 'Leyla',
            'email' => 'leyla@example.com',
            'birth_date' => '2011-03-20',
        ]);

        $id = $student->id;

        app(StudentService::class)->delete($id);

        // User soft-delete edilir; profile da soft-delete olur.
        $this->assertNull(User::find($id));
        $this->assertSoftDeleted('users', ['id' => $id]);
        $this->assertSoftDeleted('student_profiles', ['user_id' => $id]);
    }
}
