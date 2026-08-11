<?php

namespace App\Domain\City;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class City extends Model
{
    use SoftDeletes;

    protected $fillable = ['name_translations'];

    protected function casts(): array
    {
        return [
            'name_translations' => 'array',
        ];
    }

    /** Name in the given locale, falling back to az/en/first value. */
    public function name(string $locale = 'az'): string
    {
        $translations = $this->name_translations ?? [];
        return $translations[$locale]
            ?? $translations['az']
            ?? $translations['en']
            ?? (is_array($translations) ? reset($translations) : '');
    }
}
