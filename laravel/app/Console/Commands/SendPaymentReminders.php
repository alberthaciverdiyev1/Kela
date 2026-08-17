<?php

namespace App\Console\Commands;

use App\Application\PaymentReminder\PaymentReminderService;
use Illuminate\Console\Command;

class SendPaymentReminders extends Command
{
    protected $signature = 'payments:remind';

    protected $description = 'Ödəniş müddətinə 5 gün qalmış və ödəniş günü bildirişlər yaradır';

    public function handle(PaymentReminderService $reminders): int
    {
        $sent = $reminders->run();
        $this->info("{$sent} yeni ödəniş bildirişi yaradıldı.");

        return self::SUCCESS;
    }
}
