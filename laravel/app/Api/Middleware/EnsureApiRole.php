<?php

namespace App\Api\Middleware;

use Closure;
use Illuminate\Http\Request;
use Symfony\Component\HttpFoundation\Response;

/**
 * API üçün rol məhdudiyyəti.
 * Web üçün EnsureRole redirect edir; API JSON 403 gözləyir.
 */
class EnsureApiRole
{
    public function handle(Request $request, Closure $next, string ...$roles): Response
    {
        $user = $request->user();

        foreach ($roles as $role) {
            if ($user?->hasRole($role)) {
                return $next($request);
            }
        }

        return response()->json(
            ['message' => 'Bu əməliyyat üçün icazəniz yoxdur.'],
            403,
        );
    }
}
