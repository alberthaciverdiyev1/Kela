<?php

namespace App\Jobs;

use App\Application\Student\StudentExport;
use App\Application\Student\StudentService;
use App\Application\Workspace\WorkspaceService;
use Illuminate\Contracts\Queue\ShouldQueue;
use Illuminate\Foundation\Queue\Queueable;

/**
 * Toplu şagird generasiyası — queue arxa planda işləyir.
 * Yalnız say ötürülür; adlar, e-poçtlar və şifrələr avtomatik generasiya olunur.
 * workspaceId verilərsə şagirdlər sinifə əlavə olunur (avtomatik qaimə ilə).
 */
class GenerateStudentsJob implements ShouldQueue
{
    use Queueable;

    public int $timeout = 300;

    public int $tries = 1;

    public function __construct(
        public int $teacherId,
        public int $count,
        public ?int $workspaceId = null,
    ) {
    }

    public function handle(StudentService $students, WorkspaceService $workspaces): void
    {
        StudentExport::markRunning($this->teacherId);

        try {
            $result = $students->generateMany(['count' => $this->count]);

            if ($this->workspaceId !== null) {
                $workspaces->attachStudents(
                    $this->teacherId,
                    $this->workspaceId,
                    $result['users']->pluck('id')->all(),
                );
            }

            StudentExport::store($this->teacherId, $result['rows']);
        } catch (\Throwable $e) {
            StudentExport::markFailed($this->teacherId);
            report($e);
            throw $e;
        } finally {
            StudentExport::markRunning($this->teacherId, false);
        }
    }
}
