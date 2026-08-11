<?php

namespace App\Application\City;

use App\Domain\City\CityRepository;
use Illuminate\Support\Collection;

/**
 * Şəhərlərlə bağlı tətbiq səviyyəli əməliyyatlar.
 * Filament bu servisi çağırır — City modelinə birbaşa toxunmaz.
 */
class CityService
{
    public function __construct(private readonly CityRepository $cities)
    {
    }

    /** Form seçimi üçün id => ad (lokallaşdırılmış) cütləri. */
    public function options(string $locale = 'az'): array
    {
        return $this->cities->all()
            ->mapWithKeys(fn ($city) => [$city->id => $city->name($locale)])
            ->all();
    }

    /** @return Collection<int, \App\Domain\City\City> */
    public function all(): Collection
    {
        return $this->cities->all();
    }
}
