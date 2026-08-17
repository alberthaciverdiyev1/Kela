@extends('common.layouts.teacher')
@section('title', 'Ödənişlər - Kela')
@section('content')

<div class="space-y-6" x-data="paymentTracker()">
    <x-teacher.heading subtitle="Tələbələrin aylıq ödənişlərini və borclarını izləyin">
        Ödənişlər
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.payments.reminders') }}" variant="outline" icon="bell">
                Bildirişlər
            </x-teacher.button>
            @if($selectedWorkspaceId)
                <button type="button" class="btn btn-primary btn-sm" @click="openGenerate()">
                    <x-icon name="heroicon-o-plus" class="size-4" /> Qaimə Yarat
                </button>
            @endif
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Toolbar: Sinif + Ay filteri --}}
    <form method="GET" action="{{ route('teacher.payments.index') }}" class="flex flex-wrap items-center gap-3">
        <select name="workspace" class="select select-bordered text-sm" onchange="this.form.submit()">
            <option value="">Sinif seç...</option>
            @foreach($workspaces as $ws)
                <option value="{{ $ws['id'] }}" {{ $selectedWorkspaceId == $ws['id'] ? 'selected' : '' }}>
                    {{ $ws['name'] }}
                </option>
            @endforeach
        </select>

        <input
            type="month"
            name="month"
            value="{{ $month }}"
            class="input input-bordered text-sm"
            onchange="this.form.submit()"
        />

        <x-teacher.button type="submit" variant="ghost">Yenilə</x-teacher.button>
    </form>

    {{-- Cədvəl --}}
    @if(!$selectedWorkspaceId)
        <x-teacher.empty-state icon="banknotes" title="Sinif seçin" description="Ödəniş cədvəlini açmaq üçün yuxarıdan bir sinif seçin." />
    @elseif($tracks->isEmpty())
        <x-teacher.empty-state icon="document-text" title="Qaimə yoxdur" description="Bu sinif üzrə cari ay üçün heç bir ödəniş qaiməsi yoxdur. Yuxarıdakı 'Qaimə Yarat' düyməsi ilə əl ilə yarada bilərsiniz." />
    @else
        <x-teacher.table :headers="['Şagird', 'Aylıq Ödəniş (Cəmi)', 'Ödənilən', 'Qalıq Borc', 'Status', 'Növbəti ödəniş', 'Əməliyyat']">
            @foreach($tracks as $track)
                @php
                    $debt = max(0, $track->total_amount - $track->paid_amount);
                    $txns = $track->transactions;
                    // Silinmiş (soft-delete) tələbələr üçün yaddaşda qalmış ad göstərilir.
                    $studentIsTrashed = $track->student?->trashed() ?? false;
                    $studentName = $track->student?->full_name ?? ('Şagird #'.$track->student_id);
                    $studentEmail = $track->student?->email ?? '';
                    $initials = $track->student ? initials($track->student->first_name, $track->student->last_name) : '—';
                    $startDate = $track->start_date ? fmt_date($track->start_date) : null;
                    $nextDue = $track->next_due_date ? fmt_date($track->next_due_date) : null;
                @endphp
                        <tr class="transition {{ $studentIsTrashed ? 'bg-error/10 hover:bg-error/15' : 'hover:bg-base-200/30' }}">
                            <td class="font-medium px-5 py-3">
                                <div class="flex items-center gap-3">
                                    <div class="avatar placeholder">
                                        <div class="bg-primary/10 text-primary rounded-xl w-9 h-9 flex items-center justify-center font-bold">
                                            <span class="text-xs">{{ $initials }}</span>
                                        </div>
                                    </div>
                                    <div>
                                        <div class="flex items-center gap-2 font-semibold text-base-content">
                                            {{ $studentName }}
                                            @if($studentIsTrashed)
                                                <x-teacher.badge color="red">Silinmiş şagird</x-teacher.badge>
                                            @endif
                                        </div>
                                        <div class="text-[11px] text-base-content/50">
                                            {{ $studentEmail }}
                                            @if($startDate)
                                                <span class="text-base-content/40">· Başlama: {{ $startDate }}</span>
                                            @endif
                                        </div>
                                    </div>
                                </div>
                            </td>
                            <td class="px-5 py-3 font-medium text-base-content/80">
                                @if($track->total_amount > 0)
                                    {{ money($track->total_amount) }}
                                @else
                                    <span class="text-error font-semibold text-xs bg-error/10 px-2 py-1 rounded-md">Təyin edilməyib</span>
                                @endif
                            </td>
                            <td class="px-5 py-3 font-medium text-success">{{ money($track->paid_amount) }}</td>
                            <td class="px-5 py-3 font-semibold {{ $debt > 0 ? 'text-error' : 'text-base-content/50' }}">
                                @if($track->total_amount > 0)
                                    {{ money($debt) }}
                                @else
                                    -
                                @endif
                            </td>
                            <td class="px-5 py-3">
                                @if($track->total_amount > 0 && $track->status == \App\Domain\StudentPaymentTrack\StudentPaymentTrack::STATUS_PAID)
                                    <x-teacher.badge color="green">Ödənildi</x-teacher.badge>
                                @elseif($track->status == \App\Domain\StudentPaymentTrack\StudentPaymentTrack::STATUS_PARTIAL)
                                    <x-teacher.badge color="blue">Qismən Ödəniş</x-teacher.badge>
                                @elseif($track->status == \App\Domain\StudentPaymentTrack\StudentPaymentTrack::STATUS_OVERDUE && $track->total_amount > 0)
                                    <x-teacher.badge color="red">Vaxtı keçib</x-teacher.badge>
                                @elseif($track->status == \App\Domain\StudentPaymentTrack\StudentPaymentTrack::STATUS_CANCELLED)
                                    <x-teacher.badge color="gray">Ləğv edilib</x-teacher.badge>
                                @elseif($track->total_amount == 0)
                                    <x-teacher.badge color="yellow">Gözləyir</x-teacher.badge>
                                @else
                                    <x-teacher.badge color="red">Ödənilməyib</x-teacher.badge>
                                @endif
                            </td>
                            <td class="px-5 py-3 whitespace-nowrap text-sm font-medium text-base-content/80">
                                {{ $nextDue }}
                            </td>
                            <td class="px-5 py-3 text-right">
                                <div class="dropdown dropdown-end">
                                    <label tabindex="0" class="btn btn-sm btn-ghost btn-circle">
                                        <x-icon name="heroicon-o-ellipsis-vertical" class="w-5 h-5 text-base-content/60" />
                                    </label>
                                    <ul tabindex="0" class="dropdown-content z-[1] menu p-2 shadow-lg bg-base-100 rounded-box w-48 border border-base-200">
                                        @if($debt > 0 && $track->total_amount > 0)
                                            <li>
                                                <a @click="openModal({{ $track->id }}, '{{ addslashes($studentName) }}', {{ $debt }})" class="text-primary hover:bg-primary/10">
                                                    <x-icon name="heroicon-o-currency-dollar" class="w-4 h-4" /> Ödəniş al
                                                </a>
                                            </li>
                                        @endif
                                        <li>
                                            <a @click="openEditModal({{ $track->id }}, '{{ addslashes($studentName) }}', {{ $track->total_amount }})" class="hover:bg-base-200">
                                                <x-icon name="heroicon-o-pencil-square" class="w-4 h-4" /> Məbləği Təyin Et
                                            </a>
                                        </li>
                                        @if($txns->isNotEmpty())
                                            <li>
                                                <a @click="openTxnModal({{ $track->id }})" class="hover:bg-base-200">
                                                    <x-icon name="heroicon-o-receipt-percent" class="w-4 h-4" /> Tarixçə ({{ $txns->count() }})
                                                </a>
                                            </li>
                                        @endif
                                    </ul>
                                </div>
                            </td>
                        </tr>
            @endforeach
        </x-teacher.table>
    @endif

    {{-- Qaimə Yarat Modalı --}}
    <x-teacher.modal show="isGenerateOpen" title="Qaimə Yarat" maxWidth="md">
        <form method="POST" action="{{ route('teacher.payments.generate') }}">
            @csrf
            <input type="hidden" name="workspace_id" value="{{ $selectedWorkspaceId }}">
            <input type="hidden" name="month" value="{{ $month }}">
            <div class="space-y-4">
                <x-teacher.field label="Şagird" name="student_id" :required="true">
                    <select name="student_id" class="select select-bordered w-full text-sm" required>
                        <option value="">Şagird seç...</option>
                        @foreach($students as $student)
                            <option value="{{ $student['id'] }}">{{ $student['name'] }}</option>
                        @endforeach
                    </select>
                </x-teacher.field>
                <x-teacher.field label="Məbləğ (AZN, boş = sinif/sagird qiyməti)" name="amount">
                    <x-teacher.input name="amount" type="number" step="0.01" min="0" placeholder="Boş buraxıla bilər" />
                </x-teacher.field>
            </div>
            <div class="mt-6 flex justify-end gap-2">
                <x-teacher.button type="button" variant="ghost" @click="isGenerateOpen = false">Ləğv et</x-teacher.button>
                <x-teacher.button type="submit" icon="plus">Qaimə Yarat</x-teacher.button>
            </div>
        </form>
    </x-teacher.modal>

    {{-- Ödəniş Qəbulu Modalı --}}
    <x-teacher.modal show="isModalOpen" title="Ödəniş qəbul et" maxWidth="sm">
        <form id="paymentForm" method="POST" action="{{ route('teacher.payments.store') }}">
            @csrf
            <input type="hidden" name="track_id" x-model="modalTrackId">
            <div class="space-y-4">
                <p class="text-sm font-medium">Şagird: <span class="text-primary" x-text="modalStudentName"></span></p>
                <div class="form-control">
                    <label class="label"><span class="label-text">Ödənilən Məbləğ (AZN)</span></label>
                    <input type="number" step="0.01" name="amount" x-model="modalAmount" class="input input-bordered w-full" required min="0.01" :max="modalMaxDebt" />
                    <label class="label"><span class="label-text-alt text-error">Qalıq borc: <span x-text="modalMaxDebt"></span> AZN</span></label>
                </div>
                <div class="form-control">
                    <label class="label"><span class="label-text">Qeyd (İxtiyari)</span></label>
                    <input type="text" name="note" class="input input-bordered w-full" placeholder="Nəğd, kartla və s." />
                </div>
            </div>

            <div class="mt-6 flex justify-end gap-2">
                <x-teacher.button type="button" variant="ghost" @click="closeModal()">Ləğv et</x-teacher.button>
                <x-teacher.button type="submit" variant="primary">Təsdiqlə</x-teacher.button>
            </div>
        </form>
    </x-teacher.modal>

    {{-- Məbləği Təyin Et Modalı --}}
    <x-teacher.modal show="isEditModalOpen" title="Məbləği təyin et" maxWidth="sm">
        <form id="editForm" method="POST" :action="'{{ url('teacher/payments') }}/' + editModalTrackId">
            @csrf
            @method('PATCH')
            <div class="space-y-4">
                <p class="text-sm font-medium">Şagird: <span class="text-primary" x-text="editModalStudentName"></span></p>
                <div class="form-control">
                    <label class="label"><span class="label-text">Aylıq Ödəniş (Cəmi, AZN)</span></label>
                    <input type="number" step="0.01" name="total_amount" x-model="editModalTotalAmount" class="input input-bordered w-full" required min="0" />
                </div>
            </div>

            <div class="mt-6 flex justify-end gap-2">
                <x-teacher.button type="button" variant="ghost" @click="closeEditModal()">Ləğv et</x-teacher.button>
                <x-teacher.button type="submit" variant="primary">Saxla</x-teacher.button>
            </div>
        </form>
    </x-teacher.modal>

    {{-- Tarixçə Modalı --}}
    <x-teacher.modal show="isTxnOpen" title="Ödəniş tarixçəsi" maxWidth="sm">
        <div class="overflow-x-auto">
            <table class="table w-full text-sm">
                <thead>
                    <tr class="border-b border-base-300 bg-base-200/60">
                        <th class="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-base-content/60">Tarix</th>
                        <th class="px-3 py-2 text-right text-xs font-semibold uppercase tracking-wide text-base-content/60">Məbləğ</th>
                        <th class="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-base-content/60">Qeyd</th>
                    </tr>
                </thead>
                <tbody x-ref="txnBody">
                    {{-- JS tərəfindən doldurulur --}}
                </tbody>
            </table>
        </div>
        <div class="mt-6 flex justify-end">
            <x-teacher.button type="button" variant="ghost" @click="isTxnOpen = false">Bağla</x-teacher.button>
        </div>
    </x-teacher.modal>
