# Edutor — Geliştirme Kuralları

Bu dosya projede uyulacak kuralları tanımlar. Yeni kural eklendikçe buraya yazılır.

## Kod Kuralları

- **Gereksiz yorum yazma.** `/// <summary> ... </summary>` gibi açıklama bloklarını yalnızca
  gerçekten faydalı olduğu yerde kullan. Kaynağı tekrarlayan yorumlar **yazılmaz**.
- Yorum yalnızca **neden** açıklaması gerektiğinde (kafa karıştıran iş mantığı, workaround vb.) kısa yazılır.
- Kod ve tanımlayıcılar **İngilizce**; kullanıcıya dönen mesajlar mevcut dilde (Azerice/Türkçe) korunur.

## Mimari Kurallar

Repo dört bağımsız uygulama grubuna ayrılmıştır; her grup kendi solution'ı ile açılır/build edilir:

| Qovluq | Solution | Projeler | Rol |
|---|---|---|---|
| `api/` | `Edutor.Api.slnx` | `Edutor.Api` | JSON API (controller, `ApiResponse` zarfı, middleware) |
| `admin/` | `Edutor.Admin.slnx` | `Edutor.Admin` | Razor MVC + Tailwind (admin paneli) |
| `web/` | `Edutor.Web.slnx` | `Edutor.App` (MAUI), `Edutor.App.Shared`, `Edutor.App.Web` (WASM) | istemci |
| `worker/` | `Edutor.Worker.slnx` | `Edutor.Nodes.App` (Avalonia), `Edutor.Nodes.Worker` | video encoding worker |
| `shared/` | — | `Edutor.Core`, `Edutor.Nodes.Shared` | paylaşılan domain/uygulama və kontraktlar |
| `tests/` | — | `Edutor.Tests` | inteqrasiya testləri |

- Kök `Edutor.slnx` hamısını birlikdə yükləyir; gündəlik işdə sahə solution'ı istifadə olunur.
- `shared/*` heç bir host-a aid deyil; `api`, `admin`, `worker` ona referans verir.

- **İş mantığı yalnızca Core'da yaşar.** Api ve Admin ince host'lardır; Core'u referans alır.
  Aynı mantık iki yerde **yazılmaz** (hem app hem admin için ayrı sorgu/mantık kopyalanmaz).
- MVC, Core servislerini **DI ile direkt inject eder** (server-side render). Yalnızca gerçekten
  dinamik yerlerde (satır içi arama, anlık liste vb.) JS→API kullanılabilir.
- Api ve Admin, servisleri `AddCore(builder.Configuration)` ile tek noktadan kaydeder.
- Storage paylaşımlıdır (`Storage:RootPath`); her iki host da `/uploads` yolundan servis eder.

### Core modül yerleşimi
Her dosya ilgili alt klasöründe; boşta dosya bırakılmaz.
```
shared/Edutor.Core/Modules/<Feature>/
├── Entities/<Feature>.cs
├── Configurations/<Feature>Configuration.cs   (IEntityTypeConfiguration)
├── Abstractions/  (<Feature>Service interfeysləri)
├── Services/      (<Feature>Service tətbiqi)
├── Requests/    (DTO)
├── Responses/   (DTO)
└── Validators/  (FluentValidation)
```

### Interface qaydası
Servis interfeysləri (`I<Feature>Service`) **`Abstractions/`** folderində, tətbiqlər `Services/`-də.
Core və Api-də `GlobalUsings.cs` ilə bütün abstraksiya namespace-ləri qlobal `using` edilir.

### Controller yerleşimi (sunum)
- Api: `api/Edutor.Api/Modules/<Feature>/Controllers/<Feature>Controller.cs`
- Admin: `admin/Edutor.Admin/Controllers/<Feature>Controller.cs`
- **Tek controller kuralı:** public + admin uçları **aynı controller'da** toplanır; `admin` route prefix'i yoktur.
- **Routing konvansiyonu:** controller sınıfına `[Route("api/[controller]")]`; action'lara yalnızca fiil yazılır:
  `[HttpGet]`, `[HttpGet("{id:long}")]`, `[HttpPost]`, `[HttpPut("{id:long}")]`, `[HttpDelete("{id:long}")]`.
  **Baştaki `/` kullanılmaz** (aksi halde route absolute olur ve bozulur).
- Route token'ları kebab-case'e çevrilir (`KebabCaseParameterTransformer` + `RouteTokenTransformerConvention`)
  → `MainCategory` → `api/main-category`.

### Form binding
Multipart/form alanları snake_case (`variant_a`, `subject_id`) gönderilir. C# özellikleri PascalCase olduğundan
eşleşme olmaz → her böyle alan için `[FromForm(Name = "variant_a")]` kullanılır. (Basit tek kelime alanlar
(`title`) otomatik eşleşir.)

### Bilinen tuzak
Entity adı modül adıyla aynı olduğunda (örn. `Banner`), alt namespace'lerde tür adı namespace'e çözülür.
Bu yüzden entity'yi kullanan dosyalarda tür alias'ı **namespace gövdesinde** tanımlanır:
```csharp
namespace Edutor.Core.Modules.Banner.Services;
using Banner = Edutor.Core.Modules.Banner.Entities.Banner;
```

## Auth / Rollar
- JWT-də `role` claim: `student` · `teacher` · `admin`.
- Policy-lər: `AuthPolicies.Student` · `AuthPolicies.Teacher` (teacher+admin) · `AuthPolicies.Admin`.
- `RequireClaim("role", ...)` **işləmir** (JWT "role"-u ClaimTypes.Role-a map edir) → **`RequireRole(...)`** istifadə olunur.
- İçerik idarəsi `[Authorize(Policy = AuthPolicies.Teacher)]`, istifadəçi idarəsi `Admin`, şagird uçları `Student`.

## Multi-tenancy
- `BaseEntity`-də `tenant_id` var (Setting/PrivacyPolicy də tenant-scoped).
- Resolution: `X-Tenant-Id` header (admin tenant dəyişməsi/bootstrap) > JWT `tenant_id` claim.
- Global query filter: `CurrentTenantId == null || TenantId == CurrentTenantId`. Tenant bilinməyəndə **filter tətbiq olunmur** (dev/test rahatlığı). SaveChanges `tenant_id`-i avtomatik qoyur.
- ⚠️ Filter ifadəsində nullable'i `(long)`-a çevirmə — `Convert(TenantId, long?)` istifadə et (nullable cast exception).

## Referans Proje
- Mantık kaynağı: Laravel tabanlı `/home/albert/Downloads/BPA-Backend-main/`.
- **Yapı değil, yalnızca iş mantığı** taşınır; .NET idiomlarına uyarlanır.
- Eşleştirme tablosu `PROJECT.md` içinde.
- Production deployment: `DEPLOY.md` (Docker yoxdur; systemd + nginx).
