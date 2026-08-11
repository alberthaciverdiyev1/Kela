<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\City\City;
use App\Domain\City\CityRepository;
use Illuminate\Support\Collection;

class EloquentCityRepository implements CityRepository
{
    public function all(): Collection
    {
        return City::orderBy('id')->get();
    }
}
