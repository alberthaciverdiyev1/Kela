<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('workspace_students', function (Blueprint $table) {
            $table->foreignId('student_id')->constrained('users')->cascadeOnDelete();
            $table->foreignId('workspace_id')->constrained('workspaces')->cascadeOnDelete();
            $table->primary(['student_id', 'workspace_id']);
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('workspace_students');
    }
};
