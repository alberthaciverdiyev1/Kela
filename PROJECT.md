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
- [ ] Auth / Student (OTP, token, `student.active`, roller)
- [ ] Setting
- [ ] Group / MainCategory / Topic / TopicContent
- [ ] Video / VoiceRecord / File (BunnyCDN)
- [ ] Question / Quiz / QuizQuestion
- [ ] StudentQuiz / QuizRepeat
- [ ] Progress
- [ ] Statistics
- [ ] Notification
- [ ] PrivacyPolicy
- [ ] Admin (dashboard, sort order)

---
_En son güncelleme: Adım 14 — Profession modülü + Endpoint sadeleştirme._
