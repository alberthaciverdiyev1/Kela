<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('contents', function (Blueprint $table) {
            $table->id();
            $table->foreignId('teacher_id')->constrained('users')->cascadeOnDelete();
            $table->string('title', 200);
            $table->string('description', 2000)->nullable();
            $table->unsignedTinyInteger('type'); // 0=Lesson, 1=Quiz, 2=Pdf, 3=Video, 4=Link
            $table->string('url', 500)->nullable();
            $table->boolean('is_published')->default(false);
            $table->timestamps();
            $table->softDeletes();

            $table->index(['teacher_id', 'type']);
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('contents');
    }
};
