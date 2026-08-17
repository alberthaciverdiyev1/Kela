<?php

namespace App\Api\Controllers;

use App\Application\Auth\AuthService;
use App\Api\Resources\UserResource;
use App\Http\Requests\LoginRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Validation\ValidationException;

class AuthController
{
    public function __construct(private readonly AuthService $auth)
    {
    }

    /** POST /auth/login → { token, token_type, user } */
    public function login(LoginRequest $request): JsonResponse
    {
        $credentials = $request->validated();

        if (! $this->auth->validateCredentials($credentials)) {
            throw ValidationException::withMessages([
                'email' => ['Email və ya şifrə yanlışdır.'],
            ]);
        }

        $user = $this->auth->findByEmail($credentials['email']);
        if ($user === null || (int) $user->status !== 1) {
            throw ValidationException::withMessages([
                'email' => ['Hesabınız aktiv deyil.'],
            ]);
        }

        return response()->json([
            'data' => [
                'token' => $this->auth->issueToken($user),
                'token_type' => 'Bearer',
                'user' => new UserResource($user),
            ],
        ]);
    }

    /** GET /auth/me */
    public function me(Request $request): JsonResponse
    {
        return response()->json([
            'data' => new UserResource($request->user()),
        ]);
    }

    /** POST /auth/logout — cari istifadəçinin bütün tokenlərini ləğv edir. */
    public function logout(Request $request): JsonResponse
    {
        $this->auth->revokeTokens($request->user());

        return response()->json(['message' => 'Çıxış edildi.']);
    }
}
