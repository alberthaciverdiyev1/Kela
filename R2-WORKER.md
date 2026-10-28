# R2 + Video Konversiya Sözleşmesi

Bu dosya **ayrı layihə** olan video konversiya worker-ı üçün API müqaviləsini təsvir edir.
Worker bu repoda deyil — burada yalnız onunla əlaqə nöqtələri var.

## Bucket quruluşu

Hər tenant öz qovluğunda (tenant-ın `slug`-ı ilə):

```
{tenant}/videos/raw/{guid}.mp4      ← admin birbaşa yükləyir (presigned PUT)
{tenant}/videos/m3u8/...            ← worker bura yazır (m3u8 + seqmentlər + thumbnail)
{tenant}/voices/{guid}.mp3
{tenant}/files/{guid}.pdf
```

Məsələn tenant "Edutor Academy" (slug `edutor`) üçün: `edutor/videos/raw/0dc9....mp4`.

> Qovluq adı tenant-ın **slug**-ıdır, adı deyil — ad boşluq və Azərbaycan hərfləri
> daşıya bilər, slug isə URL-təhlükəsizdir. Tenant adı dəyişsə də mövcud fayllar
> qırılmır: DB-də tam açar saxlanılır, yalnız yeni yükləmələr yeni qovluğa düşür.

## Bucket CORS (məcburi)

Brauzer faylı birbaşa R2-yə qoyduğu üçün bucket-da CORS icazəsi olmalıdır; olmasa
`PUT` preflight-da düşür və istifadəçi "Fayl birbaşa yüklənə bilmədi…" xətası alır
(böyük fayllar serverdən də keçmədiyi üçün yükləmə tam dayanır). Cloudflare paneldə
**R2 → edutor → Settings → CORS Policy** bölməsinə bu siyasət yazılır:

```json
[
  {
    "AllowedOrigins": [
      "https://platform-edutor.foxsoft.online",
      "https://edutor-api.foxsoft.online",
      "http://localhost:5081",
      "http://localhost:5091"
    ],
    "AllowedMethods": ["GET", "PUT", "HEAD"],
    "AllowedHeaders": ["*"],
    "ExposeHeaders": ["ETag"],
    "MaxAgeSeconds": 3600
  }
]
```

`ETag` xüsusilə vacibdir: çoxhissəli (multipart) yükləmədə hər hissənin cavabından
`ETag` oxunur.

## Axın

1. **Admin yükləyir** — brauzer `POST /lesson-content/upload-url` ilə presigned PUT URL-i alır
   və faylı **birbaşa R2-yə** qoyur (serverdən keçmir). Sonra `path` ilə `POST /lesson-content`
   çağırır. API yolun həqiqətən bucket-da olduğunu yoxlayır (`ExistsAsync`).
2. **DB sətri yaranır** — `videos.conversion_status = 'pending'`, `playback_url` boş.
3. **Worker işə düşür** — R2 event notification (`{tenant}/videos/raw/` prefiksi) kuyruğa düşür.
   Worker m3u8 + thumbnail hazırlayıb `{tenant}/videos/m3u8/` altına yazır.
4. **Worker nəticəni bildirir** — aşağıdakı callback uclarından biri ilə.
5. **Tətbiq oynadır** — `playback_url` `.m3u8` ilə bitirsə hls.js işə düşür.

## Callback ucları

Baza: `{API}/api/video`. Autentifikasiya: **`X-Worker-Key`** başlığı
(`Worker:ApiKey` konfiqurasiyası; boşdursa uclar tamamilə bağlıdır).

### Konversiya başlayır

```
POST /api/video/processing
X-Worker-Key: <açar>

{ "sourcePath": "edutor/videos/raw/0dc9....mp4" }
```

### Konversiya bitir

```
POST /api/video/processed
X-Worker-Key: <açar>

{
  "sourcePath": "edutor/videos/raw/0dc9....mp4",
  "playbackUrl": "edutor/videos/m3u8/0dc9.../master.m3u8",
  "thumbnail": "edutor/videos/m3u8/0dc9.../thumb.jpg",
  "durationSeconds": 754,
  "status": "ready"
}
```

Uğursuzluqda:

```
{ "sourcePath": "...", "status": "failed", "error": "ffmpeg exit 1: ..." }
```

| Sahə | Qeyd |
|---|---|
| `sourcePath` | **Məcburi.** `videos.path` ilə eyni olmalıdır — video bununla tapılır |
| `playbackUrl` | m3u8 açarı (nisbi yol) və ya tam URL. Nisbi verilsə API `PublicBaseUrl` ilə birləşdirir |
| `thumbnail` | Eyni qayda |
| `durationSeconds` | Yalnız `> 0` olduqda yazılır |
| `status` | `ready` və ya `failed` — başqa dəyər 400 qaytarır |
| `error` | Yalnız `failed` olduqda saxlanılır (500 simvola kəsilir) |

## Cavablar

| Kod | Məna |
|---|---|
| `200` | Qeyd olundu |
| `401` | `X-Worker-Key` yoxdur və ya yanlışdır |
| `404` | Bu `sourcePath` üçün **video tapılmadı** |

**404 vacibdir.** R2 event-i yükləmə bitən kimi gəlir, lakin DB sətri istemci
`POST /lesson-content` çağırışını etdikdən sonra yaranır — yəni event sətirdən
**əvvəl** gələ bilər. Bu halda worker 404 alır və **kuyruk təkrar cəhd etməlidir**
(məsələn 30 saniyə sonra, bir neçə dəfə). Uclar idempotentdir: eyni sorğu təkrar
göndərilə bilər.

## Vacib qaydalar

- **Uğursuz konversiya mövcud m3u8-i silmir.** Əvvəl uğurlu olubsa köhnə
  `playback_url` qalır və istifadəçi videonu oynatmağa davam edir; yalnız
  `conversion_status` → `failed` olur.
- **`sourcePath` dəyişməzdir** — orijinal faylı silməyin, yenidən cəhd üçün lazımdır.
- Presigned PUT yalnız `video/*` üçün deyil, hər növ üçün işləyir; worker yalnız
  `{tenant}/videos/raw/` altındakılarla maraqlanır.

## Test üçün əlverişli

Worker yazmadan zənciri yoxlamaq:

```bash
curl -X POST {API}/api/video/processed \
  -H "X-Worker-Key: $KEY" -H "Content-Type: application/json" \
  -d '{"sourcePath":"edutor/videos/raw/xxx.mp4","playbackUrl":"edutor/videos/m3u8/xxx/master.m3u8","status":"ready"}'
```
