<?php

use Illuminate\Foundation\Application;
use Illuminate\Foundation\Configuration\Exceptions;
use Illuminate\Foundation\Configuration\Middleware;
use Illuminate\Http\Request;

return Application::configure(basePath: dirname(__DIR__))
    ->withRouting(
        web: __DIR__.'/../routes/web.php',
        api: __DIR__.'/../routes/api.php',
        apiPrefix: 'api/v1',
        commands: __DIR__.'/../routes/console.php',
        health: '/up',
    )
    ->withMiddleware(function (Middleware $middleware): void {
        $middleware->alias([
            'role' => \App\Web\Middleware\EnsureRole::class,
            'role_api' => \App\Api\Middleware\EnsureApiRole::class,
        ]);

        // Doğrulamasız kullanıcıyı doğrudan giriş sayfasına yönlendir.
        $middleware->redirectGuestsTo('/auth/login');

        // Browser-dəki JS (sessiya ilə auth) /api/v1/* çağırdıqda Sanctum-un
        // session-u tanıması üçün stateful middleware lazımdır. Bunun sayəsində
        // auth:sanctum web guard vasitəsilə session istifadəçisini doğrulayır
        // (Bearer token tələb olunmadan) — əks halda 401 Unauthenticated olur.
        $middleware->api(prepend: [
            \Laravel\Sanctum\Http\Middleware\EnsureFrontendRequestsAreStateful::class,
        ]);
    })
    ->withExceptions(function (Exceptions $exceptions): void {
        $exceptions->shouldRenderJsonWhen(
            fn (Request $request) => $request->is('api/*'),
        );
    })->create();
