<?php

namespace App\Application\Auth;

use App\Domain\User\User;
use App\Domain\User\UserRepository;
use Illuminate\Support\Facades\Auth;

/**
 * API giriş/çıxış/register use-case-ləri.
 * Controller modellərə toxunmaz; bütün məlumat bu servis üzərindən axır.
 */
class AuthService
{
    public function __construct(
        private readonly UserRepository $users,
    ) {
    }

    /** Email + şifrə doğrudurmu? (session yaradılmır — yalnız yoxlama). */
    public function validateCredentials(array $credentials): bool
    {
        return Auth::validate($credentials);
    }

    public function findByEmail(string $email): ?User
    {
        return $this->users->findByEmail($email);
    }

    /** Yeni müəllim hesabı yaradır (aktiv status + Teacher rolu). */
    public function registerTeacher(array $data): User
    {
        $email = strtolower(trim($data['email']));
        if ($this->users->findByEmail($email) !== null) {
            throw new \InvalidArgumentException('Bu e-poçt artıq istifadə olunur.');
        }

        return $this->users->createTeacher([
            'first_name' => trim($data['first_name']),
            'last_name' => isset($data['last_name']) ? trim($data['last_name']) : null,
            'email' => $email,
            'password' => $data['password'],
        ]);
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
