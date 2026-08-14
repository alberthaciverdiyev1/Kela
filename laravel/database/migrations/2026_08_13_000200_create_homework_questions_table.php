<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('homework_questions', function (Blueprint $table) {
            $table->id();
            $table->foreignId('homework_id')->constrained('homeworks')->cascadeOnDelete();
            $table->unsignedTinyInteger('type'); // 0=TASK (əl ilə), 1=QUIZ (quizdən)
            $table->unsignedInteger('position')->default(0);
            $table->text('text'); // sual mətni (quiz sualının anlıq görüntüsü)
            $table->string('option_a', 500)->nullable();
            $table->string('option_b', 500)->nullable();
            $table->string('option_c', 500)->nullable();
            $table->string('option_d', 500)->nullable();
            $table->string('option_e', 500)->nullable();
            $table->unsignedTinyInteger('correct_option')->nullable();
            // Mənbə (quizdən götürülən suallar üçün məlumat izi)
            $table->foreignId('source_question_id')->nullable()->constrained('questions')->nullOnDelete();
            $table->foreignId('source_quiz_id')->nullable()->constrained('quizzes', 'content_id')->nullOnDelete();
            $table->timestamps();

            $table->index(['homework_id', 'position']);
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('homework_questions');
    }
};
