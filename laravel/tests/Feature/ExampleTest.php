<?php

namespace Tests\Feature;

use Tests\TestCase;

class ExampleTest extends TestCase
{
    /**
     * Kök URL varsayılan olarak Filament paneline yönlendirir.
     */
    public function test_the_application_redirects_to_filament(): void
    {
        $this->get('/')
            ->assertRedirect('/admin');
    }

    /**
     * Özel blade giriş sayfası doğrudan erişilebilir kalır.
     */
    public function test_custom_blade_login_is_still_accessible(): void
    {
        $this->get('/auth/login')->assertOk();
    }

    /**
     * Misafir Filament paneline giderse Filament girişine yönlendirilir.
     */
    public function test_guest_hitting_admin_redirects_to_filament_login(): void
    {
        $this->get('/admin')->assertRedirect('/admin/login');
    }
}
