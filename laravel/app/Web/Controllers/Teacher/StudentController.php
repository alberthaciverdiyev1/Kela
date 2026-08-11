<?php

namespace App\Web\Controllers\Teacher;

use App\Application\City\CityService;
use App\Application\Student\StudentService;
use App\Domain\User\Values\UserStatus;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;
use Illuminate\Validation\Rule;

/**
 * Şagird səhifələri — server-rendered Blade.
 * Bütün əməliyyatlar StudentService üzərindən; modellərə birbaşa toxunulmur.
 */
class StudentController
{
    public function __construct(
        private readonly StudentService $students,
        private readonly CityService $cities,
    ) {
    }

    public function index(Request $request): View
    {
        $search = (string) $request->string('search');

        return view('teacher.students.index', [
            'students' => $this->students->paginate($search ?: null, 15),
            'search' => $search,
        ]);
    }

    public function create(): View
    {
        return $this->form(null, 'Yeni Şagird', 'Yeni şagird əlavə et', true);
    }

    public function store(Request $request): RedirectResponse
    {
        $data = $this->validated($request, null);

        $this->students->create([
            'first_name' => trim($data['first_name']),
            'last_name' => trim($data['last_name'] ?? '') ?: null,
            'email' => $data['email'],
            'password' => $data['password'],
            'city_id' => ! empty($data['city_id']) ? (int) $data['city_id'] : null,
            'birth_date' => $data['birth_date'] ?: null,
            'status' => (int) $data['status'],
        ]);

        return redirect()->route('teacher.students.index')->with('success', 'Şagird yaradıldı.');
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

    public function update(Request $request, int $student): RedirectResponse
    {
        $data = $this->validated($request, $student);

        $this->students->update($student, [
            'first_name' => trim($data['first_name']),
            'last_name' => trim($data['last_name'] ?? '') ?: null,
            'email' => $data['email'],
            'password' => $data['password'] ?? null,
            'city_id' => ! empty($data['city_id']) ? (int) $data['city_id'] : null,
            'birth_date' => $data['birth_date'] ?: null,
            'status' => (int) $data['status'],
        ]);

        return redirect()->route('teacher.students.index')->with('success', 'Şagird yeniləndi.');
    }

    public function destroy(int $student): RedirectResponse
    {
        $this->students->delete($student);

        return redirect()->route('teacher.students.index')->with('success', 'Şagird silindi.');
    }

    protected function validated(Request $request, ?int $ignoreId): array
    {
        return $request->validate([
            'first_name' => ['required', 'string', 'max:255'],
            'last_name' => ['nullable', 'string', 'max:255'],
            'email' => ['required', 'email', 'max:255', Rule::unique('users', 'email')->ignore($ignoreId)],
            'password' => ['nullable', 'string', 'min:6', 'max:255'],
            'city_id' => ['nullable', 'integer', 'exists:cities,id'],
            'birth_date' => ['nullable', 'date', 'before_or_equal:today'],
            'status' => ['required', 'integer', 'in:1,2,3'],
        ]);
    }

    protected function form(?array $student, string $heading, string $subtitle, bool $creating): View
    {
        return view('teacher.students.form', [
            'heading' => $heading,
            'subtitle' => $subtitle,
            'creating' => $creating,
            'student' => $student,
            'cities' => $this->cities->options(),
            'statuses' => [
                UserStatus::ACTIVE => 'Aktiv',
                UserStatus::INACTIVE => 'Deaktiv',
                UserStatus::SUSPENDED => 'Dayandırılmış',
            ],
        ]);
    }
}
