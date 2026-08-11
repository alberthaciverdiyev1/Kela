<?php

namespace App\Infrastructure\Media;

use MmesDesign\FilamentFileManager\Services\ThumbnailService;

/**
 * GD'siz ortamlar için ThumbnailService.
 *
 * Varsayılan paket ThumbnailService, yapıcı metotta doğrudan
 * `new ImageManager(new Gd\Driver)` kurar. Bu sistemde GD PHP uzantısı
 * (ve bağımlılığı libgd) yüklü değil ve sudo olmadığı için php-gd
 * kurulamıyor. ImageManager kurulumu o anda "GD PHP extension must be
 * installed" hatasıyla patladığından, bu alt sınıf görsel kütüphanesini
 * hiç başlatmaz ve tüm küçük resim isteklerini kapalı tutar.
 *
 * FileManagerService container'dan ThumbnailService çözümlediğinde bu
 * sınıf devreye girer; dosya yöneticisi tam çalışır, yalnızca küçük
 * resimler (image thumbnails) üretilmez.
 */
class SafeThumbnailService extends ThumbnailService
{
    /**
     * GD ImageManager'ı hiç başlatma.
     */
    public function __construct()
    {
        // Parent constructor, Intervention ImageManager'ı GD driver ile kurar.
        // Onu çağırmıyoruz; böylece GD olmasa bile servis sorunsuz çözülür.
        // $this->imageManager'a asla erişilmez çünkü isEnabled() her zaman false.
    }

    /**
     * Küçük resimler her zaman kapalıdır (GD olmadan üretilemez).
     */
    protected function isEnabled(): bool
    {
        return false;
    }
}
