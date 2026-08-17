<?php

namespace App\Api\Controllers;

use App\Application\Student\StudentService;
use App\Api\Resources\StudentResource;
use App\Domain\User\User;
use App\Http\Requests\StudentRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Pagination\LengthAwarePaginator;

class StudentController
{
    public function __construct(private readonly StudentService $students)
    {
    }

    /** StudentService::paginate array DTO döndürür → birbaşa JSON. */
    public function index(Request $request): LengthAwarePaginator
    {
        return $this->students->paginate(
            $request->string('search')->toString() ?: null,
            (int) $request->integer('per_page', 15),
        );
    }

    public function store(StudentRequest $request): JsonResponse
    {
        $data = $request->validated();

        $student = $this->students->create($data);

        return (new StudentResource($student))->response()->setStatusCode(201);
    }

    public function show(int $student): StudentResource
    {
        $student = $this->students->find($student);
        if ($student === null || ! $student->hasRole(User::ROLE_STUDENT)) {
            abort(404);
        }

        return new StudentResource($student);
    }

    public function update(StudentRequest $request, int $student): StudentResource
    {
        $data = $request->validated();

        $model = $this->students->update($student, $data);

        return new StudentResource($model);
    }

    public function destroy(int $student): JsonResponse
    {
        $this->students->delete($student);

        return response()->json(['message' => 'Şagird silindi.']);
    }
}
