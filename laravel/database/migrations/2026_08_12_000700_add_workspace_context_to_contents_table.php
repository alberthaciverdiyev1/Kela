<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('contents', function (Blueprint $table) {
            $table->foreignId('workspace_id')
                ->nullable()
                ->after('teacher_id')
                ->constrained('workspaces')
                ->nullOnDelete();
            $table->foreignId('folder_id')
                ->nullable()
                ->after('workspace_id')
                ->constrained('workspace_folders')
                ->nullOnDelete();
            $table->index(['workspace_id', 'folder_id']);
        });
    }

    public function down(): void
    {
        Schema::table('contents', function (Blueprint $table) {
            $table->dropForeign(['folder_id']);
            $table->dropForeign(['workspace_id']);
            $table->dropIndex(['workspace_id', 'folder_id']);
            $table->dropColumn(['workspace_id', 'folder_id']);
        });
    }
};
