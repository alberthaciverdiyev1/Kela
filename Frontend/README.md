# Kela Frontend — SvelteKit SPA (JavaScript)

Eski Razor/Web (7288) yapısının yanına paralel büyüyen yeni ön yüz.
Tek backend: `Kela.Api` (https://localhost:7047, cookie auth).

## Çalıştırma

```bash
# 1) API açık olmalı
ASPNETCORE_ENVIRONMENT=Development dotnet run --project ../src/Kela.Api --launch-profile https

# 2) Svelte dev
npm install
npm run dev          # http://localhost:5173
```

Vite proxy'si `/api` isteklerini `https://localhost:7047`'ye yönlendirir.
Cookie (`Kela.Auth`) `localhost` güvenilir origin olduğu için `Secure` olsa da
dev'de http://localhost:5173 üzerinden saklanır ve otomatik gönderilir.

## Yapı

```
src/
├── app.html              # SPA kabuğu
├── app.css               # Tailwind v4 + DaisyUI "kela" teması (.NET app.css ile aynı)
├── lib/
│   ├── api.js            # fetch sarmalayıcı: 401 → /auth/login, envelope → data
│   ├── auth.js           # user/ready store + login/logout + role yönlendirme
│   ├── i18n.js           # $t(key, params) + locale store (tr/az/en/ru)
│   ├── notify.js         # toast (sweetalert2 / alertify)
│   ├── icons.js          # ikon path'leri (mevcut icons.js'den taşındı)
│   ├── locales/          # messages_{lang}.json (.NET'ten kopyalandı)
│   └── components/       # AppIcon, AppBtn, AppPageHead, AppModal
└── routes/
    ├── +layout.svelte    # kök kabuk + auth init + tema
    ├── +page.svelte      # role'e göre yönlendirme
    ├── auth/login/       # giriş
    ├── auth/register/    # kayıt
    ├── blocked/          # erişim yok
    └── teacher/          # öğretmen kabuğu (guard + nav) + dashboard
```

## Lokalizasyon senkronu

`.NET` tarafındaki anahtarları güncellemek için:

```bash
for l in tr az en ru; do
  cp ../src/Kela.Web/Localization/messages_$l.json src/lib/locales/messages_$l.json
done
```

## Notlar

- SPA modu: `src/routes/+layout.js` içinde `ssr = false` + `adapter-static` fallback.
- Auth tamamen cookie tabanlı; JWT yok. `/api/users/me` endpoint'i bu migrasyon
  için API'ye eklendi (Svelte mevcut kullanıcıyı oradan alır).
- Eski Web (7288) aynen çalışıyor — geri dönüş hep açık.
