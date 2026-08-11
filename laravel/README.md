# Kela — Education Platform

Laravel 13 tətbiqi: dərslər, quizlər, iş sahələri və şagirdlər üçün təhsil idarəetmə platforması. **Frontend və backend tam ayrıdır** — səhifələr server-rendered Blade ilə çəkilir, JS yalnız lazım olan hissəni `/api/v1` JSON endpointləri ilə yeniləyir. Modellərə birbaşa toxunulmur — hər şey Application servisləri üzərindən axır.

## Layering — Web / API / Core

```
┌──────────────────────────────┐     ┌──────────────────────────────────────┐
│ WEB (server-rendered Blade)  │     │ API (backend JSON)                   │
│ app/Web/Controllers/**       │     │ app/Api/Controllers/**  (thin JSON)  │
│ app/Web/Controllers/Teacher/ │     │ app/Api/Middleware/**   (role 403)   │
│ app/Web/Middleware/**        │     │ app/Api/Resources/**    (JSON map)   │
│ resources/views/teacher/**   │     │ routes/api.php  →  /api/v1/*        │
│ routes/web.php               │     │                                      │
└──────────────┬───────────────┘     └──────────────┬───────────────────────┘
               │                                    │
               └──────────► Application services ◄──┘
                           (app/Application/**)
                                   │
                                   ▼
                        Domain / Infrastructure repos
                        (app/Domain/**, app/Infrastructure/Persistence/**)
```

**Qızıl qayda:** nə Web controller, nə də API controller modellərlə birbaşa danışmır. Hər ikisi yalnız `app/Application/**` servisləri üzərindən işləyir.

## Backend API (v1)

- Bazası: `routes/api.php` (prefix `/api/v1`), auth: **Sanctum Bearer token**
- Giriş: `POST /api/v1/auth/login` → `{ token, token_type, user }`
- Web səhifələrindən edilən JS çağrıları sessiya (cookie) ilə də doğrulanır (`auth:sanctum` web-guard fallback).
- Admin/Müəllim resursları `role_api:Admin,Teacher` ilə qorunur (student üçün 403).

| Metod | Endpoint | Açıqlama |
|---|---|---|
| POST | `/auth/login` | token əldə et |
| POST | `/auth/logout` | tokenləri ləğv et |
| GET | `/auth/me` | cari istifadəçi |
| GET | `/cities` | lüğət məlumatı |
| CRUD | `/students` | şagirdlər |
| CRUD | `/lessons` | dərslər (show → `viewer.stream_url/thumbnail_url`) |
| GET | `/lessons/{contentId}/stream` `/thumbnail` | video/şəkil media |
| CRUD | `/quizzes` | quizlər (show → `questions`) |
| POST/DELETE | `/quizzes/{id}/questions[/{questionId}]` | sual əlavə/çıxar |
| POST | `/quizzes/{id}/questions/{qid}/move` | sıralama (`up`/`down`) |
| CRUD | `/questions` | sual bankı |
| CRUD | `/workspaces` | iş sahələri (show → `students` + `directory`) |
| POST/DELETE | `/workspaces/{id}/students[/{studentId}]` | şagird əlavə/çıxar |
| POST | `/workspaces/{id}/folders` `/contents` | qovluq/məzmun |
| POST | `/workspaces/{id}/nodes/{nodeId}/move` `/rename` | node hərəkəti/adı |
| DELETE | `/workspaces/{id}/nodes/{nodeId}` | node/qovluq ağacını sil |

İstifadə nümunəsi:

```bash
TOKEN=$(curl -s -X POST http://localhost:8000/api/v1/auth/login \
  -H "Accept: application/json" -H "Content-Type: application/json" \
  -d '{"email":"teacher@example.com","password":"password"}' | jq -r .data.token)

curl http://localhost:8000/api/v1/lessons -H "Authorization: Bearer $TOKEN"
```

## Frontend (Web UI — Teacher Paneli)

- `app/Web/Controllers/Teacher/**` — server-rendered səhifə controller-ları (`teacher.lessons.index` kimi)
- `app/Web/Controllers/**` — auth/media/dashboard controller-ları
- `resources/views/teacher/**` — səhifə görünüşləri (Blade)
- `resources/views/components/teacher/**` — anonim komponentlər (`x-teacher.card`, `x-teacher.button`, ...)
- `resources/views/layouts/teacher.blade.php` — panel layout-u
- `routes/web.php` — səhifə marşrutları (`/teacher/dashboard`, `/teacher/lessons`, `/teacher/quizzes`, `/teacher/students`, `/teacher/workspaces`)
- Auth: web sessiya (`/auth/login`); rol yoxlaması `role:Admin,Teacher` ilə

### "Server-rendered + JS yeniləmə" yanaşması

Səhifələr ilk dəfə tam serverdə çəkilir. Yalnız dinamik hissələr JS ilə yenilənir:

- **Quiz redaktoru** — sual əlavə/düzləndir/sırala/çıxar: JS `/api/v1/quizzes/*` çağırır, sonra `GET /teacher/quizzes/{id}/questions` server-rendered fragment-i yenidən çəkib DOM-da əvəz edir.
- **Workspace file-manager** — qovluq naviqasiyası GET linkləri (server), node əməliyyatları JS → `/api/v1/workspaces/*`, kataloq `GET /teacher/workspaces/{id}/directory` fragment-i ilə təzələnir.
- **İndeks səhifələrində silmə** — JS confirm + API DELETE, sonra səhifə yenilənir.
- **Axtarış** — sadə GET form (`?search=`) ilə server-rendered.
- **Tema (açıq/tünd)** — Alpine + `localStorage` (FOUC qarşısı alınır).

Alpine + yardımçılar (`KelaApi`, `KelaFragment`) `resources/js/app.js`-dədir; Livewire istifadə edilmir.

## Test

```bash
php artisan test   # 53 test / 220 assertion
```
