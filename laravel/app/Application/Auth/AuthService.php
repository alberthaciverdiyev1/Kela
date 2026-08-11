<?php

namespace App\Application\Auth;

use App\Domain\User\User;
use Illuminate\Support\Facades\Auth;

/**
 * API giriş/çıxış use-case-ləri.
 * Controller modellərə toxunmaz; bütün məlumat bu servis üzərindən axır.
 */
class AuthService
{
    /** Email + şifrə doğrudurmu? (session yaradılmır — yalnız yoxlama). */
    public function validateCredentials(array $credentials): bool
    {
        return Auth::validate($credentials);
    }

    public function findByEmail(string $email): ?User
    {
        return User::where('email', $email)->first();
    }

    /** İstifadəçiyə yeni API (Bearer) tokeni verir. */
    public function issueToken(User $user, string $device = 'api-client'): string
    {
        return $user->createToken($device)->plainTextToken;
    }

    /** İstifadəçinin bütün tokenlərini ləğv edir (logout). */
    public function revokeTokens(User $user): void
    {
        $user->tokens()->delete();
    }
}
