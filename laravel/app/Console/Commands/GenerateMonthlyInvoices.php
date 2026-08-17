<?php

namespace App\Console\Commands;

use Illuminate\Console\Command;

class GenerateMonthlyInvoices extends Command
{
    protected $signature = 'payments:generate {--month= : Siyahı üçün müəyyən ay (YYYY-MM)}';

    /**
     * The console command description.
     *
     * @var string
     */
    protected $description = 'Bütün aktiv siniflər üzrə şagirdlər üçün hər ayın əvvəlində avtomatik qaimə yaradır';

    /**
     * Execute the console command.
     */
    public function handle(\App\Application\Payment\PaymentService $paymentService, \App\Domain\Workspace\WorkspaceRepository $workspaceRepository)
    {
        $month = $this->option('month') ?: date('Y-m');
        $this->info("{$month} ayı üçün qaimələr yaradılır...");

        // Bütün sinifləri tapırıq. 
        // Lakin Kela arxitekturasında Repository və təmiz model yanaşması var, Workspace-ləri id-ləri ilə çəkirik.
        $workspaces = \App\Domain\Workspace\Workspace::with('students')->get();
        $count = 0;

        foreach ($workspaces as $workspace) {
            foreach ($workspace->students as $student) {
                try {
                    $paymentService->generateInvoice($student->id, $workspace->id, $month);
                    $count++;
                } catch (\Exception $e) {
                    $this->error("Xəta (Tələbə: {$student->id}, Sinif: {$workspace->id}): " . $e->getMessage());
                }
            }
        }

        $this->info("Uğurla yekunlaşdı! Yaranan qaimə sayı: {$count}");
    }
}
