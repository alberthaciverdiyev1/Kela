<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::create('payment_reminders', function (Blueprint $table) {
            $table->id();
            $table->foreignId('payment_track_id')->constrained('student_payment_tracks')->cascadeOnDelete();
            $table->string('type', 20); // upcoming | due
            $table->text('message');
            $table->timestamp('sent_at')->nullable();
            $table->timestamps();

            // Hər track üçün hər tipdən yalnız 1 bildiriş — saatlıq cron təkrar göndərməz.
            $table->unique(['payment_track_id', 'type']);
        });
    }

    public function down(): void
    {
        Schema::dropIfExists('payment_reminders');
    }
};
