# LanguageLMS — Geliştirme Takip Dosyası

> Projede yaptığımız her adım ve sıradaki işler. Kurallar için: `CLAUDE.md`.

## 1. Proje Özeti

LanguageLMS — dil öğrenme (LMS) uygulaması. İki istemci, bir çekirdek:

| Proje | Rol |
|---|---|
| `LanguageLMS.Core` | domain + uygulama + altyapı (entity, EF, servis, DTO, validator, storage) |
| `LanguageLMS.Api` | REST API (mobil/web istemcileri) |
| `LanguageLMS.Mvc` | Admin / web paneli (Tailwind CSS) |

**Mantık kaynağı:** Laravel `BPA-Backend` (`/home/albert/Downloads/BPA-Backend-main/`). Yalnızca iş mantığı taşınır.

## 2. Teknoloji Yığını

| Katman | Seçim |
|---|---|
| Framework | .NET 10 |
| Solution | `LanguageLMS.slnx` |
| ORM | Entity Framework Core 10 + Npgsql (PostgreSQL) |
| Doğrulama | FluentValidation 12 (Core'da; API'de global filtre, MVC'de servis içi) |
| API dokümanı | Microsoft.AspNetCore.OpenApi |
| MVC UI | Razor + Tailwind CSS v4 (`@tailwindcss/cli`) |
| Cache | IMemoryCache |

## 3. Mimari

Pragmatik **modular monolith** (dikey dilim). Katmanlar Onion/Clean gibi ayrılmadı; altyapı (EF) Core içinde.
Proje tutarsa ileride Domain/Application/Infrastructure olarak ayrılabilir.

```
LanguageLMS.Core/
├── Common/{Entities,Responses,Storage}
├── Data/            (AppDbContext + Migrations)
├── Modules/<Feature>/{Entities,Configurations,Services,Requests,Responses,Validators}
└── DependencyInjection.cs   (AddCore + UseCoreStorage)
LanguageLMS.Api/     (Program.cs, Common/{Controllers,Middleware,Validation,Responses}, Modules/<F>/Controllers)
LanguageLMS.Mvc/     (Program.cs, Controllers, Models, Views)
```

- API yanıtları **tek envelope**: `{ success, statusCode, message, data, errors }` (`Common/Responses/ApiResponse`).
- Storage paylaşımlı: `Storage:RootPath` (`.gitignore`'da `uploads/`). Şimdilik yerel; ileride BunnyCDN (`IFileStorage`).

### Laravel → .NET eşleştirmesi
| Laravel | .NET |
|---|---|
| `app/Models/*` | `Core/Modules/<F>/Entities/*` |
| `database/migrations/*` | EF Core Migration (`Core/Data/Migrations`) |
| `app/Http/Controllers/**` | `Api` veya `Mvc` controller |
| `app/Http/Requests/**` | `Core/Modules/<F>/Requests` (+`Validators`) |
| `app/Http/Resources/**` | `Core/Modules/<F>/Responses` |
| `app/Services/**` | `Core/Modules/<F>/Services` |
| `responseHelper()` | `Common/Responses/ApiResponse` |
| `SoftDeletes` | `BaseEntity.DeletedAt` + EF global query filter |

## 4. İlerleme

- ✅ Adım 1 — Solution (`slnx`) + Api & Mvc projeleri.
- ✅ Adım 2 — API iskeleti (FluentValidation, OpenAPI).
- ✅ Adım 3 — ORM: EF Core + Npgsql, `AppDbContext`.
- ✅ Adım 4 — MVC Tailwind kurulumu, Bootstrap kaldırıldı.
- ✅ Adım 5 — Referans analizi + `CLAUDE.md` kuralları.
- ✅ Adım 6 — Modüler yapı (`Modules/`).
- ✅ Adım 7 — **Banner** modülü: entity/config/soft delete/migration, requests+validator, responses, service (cache + CRUD), API controller.
- ✅ Adım 8 — Birleşik API envelope (tüm yanıtlar tek format).
- ✅ Adım 9 — Modül içi düzen + tek controller.
- ✅ Adım 10 — **`LanguageLMS.Core`** ayrıldı; Api & Mvc ince host oldu; paylaşımlı storage.
- ✅ Adım 11 — **MVC Banner CRUD** (index/create/edit/update/delete/details), `IBannerService` direkt inject.
- ✅ Adım 12 — Migration uygulandı, uçtan uca doğrulandı (API CRUD + MVC CRUD, gerçek DB).
- ✅ Adım 13 — **Profession** modülü (2 seviyeli hiyerarşi, benzersiz ad, çocuklu kayıt silinemez) + testler.
- ✅ Adım 14 — Banner uçları sadeleştirildi: `admin` prefix kaldırıldı, tek `list`; endpointler: list/show/add/update/delete.
- ✅ Adım 15 — **Setting** modülü (tek satırlık ayar + `/api/app-version`) + testler.
- ✅ Adım 16 — **Auth** modülü: JWT ile register/login/me/logout (OTP ve roller **yok** — sonra eklenecek) + testler.
- ✅ Adım 17 — **MainCategory (fenn)** modülü (unique ad, sahip `user_id`) + testler. (İçerik zənciri: fenn→qrup/ixtisas)
- ✅ Adım 18 — Routing konvansiyonu (class-level `api/[controller]` + kebab-case) + **User (teacher/admin)** modülü + testler.
- ✅ Adım 19 — **Group (qrup)** modülü: sahip (`user_id`→User FK), görsel, **student_groups** üzvülüyü (start/end date), öğrenci atama/listeleme/çıkarma + testler.
- ✅ Adım 20 — **Profession (ixtisas) silindi** (ücretsiz plan yok). `MainCategory` → **`Subject`** (fenn) olarak adlandırıldı; tablo `main_categories` → `subjects` (rename migration). Student'tan profession alanları çıkarıldı. Anlamlı isimlendirme kuralı benimsendi (ileride `Topic`→`Lesson`, `TopicContent`→`LessonContent`).
- ✅ Adım 21 — **Subject ↔ Group bağı** (`group_subjects` pivot): qrupa fenn bağlama/listeleme/çıkarma (`/api/group/{id}/subjects`) + testler. (Course yok; tenant sonra eklenecek.)
- ✅ Adım 22 — **Lesson (dərs)** modülü: `subject_id` (FK), title, image, sort_order, is_active; filtreli listeleme + CRUD + testler.
- ✅ Adım 23 — **BunnyCDN entegrasyonu** (`BunnyCdnStorage`, `Storage:Provider = Local|BunnyCdn`) + **medya modeli** (`Video`, `MediaFile`, `VoiceRecord`) + **LessonContent** (lesson_id + media refs, tip: video/file/voice) + CRUD + testler.
- ✅ Adım 24 — **User (müəllim/admin) auth**: JWT `actor` claim + authorization policy'leri (`Student` / `User`). `/api/user-auth/{login,me,logout}`. Student və User token'ları bir-birinin uçlarına gire bilmir (403). + testler.
- ✅ Adım 25 — **Şagird app axını** (`StudentAppService`): `groups → subjects → lessons → contents` (aktiv filtrləri + üzvlük yoxlaması, 403). `ForbiddenException` + middleware + testler.
- ✅ Adım 26 — **Progress / Statistics**: `LessonProgress` (içerik tamamlama), `POST /api/progress` (mark), `GET /api/progress` (dərs üzrə faiz), `GET /api/statistics` (ümumi + fenn üzrə). Paylaşılan `StudentAccessService`. + testler.
- ✅ Adım 27 — **Question bank** (fenn üzrə suallar): `Question` (title, variant A-E, correct_answer, variant_type), CRUD + testler. (Quiz bölməsi sırada.)
- ✅ Adım 28 — **Quiz**: `Quiz` (lesson_id, title, duration, type, is_active, is_repeat, sort_order) + `QuizQuestion` pivot. CRUD + sual bağlama/listeleme/çıxarma + questionCount. + testler.
- ✅ Adım 29 — **Şagird imtahanı**: `StudentQuiz` (attempt) + `StudentQuizQuestion` (cavablar). `POST /api/student-quiz/{start,complete}`, `GET .../results[/{id}]`. Bal hesabı, doğru cavab sızması yoxdur, üzvlük yoxlaması. + testler.
- ✅ Adım 30 — **Group general quizzes**: `GroupQuiz` pivot (general quizlər üçün `Quiz.LessonId` opsiyonel). `POST/GET/DELETE /api/group/{id}/quizzes` + şagird `GET /api/student/quizzes`. + testler.
- ✅ Adım 31 — **Notification**: `Notification` (+`notification_students` pivot) + `NotificationToken` (cihaz). Müəllim: `/api/notification` (list/create/delete). Şagird: `GET /api/student/notifications`, `POST/DELETE .../token`. Push: `IPushSender` (hazırda `NoOp`, FCM açarları gözləyir). + testler.
- ✅ Adım 32 — **Roles**: JWT `role` claim (`student`/`teacher`/`admin`) + policy'ler (`Student` / `Teacher`=teacher+admin / `Admin`). İçerik uçları → **Teacher**, istifadəçi idarəsi → **Admin**, şagird uçları → **Student**. Boşluqlar bağlandı (əvvəl hamısı açıq idi). + testler.
- ✅ Adım 33 — **PrivacyPolicy**: `privacy_policies` (type + HTML content). `GET /api/privacy-policy[/{type}]` (public), `PUT /api/privacy-policy` (Teacher, upsert). + testler.
- ✅ Adım 34 — **Group quiz statistikası** (`GroupQuizStatService`): `GET /api/group/{id}/quiz-stat?quiz_type=lesson|general` → qrupun quizləri üzrə hər şagirdin nəticəsi (attempted, correct, wrong, score %). Reference `GroupQuizStatController` məntiqi. + testler.
- ✅ Adım 48 — **Kiçik admin/API parçaları (III)**: **şagird generasiyası** (`POST /api/students/generate`), **admin device idarəsi** (`GET/DELETE /api/students/{id}/devices` — device lock sıfırlama), **Excel (.xlsx) export** (students + group quiz-stat, ClosedXML). + testlər.
- ✅ Adım 47 — **Kiçik admin/API parçaları (II)**: **in-app (system) notifications** (`system_notifications`, oxundu/oxunmadı — `/api/system-notification`) + **assignment audit log** (`main_category_assignment_logs`, fenn atama/çıxarma qeyd olunur — `GET /api/group/{id}/subject-logs`). + testlər.
- ✅ Adım 46 — **Kiçik admin/API parçaları (I)**: **sort/reorder** (`POST /api/{lesson,quiz,lesson-content}/sort`), **editor image upload** (`/api/editor/upload`), **clear-cache** (`/api/cache/clear`), **lesson settings endpoint** (`GET/PUT /api/lesson/{id}/settings`). + testlər.
- ✅ Adım 45 — **Admin funksionallığı (API)**: `GET /api/dashboard` (statistika), **şagird idarəsi** (`/api/students` list/show/status/delete/restore + bulk), **bulk əməliyyatlar** (`POST /api/{banner,subject,lesson,lesson-content,group,question,quiz}/bulk` → delete/restore/activate/deactivate), **CSV export** (students + group quiz-stat). + testlər.
- ✅ Adım 44 — **Presigned (birbaşa) upload**: `IFileStorage.CreatePresignedPutUrl` + `ExistsAsync`; `POST /api/lesson-content/upload-url` → `{ uploadUrl, path }`; client birbaşa R2-yə PUT edir; sonra `path` ilə content yaradılır. `Video.playback_url` (HLS/m3u8 üçün). + testlər.
- ✅ Adım 43 — **Cloudflare R2 storage** (`R2FileStorage`, `AWSSDK.S3`, S3-uyğun): `Storage:Provider = R2`; `R2` konfiqurasiyası (AccountId/AccessKey/Bucket/PublicBaseUrl). Bunny Stream **yoxdur**; videolar R2-yə. (m3u8/HLS konvertor sonra.)
- ✅ Adım 42 — **Refaktor**: servis interfeysləri `Modules/*/Abstractions/`-ə köçürüldü (Services-də yalnız tətbiqlər); Core+Api-də `GlobalUsings.cs`. + switch-account.
- ✅ Adım 41 — **İncə icazələr (permissions)**: `permissions` + `role_permissions`; `HasPermission("x")` filter (admin bypass; rol üçün icazə təyin olunmayıbsa permissive); `/api/permission` (Admin: list/create/set-role). İçerik controller'larına tətbiq olundu. + testlər.
- ✅ Adım 40 — **Quiz repeats**: `StudentQuiz.attempt_time` + `is_repeat` tətbiqi (təkrar quizlər çoxlu cəhd, digərləri yalnız 1). + testlər.
- ✅ Adım 39 — **Sual şəkilləri**: `Question.title_type`/`variant_type` (text|image), şəkil yükləmə (`title_image`, `variant_x_image`), cavabda `titleUrl`/`variantXUrl`. + test.
- ✅ Adım 38 — **Device lock** (`student_devices`): ilk girişdə cihaz bağlanır; fərqli `device_id` ilə giriş → 403. `LoginRequest.DeviceId`. + testler.
- ✅ Adım 37 — **Məzmun açarları + üzvülük vaxt pəncərəsi**: Lesson `settings` JSON (topic_explain/live_video/pdf/voice_record/topic_quiz/general_quiz bayraqları), `GET /api/student/lessons/{id}/content-count`, və şagirdin **tarixli üzvülük pəncərələri** ilə dərs/material filtri. + testler.
- ✅ Adım 36 — **Şagird hesab idarəsi + OTP (e-poçt)**: `send-otp`/`check-otp`/`reset-password`, `change-password`/`change-email`/`change-phone`/`change-full-name`/`notification-status`/`delete-account`. `OtpCode` + `SmtpEmailSender` (SMTP konfiqurasiyası gəlincə işləyir). `check-otp` kodu istehlak etmir. + testler.
- ✅ Adım 35 — **Tenant (multi-tenancy)**: `tenants` cədvəli; `BaseEntity`'yə + Setting/PrivacyPolicy'ə `tenant_id`. `ITenantProvider` (header `X-Tenant-Id` > JWT `tenant_id` claim). Global query filter (soft-delete + tenant). JWT-yə `tenant_id` claim. `POST/GET /api/tenant` (bootstrap). + izolyasiya testleri.

## 5. Durum / Notlar

- 🗄️ DB: `renewLms` (PostgreSQL). `banners` tablosu oluşturuldu (`dotnet ef database update`).
  Migration komutu: `dotnet ef migrations add <Ad> --project LanguageLMS.Core --startup-project LanguageLMS.Api`
- 🔐 **Auth yok** — Banner uç/sayfaları şimdilik herkese açık; roller sonra bağlanacak.
- 🖼️ Görsel sıkıştırma yok (referanstaki Intervention Image yerine düz yükleme); BunnyCDN ileride.
- ⚠️ Ortamda `Microsoft.AspNetCore.App.Ref` pack'i eksik → kök `Directory.Build.props` içinde
  `AllowMissingPrunePackageData=true` workaround'u var.

## 6. Yol Haritası

- [x] Banner
- [x] Profession
- [~] Auth — Student (register/login) + User (login) hazır; OTP, roller, student.active gözləyir
- [x] Şagird app axını — groups → subjects → lessons → contents (üzvlük yoxlaması ilə)
- [x] Progress / Statistics — içerik tamamlama + statistika
- [x] Setting
- [x] İçerik zənciri — Subject · Group · group_subjects · Lesson · LessonContent · medya (BunnyCDN). **Profession iptal.**
- [ ] Video / VoiceRecord / File (BunnyCDN)
- [x] Question / Quiz / StudentQuiz — sual bankı, quiz, tələbə imtahanı (bal + nəticələr)
- [ ] StudentQuiz / QuizRepeat
- [ ] Progress
- [ ] Statistics
- [x] PrivacyPolicy
- [ ] Admin (dashboard, sort order)

---
_En son güncelleme: Adım 48 — Student generate + device idarəsi + Excel export._
