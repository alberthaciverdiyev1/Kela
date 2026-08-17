<?php

namespace App\Console\Commands;

use App\Application\Payment\PaymentService;
use App\Domain\Workspace\Workspace;
use App\Domain\Workspace\WorkspaceRepository;
use Illuminate\Console\Command;

class GenerateMonthlyInvoices extends Command
{
    protected $signature = 'payments:generate {--month= : Siyahı üçün müəyyən ay (YYYY-MM)}';

    protected $description = 'Bütün aktiv siniflər üzrə şagirdlər üçün hər ayın əvvəlində avtomatik qaimə yaradır';

    public function handle(PaymentService $paymentService, WorkspaceRepository $workspaceRepository): int
    {
        $month = $this->option('month') ?: date('Y-m');
        $this->info("{$month} ayı üçün qaimələr yaradılır...");

        $workspaces = $workspaceRepository->allWithStudents();
        $count = 0;

        foreach ($workspaces as $workspace) {
            foreach ($workspace->students as $student) {
                try {
                    $track = $paymentService->generateInvoice(
                        (int) $workspace->teacher_id,
                        (int) $student->id,
                        (int) $workspace->id,
                        $month,
                    );

                    // Yalnız yeni yaradılan qaimələri say (mövcud olanlar keçir).
                    if ($track !== null && $track->wasRecentlyCreated) {
                        $count++;
                    }
                } catch (\Exception $e) {
                    $this->error("Xəta (Tələbə: {$student->id}, Sinif: {$workspace->id}): " . $e->getMessage());
                }
            }
        }

        $this->info("Uğurla yekunlaşdı! Yeni yaranan qaimə sayı: {$count}");

        return self::SUCCESS;
    }
}
