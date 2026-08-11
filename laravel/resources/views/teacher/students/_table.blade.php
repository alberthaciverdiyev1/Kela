    {{-- Şagird cədvəli fragmenti — JS list.refresh() ilə yenidən çəkilir. --}}
@if ($students->isEmpty())
    <x-teacher.empty-state icon="user-group" title="Şagird tapılmadı" description="Axtarışı dəyişin və ya yeni şagird əlavə edin." />
@else
    <x-teacher.table :headers="['Ad Soyad', 'E-poçt', 'Şəhər', 'Doğum', 'Status', '']">
        @foreach ($students as $student)
            @php
                $sJson = [
                    'first_name' => $student['first_name'],
                    'last_name' => $student['last_name'],
                    'email' => $student['email'],
                    'status' => $student['status'],
                    'city_id' => $student['city_id'],
                    'birth_date' => $student['birth_date_iso'],
                ];
            @endphp
            <tr class="transition hover:bg-base-200/50">
                <td class="font-medium text-base-content">{{ $student['full_name'] }}</td>
                <td class="text-base-content/70">{{ $student['email'] }}</td>
                <td class="text-base-content/70">{{ $student['city'] ?? '—' }}</td>
                <td class="text-base-content/70">{{ $student['birth_date'] ?? '—' }}</td>
                <td>
                    <x-teacher.badge :color="match ($student['status']) { 1 => 'green', 2 => 'yellow', 3 => 'red', default => 'gray' }">
                        {{ match ($student['status']) { 1 => 'Aktiv', 2 => 'Deaktiv', 3 => 'Dayandırılmış', default => $student['status'] } }}
                    </x-teacher.badge>
                </td>
                <td class="text-right">
                    <div class="flex items-center justify-end gap-1">
                        <button
                            data-student-edit
                            data-student-id="{{ $student['id'] }}"
                            data-student='@json($sJson, JSON_HEX_APOS | JSON_HEX_QUOT | JSON_HEX_TAG)'
                            title="Redaktə"
                            class="rounded-lg p-1.5 text-base-content/50 hover:bg-amber-50 hover:text-amber-600"
                        >
                            <x-icon name="heroicon-o-pencil-square" class="size-4" />
                        </button>
                        <button
                            data-student-delete
                            data-student-id="{{ $student['id'] }}"
                            data-student-name="{{ $student['full_name'] }}"
                            title="Sil"
                            class="rounded-lg p-1.5 text-error/70 hover:bg-error/10 hover:text-error"
                        >
                            <x-icon name="heroicon-o-trash" class="size-4" />
                        </button>
                    </div>
                </td>
            </tr>
        @endforeach
    </x-teacher.table>
    <x-teacher.pagination :paginator="$students" />
@endif
