<?php

namespace App\Application\Profile;

use App\Domain\User\User;
use App\Domain\User\UserRepository;
use Illuminate\Validation\ValidationException;

/**
 * İstifadəçi profilinin yenilənməsi üçün Application Service.
 */
class ProfileService
{
    public function __construct(
        private readonly UserRepository $users,
    ) {
    }

    /**
     * Mövcud istifadəçinin profilini yeniləyir.
     * 
     * @param User $user
     * @param array $data ['first_name', 'last_name', 'email', 'password']
     * @return User
     */
    public function updateProfile(User $user, array $data): User
    {
        $updateData = [];

        if (isset($data['first_name'])) {
            $updateData['first_name'] = trim($data['first_name']);
        }

        if (array_key_exists('last_name', $data)) {
            $updateData['last_name'] = $data['last_name'] !== null ? trim($data['last_name']) : null;
        }

        if (isset($data['email'])) {
            $email = strtolower(trim($data['email']));
            
            if ($email !== $user->email) {
                $existingUser = $this->users->findByEmail($email);
                if ($existingUser !== null && $existingUser->id !== $user->id) {
                    throw ValidationException::withMessages([
                        'email' => ['Bu e-poçt ünvanı artıq başqa istifadəçi tərəfindən istifadə olunur.'],
                    ]);
                }
            }
            $updateData['email'] = $email;
        }

        if (!empty($data['password'])) {
            $updateData['password'] = $data['password'];
        }

        if (isset($data['avatar']) && $data['avatar'] instanceof \Illuminate\Http\UploadedFile) {
            // Kohne avatari sil
            if ($user->avatar) {
                \Illuminate\Support\Facades\Storage::disk('public')->delete($user->avatar);
            }
            
            $path = $data['avatar']->store('avatars', 'public');
            $updateData['avatar'] = $path;
        }

        return $this->users->update($user, $updateData);
    }
}
