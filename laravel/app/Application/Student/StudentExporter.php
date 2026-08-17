<?php

namespace App\Application\Student;

use Symfony\Component\HttpFoundation\StreamedResponse;

/**
 * Yaradılan şagird məlumatlarını Excel-uyumlu CSV olaraq endirir.
 * UTF-8 BOM əlavə olunur ki, Excel Azərbaycan hərflərini düzgün açsın.
 */
class StudentExporter
{
    /**
     * @param array<int, array{first_name: string, last_name: string, email: string, password: string}> $rows
     */
    public static function csv(array $rows): StreamedResponse
    {
        return response()->streamDownload(function () use ($rows) {
            $out = fopen('php://output', 'w');
            fwrite($out, "\xEF\xBB\xBF");
            fputcsv($out, ['Ad', 'Soyad', 'E-poçt', 'Şifrə']);
            foreach ($rows as $r) {
                fputcsv($out, [
                    $r['first_name'],
                    $r['last_name'] ?? '',
                    $r['email'],
                    $r['password'],
                ]);
            }
            fclose($out);
        }, 'sagirdler-'.now()->format('Y-m-d-His').'.csv', [
            'Content-Type' => 'text/csv; charset=UTF-8',
        ]);
    }
}
