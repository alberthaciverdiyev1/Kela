@extends(auth()->user()->isTeacher() || auth()->user()->isAdmin() ? 'common.layouts.teacher' : 'common.layouts.app')

@section('title', 'Profil Ayarları - Kela')

@section('content')
<div class="space-y-6">
    <x-teacher.heading>
        Profil Ayarları
        <x-slot:subtitle>Şəxsi məlumatlarınızı və şifrənizi buradan yeniləyə bilərsiniz.</x-slot:subtitle>
    </x-teacher.heading>

    @if($errors->any())
        <div class="mb-6 rounded-xl bg-error/10 p-4 text-sm text-error">
            <div class="flex items-center gap-2 font-medium">
                <x-icon name="heroicon-o-exclamation-triangle" class="size-5 shrink-0" />
                <span>Xəta baş verdi</span>
            </div>
            <ul class="ml-7 mt-1 list-disc text-error/80 space-y-1">
                @foreach ($errors->all() as $error)
                    <li>{{ $error }}</li>
                @endforeach
            </ul>
        </div>
    @endif

    <x-teacher.card>
        <form method="POST" action="{{ route('profile.update') }}" enctype="multipart/form-data">
            @csrf
            @method('PUT')

            <div class="grid grid-cols-1 gap-8 lg:grid-cols-3">
                {{-- Sol sütun: Məlumatlar --}}
                <div class="space-y-6 lg:col-span-2">
                    <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
                        <div>
                            <label class="mb-1.5 block text-sm font-medium text-base-content/70" for="first_name">Ad</label>
                            <input id="first_name" type="text" name="first_name"
                                   class="input input-bordered w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                                   value="{{ old('first_name', $user->first_name) }}" required autocomplete="given-name">
                        </div>
                        <div>
                            <label class="mb-1.5 block text-sm font-medium text-base-content/70" for="last_name">Soyad</label>
                            <input id="last_name" type="text" name="last_name"
                                   class="input input-bordered w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                                   value="{{ old('last_name', $user->last_name) }}" autocomplete="family-name">
                        </div>
                    </div>

                    <div>
                        <label class="mb-1.5 block text-sm font-medium text-base-content/70" for="email">E-poçt ünvanı</label>
                        <input id="email" type="email" name="email"
                               class="input input-bordered w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                               value="{{ old('email', $user->email) }}" required autocomplete="email">
                    </div>

                    <hr class="my-6 border-base-200" />

                    <div>
                        <h3 class="text-lg font-bold tracking-tight text-base-content">Şifrəni Yenilə</h3>
                        <p class="mb-4 mt-1 text-sm text-base-content/50">Şifrənizi dəyişdirmək istəmirsinizsə, bu xanaları boş saxlayın.</p>
                        <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
                            <div>
                                <label class="mb-1.5 block text-sm font-medium text-base-content/70" for="password">Yeni Şifrə</label>
                                <input id="password" type="password" name="password"
                                       class="input input-bordered w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                                       autocomplete="new-password">
                            </div>
                            <div>
                                <label class="mb-1.5 block text-sm font-medium text-base-content/70" for="password_confirmation">Yeni Şifrə (təkrar)</label>
                                <input id="password_confirmation" type="password" name="password_confirmation"
                                       class="input input-bordered w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                                       autocomplete="new-password">
                            </div>
                        </div>
                    </div>
                </div>

                {{-- Sağ sütun: Avatar --}}
                <div class="lg:col-span-1">
                    <div class="rounded-2xl border border-base-200 bg-base-100/50 p-5">
                        <h3 class="mb-4 text-sm font-semibold tracking-tight text-base-content">Profil Şəkli (Avatar)</h3>
                        <div class="flex flex-col items-center justify-center gap-4 rounded-xl border-2 border-dashed border-base-300 bg-base-200/50 p-6 text-center transition hover:border-primary/50">
                            <div class="relative flex size-24 shrink-0 items-center justify-center overflow-hidden rounded-full bg-primary text-3xl font-bold text-white ring-4 ring-base-100 shadow-md">
                                @if($user->avatar_url)
                                    <img src="{{ $user->avatar_url }}" alt="{{ $user->full_name }}" class="h-full w-full object-cover" id="avatar-preview" />
                                @else
                                    <span id="avatar-initials">{{ strtoupper(mb_substr($user->first_name, 0, 1).mb_substr($user->last_name, 0, 1)) }}</span>
                                    <img src="" class="hidden h-full w-full object-cover" id="avatar-preview" />
                                @endif
                            </div>
                            <div>
                                <label for="avatar" class="cursor-pointer inline-block rounded-lg bg-base-100 px-4 py-2 text-sm font-medium text-primary shadow-sm ring-1 ring-base-300 transition hover:bg-base-200 active:scale-95">
                                    Şəkil Seçin
                                </label>
                                <input id="avatar" type="file" name="avatar" class="hidden" accept="image/*"
                                    onchange="
                                        const file = this.files[0];
                                        if (file) {
                                            const reader = new FileReader();
                                            reader.onload = function(e) {
                                                document.getElementById('avatar-preview').src = e.target.result;
                                                document.getElementById('avatar-preview').classList.remove('hidden');
                                                const initials = document.getElementById('avatar-initials');
                                                if(initials) initials.classList.add('hidden');
                                            }
                                            reader.readAsDataURL(file);
                                        }
                                    ">
                                <p class="mt-3 text-xs leading-relaxed text-base-content/50">Tövsiyə edilən ölçü: 500x500px.<br>Maksimum 5MB (JPG, PNG)</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="mt-8 flex justify-end border-t border-base-200 pt-6">
                <x-teacher.button type="submit" variant="primary" icon="check">
                    Yadda saxla
                </x-teacher.button>
            </div>
        </form>
    </x-teacher.card>
</div>
@endsection
