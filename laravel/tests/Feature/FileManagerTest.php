<?php

namespace Tests\Feature;

use App\Domain\User\User;
use Filament\Facades\Filament;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Livewire\Livewire;
use MmesDesign\FilamentFileManager\Filament\Pages\FileManagerPage;
use MmesDesign\FilamentFileManager\Services\FileManagerService;
use MmesDesign\FilamentFileManager\Services\ThumbnailService;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class FileManagerTest extends TestCase
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

    public function test_file_manager_page_renders_without_gd(): void
    {
        // GD olmayan ortamda SafeThumbnailService çözümlenebilmeli.
        $service = app(ThumbnailService::class);
        $this->assertInstanceOf(\App\Infrastructure\Media\SafeThumbnailService::class, $service);
        $this->assertNull($service->getThumbnailUrl('local', 'foo.jpg'));

        $this->actingAs($this->teacher);
        $this->get('/admin/file-manager')->assertOk();

        Livewire::test(FileManagerPage::class)->assertOk();
    }

    public function test_file_manager_plugin_access_is_role_restricted(): void
    {
        $this->actingAs($this->teacher);
        $this->get('/admin/file-manager')->assertOk();

        // Öğrenci erişemez.
        $student = User::factory()->create();
        $student->assignRole(User::ROLE_STUDENT);
        $this->actingAs($student);
        $this->get('/admin/file-manager')->assertForbidden();
    }

    public function test_file_manager_lists_directory_via_service(): void
    {
        $this->actingAs($this->teacher);

        $service = app(FileManagerService::class);
        $listing = $service->listDirectory('local', '');

        $this->assertNotNull($listing);
        $this->assertEquals('', $listing->path);
        $this->assertIsArray($listing->folders);
        $this->assertIsArray($listing->files);
    }
}
