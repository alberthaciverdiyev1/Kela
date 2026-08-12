<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        Schema::table('questions', function (Blueprint $table) {
            // Cavabın izahı (öyrənmə üçün — isteğe bağlı)
            $table->text('explanation')->nullable()->after('correct_option');
            // Sualın balı (1-100)
            $table->unsignedSmallInteger('points')->default(1)->after('explanation');
            // Çətinlik: easy | medium | hard
            $table->string('difficulty', 20)->default('medium')->after('points');
        });
    }

    public function down(): void
    {
        Schema::table('questions', function (Blueprint $table) {
            $table->dropColumn(['explanation', 'points', 'difficulty']);
        });
    }
};
