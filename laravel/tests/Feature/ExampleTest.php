<?php

namespace Tests\Feature;

use Tests\TestCase;

class ExampleTest extends TestCase
{
    /**
     * Kök URL varsayılan olarak teacher paneline yönlendirir.
     */
    public function test_the_application_redirects_to_teacher_dashboard(): void
    {
        $this->get('/')
            ->assertRedirect('/teacher/dashboard');
    }

    /**
     * Özel blade giriş sayfası doğrudan erişilebilir kalır.
     */
    public function test_custom_blade_login_is_still_accessible(): void
    {
        $this->get('/auth/login')->assertOk();
    }

    /**
     * Misafir teacher paneline giderse özel giriş sayfasına yönlendirilir.
     */
    public function test_guest_hitting_teacher_dashboard_redirects_to_login(): void
    {
        $this->get('/teacher/dashboard')->assertRedirect('/auth/login');
    }
}
