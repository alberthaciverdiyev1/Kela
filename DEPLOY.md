# Edutor — Production Deployment

Docker istifadə olunmur: hər host `dotnet publish` çıxışı kimi birbaşa sunucuda (systemd)
işləyir, qarşısında nginx TLS-i sonlandırır. E-poçt göndərilmir (`NoOpEmailSender`).

## 1. Nə harada işləyir

| Komponent | Host | Port (nümunə) |
|---|---|---|
| `Edutor.Api` | app server | `127.0.0.1:5080` |
| `Edutor.Admin` | app server | `127.0.0.1:5081` |
| PostgreSQL | DB server / managed | `5432` |
| `Edutor.App.Web` | statik fayllar (nginx) | — |
| `Edutor.App` (MAUI) | istifadəçi cihazı / store | — |
| `Edutor.Nodes.*` | encoding cihazı | — |

API və Admin **eyni bazaya** qoşulur. `Storage:Provider=Local` olduqda hər ikisi eyni
`Storage:RootPath` qovluğunu görməlidir; ayrı maşınlardadırsa `R2` istifadə edin.

## 2. Build və publish

```bash
# CSS əvvəlcə (app.css faylları git-ə düşmür)
cd admin/Edutor.Admin && npm ci && npm run css:build && cd ../..
# Edutor.App.Shared-ın öz package.json-u yoxdur — admin-in CLI-si ilə build olunur
admin/Edutor.Admin/node_modules/.bin/tailwindcss \
  -i web/Edutor.App.Shared/wwwroot/css/input.css \
  -o web/Edutor.App.Shared/wwwroot/css/app.css --minify

dotnet publish api/Edutor.Api        -c Release -o /srv/edutor/api
dotnet publish admin/Edutor.Admin    -c Release -o /srv/edutor/admin
dotnet publish web/Edutor.App.Web    -c Release -o /srv/edutor/web
```

WASM istemcisinin API ünvanı publish-dən **əvvəl** `web/Edutor.App.Web/wwwroot/appsettings.json`
içində production dəyərinə qoyulmalıdır (`Api:BaseUrl`). MAUI üçün eyni dəyər
`web/Edutor.App/Resources/Raw/appsettings.json` faylındadır və APK build-indən əvvəl dəyişdirilir.

## 3. Konfiqurasiya (environment variables)

`ASPNETCORE_ENVIRONMENT=Production`. `appsettings.json` yalnız sxem daşıyır; bütün
secret-lər env ilə verilir (`__` = bölmə ayırıcısı).

| Dəyişən | Məcburi | Qeyd |
|---|---|---|
| `ConnectionStrings__Postgres` | ✅ | `Host=...;Database=edutor;Username=...;Password=...` |
| `Jwt__Key` | ✅ | ≥ 32 simvol, təsadüfi |
| `Admin__Email`, `Admin__Password` | ✅ | ilk superadmin |
| `DefaultAccounts__Admin__Email/Password` | ✅ | hər tenant üçün default admin |
| `DefaultAccounts__Student__Email/Password` | ✅ | hər tenant üçün default şagird |
| `Worker__ApiKey` | ✅ | node callback-ləri üçün |
| `Storage__Provider` | ✅ | `Local` / `R2` |
| `Storage__RootPath` | Local | məs. `/var/lib/edutor/uploads` |
| `Storage__Images__Enabled` | | şəkilləri yükləmə zamanı optimallaşdır (default `true`) |
| `Storage__Images__MaxDimension` | | uzun tərəfin maks. pikseli (default `1920`) |
| `Storage__Images__JpegQuality` | | JPEG keyfiyyəti 1–100 (default `82`) |
| `Storage__Images__MinSizeKb` | | bundan kiçik şəkillərə toxunulmur (default `24`) |
| `R2__AccountId/AccessKeyId/SecretAccessKey/BucketName/PublicBaseUrl` | R2 | |
| `Cors__Origins__0` | ✅ | WASM tətbiqinin domeni (boş olsa API hər origin-ə açıqdır) |
| `Database__MigrateOnStartup` | | `true` və ya migration əl ilə |
| `ReverseProxy__Enabled` | ✅ | nginx arxasındadırsa `true` |
| `ReverseProxy__KnownProxies__0` | | proxy başqa maşında olsa onun IP-si |
| `WebPush__Subject/PublicKey/PrivateKey` | Push | hər iki host-da (API + Admin) **eyni** açarlar; `npx web-push generate-vapid-keys` |
| `DataProtection__KeysPath` | Admin | məs. `/var/lib/edutor/keys` |
| `Api__BaseUrl` | Admin | admin JS-in müraciət etdiyi API ünvanı |
| `App__BaseUrl` | Admin | mobil tətbiqə yönləndirmə linkləri |
| `Logging__File__Path` | | gündəlik log qovluğu, məs. `/var/www/edutor/shared/logs` (boş olsa fayla yazılmır) |

