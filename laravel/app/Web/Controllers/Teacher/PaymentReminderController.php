<?php

namespace App\Web\Controllers\Teacher;

use App\Application\PaymentReminder\PaymentReminderService;
use Illuminate\Routing\Controller;
use Illuminate\View\View;

class PaymentReminderController extends Controller
{
    public function __construct(private readonly PaymentReminderService $reminders)
    {
    }

    public function index(): View
    {
        $reminders = $this->reminders->listForTeacher((int) auth()->id());

        return view('teacher.payments.reminders', ['reminders' => $reminders]);
    }
}
