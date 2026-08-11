<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class BaseSiteConfiguration extends Model
{
    protected $fillable = [
        'site_name',
        'primary_color',
        'secondary_color',
        'success_color',
        'warning_color',
        'error_color',
        'info_color',
        'nav_mode',
        'notification_provider',
    ];

    /** The singleton config row (only one row is used). */
    public static function get(): ?self
    {
        return static::query()->latest('id')->first();
    }
}
