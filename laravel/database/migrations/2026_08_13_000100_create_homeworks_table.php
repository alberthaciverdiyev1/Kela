<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('homeworks', function (Blueprint $table) {
            $table->id();
            $table->foreignId('teacher_id')->constrained('users')->cascadeOnDelete();
            $table->string('title', 200);
            $table->string('description', 2000)->nullable();
            $table->boolean('is_published')->default(false);
            $table->timestamps();

            $table->index('teacher_id');
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('homeworks');
    }
};
