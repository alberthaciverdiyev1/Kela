<?php

namespace App\Application\Workspace;

use App\Domain\Student\StudentRepository;
use App\Domain\User\User;
use App\Domain\Workspace\Workspace;
use App\Domain\Workspace\WorkspaceRepository;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;

/**
 * Workspace əməliyyatları: CRUD + tələbə əlaqəsi.
 */
class WorkspaceService
{
    public function __construct(
        private readonly WorkspaceRepository $workspaces,
        private readonly StudentRepository $students,
    ) {
    }

    /** Teacher-in workspaceləri: [id, name, student_count, created_at]. */
    public function listForTeacher(int $teacherId): array
    {
        return $this->workspaces->listForTeacher($teacherId)
            ->map(fn (Workspace $w) => [
                'id' => $w->id,
                'name' => $w->name,
                'student_count' => (int) $w->students_count,
                'created_at' => $w->created_at?->toDateString(),
            ])
            ->values()
            ->all();
    }

    public function find(int $workspaceId): ?Workspace
    {
        return $this->workspaces->find($workspaceId);
    }

    /** Cədvəl üçün istifadəçinin görə biləcəyi workspacelərlə məhdud sorğu. */
    public function scopeQueryFor(Builder $query, int $actingUserId): Builder
    {
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;

        return $this->workspaces->scopeForUser($query, $actingUserId, $isAdmin);
    }

    /** Dropdown üçün istifadəçinin görə biləcəyi bütün workspacelər (admin → hamısı, müəllim → özü). */
    public function listForUser(int $actingUserId): array
    {
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;

        return $this->workspaces->listForUser($actingUserId, $isAdmin)
            ->map(fn (Workspace $w) => [
                'id' => (int) $w->id,
                'name' => $w->name,
                'student_count' => (int) $w->students_count,
            ])
            ->values()
            ->all();
    }

    /** Şagirdin üzv olduğu workspacelər (müəllim özününküləri görür, admin hamısını). */
    public function listForStudent(int $actingUserId, int $studentId): array
    {
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;

        return $this->workspaces->forStudent($isAdmin ? null : $actingUserId, $studentId)
            ->map(fn (Workspace $w) => [
                'id' => (int) $w->id,
                'name' => $w->name,
                'student_count' => (int) $w->students_count,
            ])
            ->values()
            ->all();
    }

    /** Admin cədvəli üçün axtarış + səhifələnmiş workspace siyahısı (array). */
    public function paginate(int $actingUserId, ?string $search = null, int $perPage = 15): LengthAwarePaginator
    {
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;

        return $this->workspaces
            ->paginateForUser($actingUserId, $isAdmin, $search, $perPage)
            ->through(fn (Workspace $w): array => [
                'id' => (int) $w->id,
                'name' => $w->name,
                'student_count' => (int) $w->students_count,
                'created_at' => $w->created_at?->toDateString(),
            ]);
    }

    /** Workspace redaktor formu üçün ad + yaradılma tarixi. */
    public function formData(int $workspaceId): array
    {
        $workspace = $this->workspaces->find($workspaceId);
        if ($workspace === null) {
            return [];
        }

        return [
            'name' => $workspace->name,
            'created_at' => $workspace->created_at?->toDateString(),
        ];
    }

    public function create(int $teacherId, string $name): Workspace
    {
        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Workspace adı boş ola bilməz.');
        }

        return $this->workspaces->create($teacherId, $name);
    }

    public function rename(int $teacherId, int $workspaceId, string $name): void
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);
        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Ad boş ola bilməz.');
        }

        $this->workspaces->update($workspace, $name);
    }

    public function delete(int $teacherId, int $workspaceId): void
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);
        $this->workspaces->delete($workspace);
    }

    // --- Tələbələr ---

    public function attachStudents(int $teacherId, int $workspaceId, array $studentIds, ?float $agreedPrice = null, ?string $startDate = null): void
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);
        
        $attributes = [];
        if ($agreedPrice !== null) {
            $attributes['agreed_price'] = $agreedPrice;
        }
        if ($startDate !== null) {
            $attributes['start_date'] = $startDate;
        }
        
        $this->workspaces->attachStudents($workspace, $studentIds, $attributes);
        
        // Avtomatik olaraq cari ay üçün qaimə yaradaq
        $paymentService = app(\App\Application\Payment\PaymentService::class);
        $month = date('Y-m');
        foreach ($studentIds as $studentId) {
            try {
                $paymentService->generateInvoice($studentId, $workspaceId, $month, $agreedPrice);
            } catch (\Exception $e) {
                // Səssizcə keçə bilərik, bəlkə onsuz da var idi və s.
            }
        }
    }

    public function detachStudent(int $teacherId, int $workspaceId, int $studentId): void
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);
        $this->workspaces->detachStudent($workspace, $studentId);
    }

    /** Workspace-də olmayan (əlavə edilə bilən) tələbələr. */
    public function availableStudents(int $teacherId, int $workspaceId, ?string $search = null): array
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);
        $existing = $this->workspaces->studentIds($workspace);

        return $this->students->availableForWorkspace($search)
            ->reject(fn ($student) => in_array($student->id, $existing, true))
            ->map(fn ($student) => [
                'id' => $student->id,
                'name' => trim(($student->first_name ?? '').' '.($student->last_name ?? '')),
                'email' => $student->email,
            ])
            ->values()
            ->all();
    }

    /** Workspace-dəki tələbələr. */
    public function studentList(int $teacherId, int $workspaceId): array
    {
        $workspace = $this->assertOwned($teacherId, $workspaceId);

        return $this->workspaces->students($workspace)
            ->map(fn ($student) => [
                'id' => $student->id,
                'name' => trim(($student->first_name ?? '').' '.($student->last_name ?? '')),
                'email' => $student->email,
            ])
            ->values()
            ->all();
    }

    protected function assertOwned(int $teacherId, int $workspaceId): Workspace
    {
        $workspace = $this->workspaces->find($workspaceId);
        if ($workspace === null || $workspace->teacher_id !== $teacherId) {
            throw new \RuntimeException('Workspace tapılmadı.');
        }

        return $workspace;
    }
}
