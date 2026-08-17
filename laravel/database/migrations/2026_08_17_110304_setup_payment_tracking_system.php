<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        // 1. Add monthly_price to workspaces
        Schema::table('workspaces', function (Blueprint $table) {
            $table->decimal('monthly_price', 10, 2)->nullable()->after('name');
        });

        // 2. Add agreed_price to workspace_students
        Schema::table('workspace_students', function (Blueprint $table) {
            $table->decimal('agreed_price', 10, 2)->nullable();
        });

        // 3. Recreate student_payment_tracks to have the right structure
        Schema::dropIfExists('student_payment_tracks');
        Schema::create('student_payment_tracks', function (Blueprint $table) {
            $table->id();
            $table->foreignId('student_id')->constrained('users')->cascadeOnDelete();
            $table->foreignId('workspace_id')->constrained('workspaces')->cascadeOnDelete();
            
            // Ay formatı: "2026-09"
            $table->string('month', 7);
            
            $table->decimal('total_amount', 10, 2);
            $table->decimal('paid_amount', 10, 2)->default(0);
            
            $table->unsignedTinyInteger('status')->default(0); // 0: Unpaid, 1: Partial, 2: Paid
            $table->timestamp('due_date')->nullable();
            
            $table->timestamps();
            $table->softDeletes();
            
            // Bir tələbə bir sinifdə bir ay üçün yalnız 1 qaimə (track) sahibi ola bilər
            $table->unique(['student_id', 'workspace_id', 'month']);
        });

        // 4. Create student_payment_transactions for partial payments
        Schema::create('student_payment_transactions', function (Blueprint $table) {
            $table->id();
            $table->foreignId('payment_track_id')->constrained('student_payment_tracks')->cascadeOnDelete();
            $table->decimal('amount', 10, 2);
            $table->timestamp('paid_at')->useCurrent();
            $table->text('note')->nullable();
            $table->timestamps();
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('student_payment_transactions');
        
        Schema::dropIfExists('student_payment_tracks');
        Schema::create('student_payment_tracks', function (Blueprint $table) {
            $table->id();
            $table->foreignId('student_id')->constrained('users')->cascadeOnDelete();
            $table->decimal('amount', 10, 2);
            $table->timestamp('due_date')->nullable();
            $table->unsignedTinyInteger('status')->default(0);
            $table->timestamps();
            $table->softDeletes();
        });

        Schema::table('workspace_students', function (Blueprint $table) {
            $table->dropColumn('agreed_price');
        });

        Schema::table('workspaces', function (Blueprint $table) {
            $table->dropColumn('monthly_price');
        });
    }
};
