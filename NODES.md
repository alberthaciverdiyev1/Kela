# Edutor Nodes — paylanmış video konversiyası

Yüklənən videoları HLS-ə çevirən, işi şəbəkədəki cihazlara paylayan sistem.
Koordinator **mövcud Edutor API-nin içindədir** — ayrı backend yoxdur.

```
Edutor.Api ──► Nodes modulu (koordinator)          Edutor.Nodes.App (Avalonia UI)
   │            planlayıcı · lease · doğrulama        │
   │            reconciliation · master playlist        │
   │                                                   ▼
   └──────── Edutor.Nodes.Worker (ffmpeg) ◄────── cihaz worker-i
                     │
                     ▼
                 Cloudflare R2
```

| Layihə | Rol |
|---|---|
| `Edutor.Core/Modules/Nodes` | koordinator: entity, scheduler, job lifecycle |
| `Edutor.Nodes.Shared` | worker ↔ API arasında ortaq DTO-lar (yalnız BCL) |
| `Edutor.Nodes.Worker` | encoding motoru: ffmpeg, probe, encoder tespiti, benchmark |
| `Edutor.Nodes.App` | Avalonia UI (masaüstü + Android başlığı) |
| `Edutor.Admin` (`/node`) | admin: cihazlar, qeydiyyat kodları, tapşırıqlar |

## Necə işləyir

1. Admin videonu R2-yə yükləyir, `POST /lesson-content` ilə qeyd yaradır.
   `LessonContentService` dərhal `EnsureJobsAsync` çağırır.
2. Hər keyfiyyət pilləsi (360/480/720/1080) üçün **ayrı tapşırıq** yaranır —
   scheduler onları müxtəlif cihazlara paylaya bilsin deyə.
3. Cihaz `POST /api/nodes/jobs/claim` ilə tapşırıq götürür. Server
   `FOR UPDATE SKIP LOCKED` ilə kilidləyir, **lease** və **fencing token**
   (`attemptToken`) verir.
4. Cihaz mənbəni endirir, ffprobe ilə analiz edir, mənbə ölçülərini bildirir.
   Server mənbədən böyük pillələri **ləğv edir** (upscale olmasın).
5. Cihaz ffmpeg ilə HLS yaradır, seqmentləri (playlist-dən əvvəl!) yükləyir.
6. `POST .../complete` — server variant playlist-i oxuyur, istinad etdiyi
   **bütün seqmentlərin mövcud olduğunu** yoxlayır.
7. Bütün pillələr bitəndə server **master playlist-i özü yazır** və videonu
   `ready` edir. Yarımçıq çıxış heç vaxt yayıma çıxmır.

Tapşırıqlar **video üzrə** paylanır: əvvəlcə ən köhnə videonun bütün pillələri
götürülür (müxtəlif cihazlara düşə bilər), video tamamlanır, sonra növbəti.
Əvvəl resolution üzrə sıralandıqda eyni cihaz bütün 1080p-ləri əvvəl götürür və
heç bir video tam hazır olmurdu.

### R2 quruluşu

```
{tenant}/videos/raw/{ad}.mp4                    ← orijinal (dəyişməz)
{tenant}/videos/m3u8/{videoId}-{ad-sluq}/v{versiya}/
├── master.m3u8                                 ← koordinator yazır
├── thumb.jpg                                   ← ən aşağı pillə yaradır
├── 360p/{index.m3u8, segment_00000.ts, …}
├── 480p/…
└── 720p/…

Məsələn: kela/videos/m3u8/33-content-video/v1/720p/index.m3u8

Qovluqda ID də saxlanılır — iki dərsin adı eyni ola bilər və video sonradan
adlansa artıq yayımlanmış fayllar yetim qalmasın.
```

`v{versiya}` — yenidən konversiyada artır. Yeni versiya tam hazır olana qədər
köhnə `playback_url` yayımda qalır.

## Resurs profilləri

Cihazın gücü müxtəlifdir — güclü masaüstü tam gücü ilə işləməli, telefon isə
istifadəçini narahat etməməlidir. Buna görə **avtomatik seçilən** resurs profili var:

| Profil | CPU axını | Paralel iş | NVENC preset | Prioritet |
|---|---|---|---|---|
| `max` — Maksimum güc | bütün nüvələr | nüvə / 4 | `p1` | normal |
| `balanced` — Balanslı | nüvə − 1 | nüvə / 6 | `p4` | normal |
| `low` — Aşağı resurs | nüvə / 2 | 1 | `p6` | aşağı (below normal) |

`auto` (default) seçildikdə worker cihazın **ölçülmüş** göstəricilərinə baxır:

- batareya ilə işləyirsə **və ya** ≤ 4 nüvə → `low`
- aparat encoder-i **və** ≥ 8 nüvə → `max`
- qalan hallar → `balanced`

Başqa sözlə: telefon kabeldən ayrılanda özü aşağı profilə keçir; güclü masaüstü
NVENC ilə maksimuma çıxır. İstifadəçi **Tənzimləmələr → Resurs profili**-ndən
əl ilə də seçə bilər.

Açıq limitlər (`Maks. CPU axını`, `Maks. eşzamanlı görev`) profili **üstələmir** —
`0` qoyulsa profil özü qərar verir. Profilin həll olunmuş dəyərləri heartbeat ilə
serverə göndərilir, koordinator cihazın həqiqi tutumunu ona görə hesablayır.

## Kodlaşdırma zolaqları (GPU + CPU)