Nümunə `/etc/edutor/api.env`:

```ini
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://127.0.0.1:5080
ConnectionStrings__Postgres=Host=127.0.0.1;Port=5432;Database=edutor;Username=edutor;Password=<gizli>
Jwt__Key=<32+ simvol>
Admin__Email=admin@example.com
Admin__Password=<güclü>
DefaultAccounts__Admin__Email=admin@example.com
DefaultAccounts__Admin__Password=<güclü>
DefaultAccounts__Student__Email=student@example.com
DefaultAccounts__Student__Password=<güclü>
Worker__ApiKey=<təsadüfi>
Storage__Provider=R2
R2__AccountId=...
R2__AccessKeyId=...
R2__SecretAccessKey=...
R2__BucketName=edutor
R2__PublicBaseUrl=https://cdn.example.com
WebPush__Subject=mailto:info@example.com
WebPush__PublicKey=<vapid public>
WebPush__PrivateKey=<vapid private>
Cors__Origins__0=https://app.example.com
ReverseProxy__Enabled=true
Database__MigrateOnStartup=false
```

`SecretGuard` bu dəyərlər olmadan (və ya zəif/`password`/`admin123` ilə) uygulamanı
başlatmır — bu gözlənilən davranışdır.

## 4. Reverse proxy (nginx)

```nginx
server {
    listen 443 ssl http2;
    server_name api.example.com;

    ssl_certificate     /etc/letsencrypt/live/api.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.example.com/privkey.pem;

    client_max_body_size 200m;

    location / {
        proxy_pass         http://127.0.0.1:5080;
        proxy_http_version 1.1;
        proxy_set_header   Host              $host;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_set_header   Upgrade           $http_upgrade;   # SignalR
        proxy_set_header   Connection        "upgrade";        # SignalR
    }
}

server {
    listen 443 ssl http2;
    server_name panel.example.com;

    ssl_certificate     /etc/letsencrypt/live/panel.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/panel.example.com/privkey.pem;

    location / {
        proxy_pass         http://127.0.0.1:5081;
        proxy_set_header   Host              $host;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}

server {
    listen 443 ssl http2;
    server_name app.example.com;
    root /srv/edutor/web/wwwroot;

    location / {
        try_files $uri $uri/ /index.html =404;
    }
}
```

`ReverseProxy__Enabled=true` olmadan `X-Forwarded-*` nəzərə alınmır (birbaşa açıq host-da
spoofing-in qarşısını alır). Proxy eyni maşındadırsa `KnownProxies` lazım deyil (loopback
default olaraq etibarlıdır).

## 5. Verilənlər bazası

```bash
dotnet ef database update \
  --project shared/Edutor.Core \
  --startup-project api/Edutor.Api
```

- `Database__MigrateOnStartup=false` (default) → migration deploy addımında ayrıca işlədilir.
- İlk açılışda seed işləyir: `default` tenant + superadmin + default hesablar.
- `Seed__SampleTenants` artıq `false` (appsettings) — production-da nümunə tenant yaranmır.
- Deployment öncəsi backup, sonrası smoke test.

## 6. systemd (nümunə)

`/etc/systemd/system/edutor-api.service`:

```ini
[Unit]
Description=Edutor API
After=network.target postgresql.service

[Service]
WorkingDirectory=/srv/edutor/api
ExecStart=/usr/bin/dotnet /srv/edutor/api/Edutor.Api.dll
EnvironmentFile=/etc/edutor/api.env
Restart=always
RestartSec=5
User=edutor

[Install]
WantedBy=multi-user.target
```

`edutor-admin.service` eyni formada (`WorkingDirectory=/srv/edutor/admin`,
`ExecStart=.../Edutor.Admin.dll`, `EnvironmentFile=/etc/edutor/admin.env`).

