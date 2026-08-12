<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        // Workspace node ağacı kaldırıldı — workspace artıq bir node/course kimi davranır.
        Schema::dropIfExists('nodes');
    }

    public function down(): void
    {
        Schema::create('nodes', function ($table) {
            $table->id();
            $table->foreignId('workspace_id')->constrained('workspaces')->cascadeOnDelete();
            $table->foreignId('teacher_id')->constrained('users')->cascadeOnDelete();
            $table->foreignId('parent_id')->nullable()->constrained('nodes')->nullOnDelete();
            $table->string('name', 200);
            $table->unsignedTinyInteger('kind')->default(0);
            $table->integer('position')->default(0);
            $table->foreignId('content_id')->nullable()->constrained('contents')->nullOnDelete();
            $table->timestamps();
            $table->softDeletes();
            $table->index(['workspace_id', 'parent_id', 'position']);
        });
    }
};
