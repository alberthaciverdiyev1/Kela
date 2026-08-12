<?php

namespace App\Web\Controllers;

use App\Application\Auth\AuthService;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Auth;
use Illuminate\Validation\ValidationException;

class AuthController extends Controller
{
    public function __construct(private readonly AuthService $auth)
    {
    }

    public function showLogin()
    {
        if (Auth::check()) {
            return redirect(Auth::user()->homeRoute());
        }
        return view('common.auth.login');
    }

    public function login(Request $request)
    {
        $credentials = $request->validate([
            'email' => ['required', 'email'],
            'password' => ['required'],
        ]);

        if (!Auth::attempt($credentials, $request->boolean('remember'))) {
            throw ValidationException::withMessages([
                'email' => __('auth.failed'),
            ]);
        }

        $request->session()->regenerate();

        $user = Auth::user();
        if ($user->status !== 1) {
            Auth::logout();
            throw ValidationException::withMessages([
                'email' => __('Your account is inactive.'),
            ]);
        }

        return redirect()->intended($user->homeRoute());
    }

    /** Müəllim qeydiyyat formu. */
    public function showRegister()
    {
        if (Auth::check()) {
            return redirect(Auth::user()->homeRoute());
        }
        return view('common.auth.register');
    }

    /** Müəllim qeydiyyatı: hesab yaradılır, avtomatik daxil olunur. */
    public function register(Request $request)
    {
        $data = $request->validate([
            'first_name' => ['required', 'string', 'max:100'],
            'last_name' => ['nullable', 'string', 'max:100'],
            'email' => ['required', 'string', 'email', 'max:255'],
            'password' => ['required', 'string', 'min:8', 'confirmed'],
        ]);

        try {
            $user = $this->auth->registerTeacher($data);
        } catch (\InvalidArgumentException $e) {
            throw ValidationException::withMessages([
                'email' => $e->getMessage(),
            ]);
        }

        Auth::login($user);
        $request->session()->regenerate();

        return redirect()->route('teacher.dashboard')->with('success', 'Qeydiyyat tamamlandı. Xoş gəldiniz!');
    }

    public function logout(Request $request)
    {
        Auth::logout();
        $request->session()->invalidate();
        $request->session()->regenerateToken();
        return redirect('/auth/login');
    }
}