```bash
systemctl daemon-reload
systemctl enable --now edutor-api edutor-admin
```

## 7. Sağlamlıq və monitorinq

- `GET /health` — hər iki host-da anonim; baza əlçatandırsa `200 {"status":"ok"}`,
  deyilsə `503`. nginx/uptime monitorinqi buna baxa bilər.
- Log-lar stdout-a düşür → `journalctl -u edutor-api -f`.
- Admin cookie-ləri üçün `DataProtection__KeysPath` verin; verilməzsə restart-dan sonra
  bütün istifadəçilər yenidən login olur.

## 8. Yoxlama siyahısı

- [ ] DNS: `api.*`, `panel.*`, `app.*`
- [ ] TLS sertifikatları (yenilənmə avtomatik)
- [ ] PostgreSQL güclü parol, yalnız daxili şəbəkəyə açıq
- [ ] `Cors__Origins` production domeni ilə dolu
- [ ] `Storage` qovluğu (`/var/lib/edutor/uploads`) `edutor` istifadəçisinə yazıla bilir
- [ ] R2 bucket + public URL (və ya paylaşılmış disk)
- [ ] Migration tətbiq edildi, seed hesablarının parolları dəyişdirildi
- [ ] İlk login: `panel.*` superadmin ilə, `/health` 200
- [ ] WASM `appsettings.json` API ünvanı düzgün; MAUI APK imzalanıb
- [ ] Node worker-lar `Worker__ApiKey` və API ünvanı ilə konfiqurasiya edildi
- [ ] DB backup + `uploads`/R2 backup
- [ ] Log toplama və uptime alert

## 9. Avtomatik deploy (`deploy.sh`)

Serverdə `deploy.sh` (root) bütün addımları yerinə yetirir: GitHub-dan pull → DB backup →
yalnız api/admin/web build → yeni release qovluğu → `current` symlink-inin dəyişdirilməsi →
servislərin restartı → health check. Xəta olarsa əvvəlki release-ə avtomatik qayıdır.

```bash
/var/www/edutor/deploy.sh                # pull + build + deploy
/var/www/edutor/deploy.sh --skip-pull    # serverdəki checkout-u deploy et
/var/www/edutor/deploy.sh --list         # mövcud release-lər
/var/www/edutor/deploy.sh --rollback     # əvvəlki release-ə qayıt
/var/www/edutor/deploy.sh --rollback 20261028-1926-7d40a65
```

Qovluq strukturu:

```
/var/www/edutor/
├── src/                     # git checkout (deploy üçün mənbə)
├── releases/<tarix>-<sha>/  # {api,admin,web} publish çıxışları
├── current -> releases/...  # systemd və nginx bunu istifadə edir
└── shared/
    ├── keys/                # DataProtection açarları
    ├── logs/                # gündəlik loglar: {admin,api}-YYYYMMDD.log (son 30 gün)
    └── backups/             # pg_dump -Fc yedəkləri (son 10 saxlanılır)
```

- Son 3 release saxlanılır (`KEEP_RELEASES`), deploy zamanı `bin/obj` təmizlənir.
- Rollback yalnız **kodu** geri qaytarır; migration geri alınmır. Migration səbəbli
  problemdə bazanı `shared/backups`-dakı dump-dan bərpa edin:
  `pg_restore -h 127.0.0.1 -U edutor -d edutor --clean --if-exists <dump>`.
- `Seed__SampleTenants=false` hər iki host-un env faylında olmalıdır — əks halda təzə
  bazada nümunə kurslar yaranır.
- GitHub Actions üçün: `ssh` ilə serverdə bu skripti çağırmaq kifayətdir (secret-lər
  serverdə qalır; repo-ya heç bir parol düşmür).

## 10. Bilinən məhdudiyyətlər

- **E-poçt göndərilmir** — OTP ilə şifrə bərpası production-da işləmir; şifrə sıfırlama
  admin panelindən aparılır. Lazım olsa `IEmailSender` implementasiyası əlavə edilməlidir.
- SignalR (`/hubs/feedback`) bir instansiya üçün nəzərdə tutulub; çox instansiya üçün
  sticky session və ya Redis backplane tələb olunur.
- API-də OpenAPI yalnız Development-da açıqdır; production-da əlçatmazdır.
