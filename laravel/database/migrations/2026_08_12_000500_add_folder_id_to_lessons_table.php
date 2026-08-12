<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('lessons', function (Blueprint $table) {
            $table->foreignId('folder_id')
                ->nullable()
                ->after('teacher_id')
                ->constrained('lesson_folders')
                ->nullOnDelete();
            $table->index('folder_id');
        });
    }

    public function down(): void
    {
        Schema::table('lessons', function (Blueprint $table) {
            $table->dropForeign(['folder_id']);
            $table->dropIndex(['folder_id']);
            $table->dropColumn('folder_id');
        });
    }
};
