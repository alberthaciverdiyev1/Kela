<?php

namespace Database\Seeders;

use App\Models\User;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;

class DemoUserSeeder extends Seeder
{
    public function run(): void
    {
        // Admin + Teacher (mirrors test.teacher@kela.local, ID 124 in the old app)
        $admin = User::firstOrCreate(
            ['email' => 'test.teacher@kela.local'],
            [
                'first_name' => 'Test',
                'last_name' => 'Teacher',
                'password' => Hash::make('Test123456'),
                'status' => User::STATUS_ACTIVE,
            ]
        );
        $admin->syncRoles(['Admin', 'Teacher']);

        // Plain teacher
        $teacher = User::firstOrCreate(
            ['email' => 'teacher@kela.local'],
            [
                'first_name' => 'Demo',
                'last_name' => 'Teacher',
                'password' => Hash::make('Test123456'),
                'status' => User::STATUS_ACTIVE,
            ]
        );
        $teacher->syncRoles(['Teacher']);

        // Student
        $student = User::firstOrCreate(
            ['email' => 'student@kela.local'],
            [
                'first_name' => 'Demo',
                'last_name' => 'Student',
                'password' => Hash::make('Test123456'),
                'status' => User::STATUS_ACTIVE,
            ]
        );
        $student->syncRoles(['Student']);
    }
}
