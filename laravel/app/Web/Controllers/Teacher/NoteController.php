<?php

namespace App\Web\Controllers\Teacher;

use Illuminate\View\View;

/**
 * Qeydlər səhifəsi (teacher paneli) — Google Keep üslubu.
 * Məlumat JS vasitəsilə /api/v1/notes endpointlərindən gəlir.
 */
class NoteController
{
    public function index(): View
    {
        return view('teacher.notes.index');
    }
}