Hər paralel iş bir **zolaq** tutur; hər zolaq müstəqil kodlaşdırma qurğusudur:

```
Kodlaşdırma zolaqları: GPU 0·1 nvenc, GPU 0·2 nvenc, … GPU 0·6 nvenc, CPU 1 x264, CPU 2 x264
```

- Hər NVIDIA kartı üçün bir neçə NVENC zolağı açılır (kart eyni vaxtda bir neçə
  sessiya apara bilir), `-gpu N` ilə kart seçilir — çoxkartlı maşında hamısı işlənir.
- CPU zolaqları (`libx264`) **eyni vaxtda** qaçır: GPU sürətli, CPU isə maşının boş
  qalan gücünü işlədir. Nüvə sayına görə 1–4 zolaq.
- Zolaqların sayı paralelliyin fiziki tavanıdır; profil və yaddaş/disk onu yalnız
  aşağı sala bilər.

### Böyük videolar

Mənbə faylın ölçüsü koordinatorun HEAD sorğusu ilə tapşırığa əlavə olunur.
500 MB-dan böyük video işlənərkən worker bütün zolaqları **həmin videonun** qalan
pillələrinə yönəldir (`PreferVideoId`) — maşının gücü bir işə toplanır, digər
videolara dağılmır.

## Təhlükəsizlik

- Cihaz **birdəfəlik qeydiyyat kodu** ilə qoşulur, sonra öz access tokeni ilə işləyir.
- Token bazada yalnız **SHA-256 hash** kimi saxlanılır; rotasiya və ləğv dərhal təsir edir.
- Worker-a **R2 açarı verilmir** — yalnız qısaömürlü presigned GET/PUT URL-ləri.
- Hər cihaz yalnız **özünə verilmiş** tapşırığı görə bilər (`worker_node_id` yoxlanır).
- Fencing token köhnə cihazın gecikmiş nəticəsini rədd edir.
- Yükləmə açarları tapşırığın öz qovluğu ilə məhdudlaşır (`..`, mütləq yol, sxem rədd olunur).
- ffmpeg `ArgumentList` ilə işə salınır — shell yoxdur, injection mümkün deyil.

## Quraşdırma

### Server

```bash
dotnet ef database update --project Edutor.Core --startup-project Edutor.Api
```

`appsettings.json` → `Nodes` bölməsi (lease müddəti, maks. cəhd, merdiven).

### Worker

```bash
dotnet run --project Edutor.Nodes.App --framework net10.0
```

1. **Tənzimləmələr** → API ünvanını yaz, yadda saxla.
2. Admin paneldən (`/node` → Qeydiyyat kodları) kod yarat.
3. Kodu **Tənzimləmələr → Cihaz qeydiyyatı** sahəsinə yaz, "Qeydiyyatdan keç".
4. Dashboard → **İşə sal**.

Android: `dotnet build Edutor.Nodes.App -p:BuildAndroid=true -f net10.0-android`

## Testlər

```bash
dotnet test Edutor.Tests --filter "FullyQualifiedName~Nodes"
```

- `EncodePlanTests` — upscale olmaması, axın seçimi, GOP hizalanması.
- `PerformanceProfileTests` — profilin cihaza uyğun seçilməsi və ffmpeg əmr sətrinə çatması.
- `FfmpegIntegrationTests` — **real ffmpeg** ilə kodlaşdırma, playlist/seqment doğrulaması.
- `NodeSchedulingTests` — tapşırıq yaradılması, təkrar yaradılmaması, tutum, mənbə ləğvi.
- `JobLifecycleTests` — lease bitməsi, fencing, təkrar tamamlama, seqment çatışmazlığı, yol qaçışı.
- `WorkerEndToEndTests` — **real worker → real API → yayıma çıxmış video**.

> ⚠️ Testlər `Local` storage və Testcontainers bazası ilə işləyir. `ApiFactory`
> real bucket-a və ya inkişaf bazasına qoşulmağa cəhd olsa **dərhal dayanır**.

## Dəstəklənən platformalar

| Platforma | Status |
|---|---|
| Linux x64 | ✅ sınaqdan keçirilib (NVENC + libx264) |
| Windows x64 | ✅ kod platformadan asılı deyil; ffmpeg PATH-dən tapılır |
| **Android arm64** | ✅ sınaqdan keçirilib (MediaCodec + libx264) |
| macOS | ❌ dəstəklənmir (tələb olunmur) |
| Android armv7 / x86 | ❌ dəstəklənmir — APK yalnız `arm64-v8a` üçün qurulur |

Android paketi qəsdən yalnız `arm64-v8a` üçündür: ffmpeg ikililəri də yalnız o ABI
üçün derlənib. Başqa ABI-ləri paketləsək tətbiq həmin cihazlara qurular, sonra isə
heç nə edə bilməzdi.

## Məlum məhdudiyyətlər

- **Segment səviyyəsində paylama yoxdur.** Bir video üçün ən çoxu dörd paralel iş
  açılır (hər çözünürlük üçün bir), ona görə 8 zolağı olan maşında belə tək video
  üzərində 4-dən çox iş getmir. Model genişlənməyə hazırdır, amma tətbiq olunmayıb.
- **Yük testi aparılmayıb.** İki cihaz və ~200 iş ilə sınaqdan keçirilib; yüzlərlə
  eyni vaxtlı cihazda davranış ölçülməyib.
- Android paketi **Debug** quruluşudur — paylama üçün Release/AOT tövsiyə olunur.
