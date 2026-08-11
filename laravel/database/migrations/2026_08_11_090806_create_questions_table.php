<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('questions', function (Blueprint $table) {
            $table->id();
            $table->foreignId('teacher_id')->constrained('users')->cascadeOnDelete();
            $table->text('text');
            $table->string('option_a', 500)->nullable();
            $table->string('option_b', 500)->nullable();
            $table->string('option_c', 500)->nullable();
            $table->string('option_d', 500)->nullable();
            $table->string('option_e', 500)->nullable();
            $table->unsignedTinyInteger('correct_option')->nullable();
            $table->timestamps();
            $table->softDeletes();

            $table->index('teacher_id');
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('questions');
    }
};
