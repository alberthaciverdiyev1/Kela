<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('nodes', function (Blueprint $table) {
            $table->id();
            $table->foreignId('workspace_id')->nullable()->constrained('workspaces')->cascadeOnDelete();
            $table->foreignId('teacher_id')->nullable()->constrained('users')->cascadeOnDelete();
            $table->foreignId('parent_id')->nullable()->constrained('nodes')->cascadeOnDelete();
            $table->string('name', 200);
            $table->unsignedTinyInteger('kind'); // 0=Folder, 1=Content
            $table->integer('position')->default(0);
            $table->foreignId('content_id')->nullable()->constrained('contents')->cascadeOnDelete();
            $table->timestamps();
            $table->softDeletes();

            $table->index(['teacher_id', 'parent_id', 'position']);
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('nodes');
    }
};
