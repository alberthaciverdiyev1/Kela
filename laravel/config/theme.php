<?php

return [

    /*
    |--------------------------------------------------------------------------
    | Tema tənzimləmələri
    |--------------------------------------------------------------------------
    |
    | Bu fayldakı dəyərlər teacher panelin görünüşünü idarə edir.
    | Hazırda Kütüphane / Dərslər / Quizlər file-manager səhifələri
    | buradan oxuyur. Dəyişikliklər üçün: config/theme.php
    |
    */

    /*
    |--------------------------------------------------------------------------
    | Tam genişlik / container
    |--------------------------------------------------------------------------
    |
    | true  → container kısıtı (max-w-7xl) qaldırılır, səhifə tam genişlik olur.
    | false → standart mərkəzi container görünüşü qalır.
    |
    */
    'full_bleed' => true,

    /*
    |--------------------------------------------------------------------------
    | Yan boşluq (px)
    |--------------------------------------------------------------------------
    |
    | full_bleed=true olduqda səhifənin sol və sağ kənarlarından boşluq.
    |
    */
    'side_padding' => 40,

    /*
    |--------------------------------------------------------------------------
    | Sidebar genişlikləri (Tailwind sinif adları)
    |--------------------------------------------------------------------------
    */
    'sidebar_width' => 'w-56', // sol qovluq ağacı
    'preview_width' => 'w-64', // sağ önizleme paneli

    /*
    |--------------------------------------------------------------------------
    | Grid görünümü — responsive sütun sayıları
    |--------------------------------------------------------------------------
    */
    'grid_columns' => 'grid-cols-2 sm:grid-cols-3 md:grid-cols-4 xl:grid-cols-5',

    /*
    |--------------------------------------------------------------------------
    | Varsayılan görünüm
    |--------------------------------------------------------------------------
    | grid | list
    */
    'view_mode' => 'grid',

    /*
    |--------------------------------------------------------------------------
    | Varsayılan sıralama
    |--------------------------------------------------------------------------
    */
    'sort' => [
        'field' => 'name',    // name | type
        'direction' => 'asc', // asc | desc
    ],

];
