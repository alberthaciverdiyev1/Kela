<?php

namespace App\Domain\City;

use Illuminate\Support\Collection;

interface CityRepository
{
    /** @return Collection<int, City> */
    public function all(): Collection;
}
