<?php

namespace App\Web\Controllers\Teacher;

use App\Application\City\CityService;
use App\Application\Student\StudentService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\User\Enums\UserStatus;
use App\Http\Requests\StudentRequest;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Şagird səhifələri — server-rendered Blade.
 * Bütün əməliyyatlar StudentService üzərindən; modellərə birbaşa toxunulmur.
 */
class StudentController
{
    public function __construct(
        private readonly StudentService $students,
        private readonly WorkspaceService $workspaces,
        private readonly CityService $cities,
    ) {
    }

    public function index(Request $request): View
    {
        $search = (string) $request->string('search');
        $students = $this->students->paginate($search ?: null, 15);

        return view('teacher.students.index', [
            'students' => $students,
            'search' => $search,
            'cities' => $this->cities->options(),
            'statuses' => $this->statuses(),
            'fragmentUrl' => route('teacher.students.table', array_filter([
                'search' => $search ?: null,
                'page' => $students->currentPage() > 1 ? $students->currentPage() : null,
            ])),
        ]);
    }

    /**
     * Cədvəl fragmenti — JS list.refresh() tərəfindən yenidən çəkilir
     * (server-rendered; mutasiyadan sonra yalnız bu hissə yenilənir).
     */
    public function tableFragment(Request $request): View
    {
        $search = (string) $request->string('search');

        return view('teacher.students._table', [
            'students' => $this->students->paginate($search ?: null, 15),
        ]);
    }

    public function create(): View
    {
        return $this->form(null, 'Yeni Şagird', 'Yeni şagird əlavə et', true);
    }

    public function store(StudentRequest $request): RedirectResponse|\Illuminate\Http\JsonResponse
    {
        $data = $request->validated();

        $this->students->create([
            'first_name' => trim($data['first_name']),
            'last_name' => trim($data['last_name'] ?? '') ?: null,
            'email' => $data['email'],
            'password' => $data['password'],
            'city_id' => ! empty($data['city_id']) ? (int) $data['city_id'] : null,
            'birth_date' => $data['birth_date'] ?: null,
            'status' => (int) $data['status'],
        ]);

        if ($request->expectsJson()) {
            return response()->json(['message' => 'Şagird yaradıldı.']);
        }

        return redirect()->route('teacher.students.index')->with('success', 'Şagird yaradıldı.');
    }

    /** Şagirdin ətraflı profili: şəxsi məlumat + üzv olduğu workspacelər. */
    public function show(int $student): View
    {
        $data = $this->students->formData($student);
        if ($data === []) {
            abort(404);
        }

        $cities = $this->cities->options();
        $statuses = $this->statuses();

        return view('teacher.students.show', [
            'student' => [
                'id' => $student,
                'first_name' => $data['first_name'] ?? '',
                'last_name' => $data['last_name'] ?? null,
                'full_name' => trim(($data['first_name'] ?? '').' '.($data['last_name'] ?? '')),
                'email' => $data['email'] ?? '',
                'status' => (int) ($data['status'] ?? UserStatus::ACTIVE),
                'status_label' => $statuses[(int) ($data['status'] ?? UserStatus::ACTIVE)] ?? '—',
                'city' => ! empty($data['city_id']) ? ($cities[(int) $data['city_id']] ?? '—') : '—',
                'birth_date' => $data['birth_date'] ?? null,
            ],
            'workspaces' => $this->workspaces->listForStudent((int) auth()->id(), $student),
        ]);
    }

    public function edit(int $student): View
    {
        $data = $this->students->formData($student);
        if ($data === []) {
            abort(404);
        }
        $data['id'] = $student;

        return $this->form($data, 'Şagirdi Redaktə Et', trim(($data['first_name'] ?? '').' '.($data['last_name'] ?? '')), false);
    }

    public function update(StudentRequest $request, int $student): RedirectResponse|\Illuminate\Http\JsonResponse
    {
        $data = $request->validated();

        $this->students->update($student, [
            'first_name' => trim($data['first_name']),
            'last_name' => trim($data['last_name'] ?? '') ?: null,
            'email' => $data['email'],
            'password' => $data['password'] ?? null,
            'city_id' => ! empty($data['city_id']) ? (int) $data['city_id'] : null,
            'birth_date' => $data['birth_date'] ?: null,
            'status' => (int) $data['status'],
        ]);

        if ($request->expectsJson()) {
            return response()->json(['message' => 'Şagird yeniləndi.']);
        }

        return redirect()->route('teacher.students.index')->with('success', 'Şagird yeniləndi.');
    }

    public function destroy(Request $request, int $student): RedirectResponse|\Illuminate\Http\JsonResponse
    {
        $this->students->delete($student);

        if ($request->expectsJson()) {
            return response()->json(['message' => 'Şagird silindi.']);
        }

        return redirect()->route('teacher.students.index')->with('success', 'Şagird silindi.');
    }

    protected function form(?array $student, string $heading, string $subtitle, bool $creating): View
    {
        return view('teacher.students.form', [
            'heading' => $heading,
            'subtitle' => $subtitle,
            'creating' => $creating,
            'student' => $student,
            'cities' => $this->cities->options(),
            'statuses' => $this->statuses(),
        ]);
    }

    protected function statuses(): array
    {
        return [
            UserStatus::ACTIVE->value => 'Aktiv',
            UserStatus::INACTIVE->value => 'Deaktiv',
            UserStatus::SUSPENDED->value => 'Dayandırılmış',
        ];
    }
}
