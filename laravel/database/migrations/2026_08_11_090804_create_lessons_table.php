<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('lessons', function (Blueprint $table) {
            $table->foreignId('content_id')->primary()->constrained('contents')->cascadeOnDelete();
            $table->foreignId('teacher_id')->constrained('users')->cascadeOnDelete();
            $table->string('video_path', 500)->nullable();
            $table->string('thumbnail_path', 500)->nullable();
            $table->unsignedInteger('duration_seconds')->default(0);
            $table->boolean('is_published')->default(false);
            $table->unsignedInteger('order_index')->default(0);
            $table->timestamps();

            $table->index('teacher_id');
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('lessons');
    }
};
