@extends('common.layouts.teacher')
@section('title', 'Ödəniş Bildirişləri - Kela')
@section('content')

<div class="space-y-6">
    <x-teacher.heading
        icon="bell"
        subtitle="Hər saat yoxlanılır — ödəniş müddətinə 5 gün qalmış və ödəniş günü bildiriş göndərilir"
    >
        Ödəniş Bildirişləri
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.payments.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    @if($reminders->isEmpty())
        <x-teacher.empty-state
            icon="bell-slash"
            title="Bildiriş yoxdur"
            description="Ödəniş müddəti yaxınlaşan şagird olduqda burada avtomatik bildirişlər görünəcək."
        />
    @else
        <x-teacher.table :headers="['Şagird', 'Sinif', 'Tip', 'Bildiriş', 'Göndərilib']">
            @foreach($reminders as $reminder)
                @php
                    $track = $reminder->track;
                    $studentName = $track?->student?->full_name ?? ('Şagird #'.$track?->student_id);
                    $workspaceName = $track?->workspace?->name ?? '—';
                @endphp
                <tr class="transition hover:bg-base-200/30">
                    <td class="px-5 py-3 font-medium text-base-content">{{ $studentName }}</td>
                    <td class="px-5 py-3 text-base-content/70">{{ $workspaceName }}</td>
                    <td class="px-5 py-3">
                        @if($reminder->type === \App\Domain\PaymentReminder\Enums\PaymentReminderType::DUE)
                            <x-teacher.badge color="red">Ödəniş günü</x-teacher.badge>
                        @else
                            <x-teacher.badge color="yellow">5 gün qalıb</x-teacher.badge>
                        @endif
                    </td>
                    <td class="px-5 py-3 text-sm text-base-content/80">{{ $reminder->message }}</td>
                    <td class="px-5 py-3 whitespace-nowrap text-sm text-base-content/60">
                        {{ $reminder->sent_at?->format('d.m.Y H:i') }}
                    </td>
                </tr>
            @endforeach
        </x-teacher.table>

        <div class="mt-4">
            {{ $reminders->links() }}
        </div>
    @endif
</div>

@endsection
