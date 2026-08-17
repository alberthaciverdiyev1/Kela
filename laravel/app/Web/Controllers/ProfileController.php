<?php

namespace App\Web\Controllers;

use App\Application\Profile\ProfileService;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;

class ProfileController extends Controller
{
    public function __construct(
        private readonly ProfileService $profileService,
    ) {
    }

    public function edit()
    {
        return view('common.profile.edit', [
            'user' => Auth::user(),
        ]);
    }

    public function update(Request $request)
    {
        $validated = $request->validate([
            'first_name' => ['required', 'string', 'max:255'],
            'last_name' => ['nullable', 'string', 'max:255'],
            'email' => ['required', 'string', 'email', 'max:255'],
            'password' => ['nullable', 'string', 'min:8', 'confirmed'],
            'avatar' => ['nullable', 'image', 'max:5120'], // Max 5MB
        ]);

        $this->profileService->updateProfile(Auth::user(), $validated);

        return redirect()->route('profile.edit')->with('status', 'Profil məlumatlarınız uğurla yeniləndi.');
    }
}
