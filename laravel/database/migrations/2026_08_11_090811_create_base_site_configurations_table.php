<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('base_site_configurations', function (Blueprint $table) {
            $table->id();
            $table->string('site_name')->nullable();
            $table->string('primary_color')->nullable();
            $table->string('secondary_color')->nullable();
            $table->string('success_color')->nullable();
            $table->string('warning_color')->nullable();
            $table->string('error_color')->nullable();
            $table->string('info_color')->nullable();
            $table->string('nav_mode')->default('navbar');
            $table->string('notification_provider')->default('sweetalert');
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('base_site_configurations');
    }
};