</div>

@endsection

@push('scripts')
<script>
    document.addEventListener('alpine:init', () => {
        Alpine.data('paymentTracker', () => ({
            isModalOpen: false,
            modalTrackId: null,
            modalStudentName: '',
            modalMaxDebt: 0,
            modalAmount: 0,

            isEditModalOpen: false,
            editModalTrackId: null,
            editModalStudentName: '',
            editModalTotalAmount: 0,

            isGenerateOpen: false,
            isTxnOpen: false,

            txnByTrack: @json($trackTxns),

            openGenerate() {
                this.isGenerateOpen = true;
            },

            openModal(trackId, studentName, debt) {
                this.modalTrackId = trackId;
                this.modalStudentName = studentName;
                this.modalMaxDebt = debt;
                this.modalAmount = debt;
                this.isModalOpen = true;
            },

            closeModal() {
                this.isModalOpen = false;
                this.modalTrackId = null;
            },

            openEditModal(trackId, studentName, totalAmount) {
                this.editModalTrackId = trackId;
                this.editModalStudentName = studentName;
                this.editModalTotalAmount = totalAmount || 0;
                this.isEditModalOpen = true;
            },

            closeEditModal() {
                this.isEditModalOpen = false;
                this.editModalTrackId = null;
            },

            openTxnModal(trackId) {
                const rows = this.txnByTrack[trackId] || [];
                const body = this.$refs.txnBody;
                body.innerHTML = rows.length
                    ? rows.map(r => `
                        <tr class="border-b border-base-200/60">
                            <td class="px-3 py-2 text-base-content/70">${r.date}</td>
                            <td class="px-3 py-2 text-right font-medium text-success">${r.amount} AZN</td>
                            <td class="px-3 py-2 text-base-content/60">${r.note || '—'}</td>
                        </tr>`).join('')
                    : '<tr><td colspan="3" class="px-3 py-6 text-center text-base-content/50">Ödəniş yoxdur</td></tr>';
                this.isTxnOpen = true;
            },
        }));
    });
</script>
@endpush
