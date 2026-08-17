@extends('common.layouts.teacher')
@section('title', 'Ödənişlər - Kela')
@section('content')

<div class="space-y-6" x-data="paymentTracker()">
    <x-teacher.heading subtitle="Tələbələrin aylıq ödənişlərini və borclarını izləyin">
        Ödənişlər
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
        <x-teacher.empty-state icon="document-text" title="Qaimə yoxdur" description="Bu sinif üzrə cari ay üçün heç bir ödəniş qaiməsi yoxdur. Qaimələr hər ayın 1-i avtomatik yaradılır." />
    @else
        <x-teacher.table :headers="['Şagird', 'Aylıq Ödəniş (Cəmi)', 'Ödənilən', 'Qalıq Borc', 'Status', 'Əməliyyat']">
            @foreach($tracks as $track)
                @php
                    $debt = max(0, $track->total_amount - $track->paid_amount);
                @endphp
                        <tr class="hover:bg-base-200/30 transition">
                            <td class="font-medium px-5 py-3">
                                <div class="flex items-center gap-3">
                                    <div class="avatar placeholder">
                                        <div class="bg-primary/10 text-primary rounded-xl w-9 h-9 flex items-center justify-center font-bold">
                                            <span class="text-xs">{{ mb_substr($track->student->first_name, 0, 1) . mb_substr($track->student->last_name, 0, 1) }}</span>
                                        </div>
                                    </div>
                                    <div>
                                        <div class="font-semibold text-base-content">{{ $track->student->full_name }}</div>
                                        <div class="text-[11px] text-base-content/50">{{ $track->student->email }}</div>
                                    </div>
                                </div>
                            </td>
                            <td class="px-5 py-3 font-medium text-base-content/80">
                                @if($track->total_amount > 0)
                                    {{ number_format($track->total_amount, 2) }} AZN
                                @else
                                    <span class="text-error font-semibold text-xs bg-error/10 px-2 py-1 rounded-md">Təyin edilməyib</span>
                                @endif
                            </td>
                            <td class="px-5 py-3 font-medium text-success">{{ number_format($track->paid_amount, 2) }} AZN</td>
                            <td class="px-5 py-3 font-semibold {{ $debt > 0 ? 'text-error' : 'text-base-content/50' }}">
                                @if($track->total_amount > 0)
                                    {{ number_format($debt, 2) }} AZN
                                @else
                                    -
                                @endif
                            </td>
                            <td class="px-5 py-3">
                                @if($track->status == \App\Domain\StudentPaymentTrack\StudentPaymentTrack::STATUS_PAID && $track->total_amount > 0)
                                    <x-teacher.badge color="green">Ödənildi</x-teacher.badge>
                                @elseif($track->status == 1)
                                    <x-teacher.badge color="blue">Qismən Ödəniş</x-teacher.badge>
                                @elseif($track->total_amount == 0)
                                    <x-teacher.badge color="yellow">Gözləyir</x-teacher.badge>
                                @else
                                    <x-teacher.badge color="red">Ödənilməyib</x-teacher.badge>
                                @endif
                            </td>
                            <td class="px-5 py-3 text-right">
                                <div class="dropdown dropdown-end">
                                    <label tabindex="0" class="btn btn-sm btn-ghost btn-circle">
                                        <x-icon name="heroicon-o-ellipsis-vertical" class="w-5 h-5 text-base-content/60" />
                                    </label>
                                    <ul tabindex="0" class="dropdown-content z-[1] menu p-2 shadow-lg bg-base-100 rounded-box w-48 border border-base-200">
                                        @if($debt > 0 && $track->total_amount > 0)
                                            <li>
                                                <a @click="openModal({{ $track->id }}, '{{ addslashes($track->student->full_name) }}', {{ $debt }})" class="text-primary hover:bg-primary/10">
                                                    <x-icon name="heroicon-o-currency-dollar" class="w-4 h-4" /> Ödəniş al
                                                </a>
                                            </li>
                                        @endif
                                        <li>
                                            <a @click="openEditModal({{ $track->id }}, '{{ addslashes($track->student->full_name) }}', {{ $track->total_amount }})" class="hover:bg-base-200">
                                                <x-icon name="heroicon-o-pencil-square" class="w-4 h-4" /> Məbləği Təyin Et
                                            </a>
                                        </li>
                                    </ul>
                                </div>
                            </td>
                        </tr>
            @endforeach
        </x-teacher.table>
    @endif

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
            
            openModal(trackId, studentName, debt) {
                this.modalTrackId = trackId;
                this.modalStudentName = studentName;
                this.modalMaxDebt = debt;
                this.modalAmount = debt; // Default as full remaining amount
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
            }
        }));
    });
</script>
@endpush
