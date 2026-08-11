<?php

namespace Tests\Feature;

use Tests\TestCase;

class ExampleTest extends TestCase
{
    /**
     * Kök URL giriş sayfasına yönlendirir.
     */
    public function test_the_application_redirects_to_login(): void
    {
        $this->get('/')
            ->assertRedirect('/auth/login');
    }
}
