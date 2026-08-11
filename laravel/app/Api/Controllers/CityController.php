<?php

namespace App\Api\Controllers;

use App\Application\City\CityService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class CityController
{
    public function __construct(private readonly CityService $cities)
    {
    }

    public function index(Request $request): JsonResponse
    {
        return response()->json([
            'data' => $this->cities->options($request->string('locale', 'az')->toString()),
        ]);
    }
}
