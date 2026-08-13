<?php

namespace App\Web\Controllers\Student;

use Illuminate\View\View;

/**
 * Qeydlər səhifəsi (şagird paneli) — Google Keep üslubu.
 */
class NoteController
{
    public function index(): View
    {
        return view('student.notes.index');
    }
}
