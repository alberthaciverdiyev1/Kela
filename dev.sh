#!/usr/bin/env bash
# Bütün layihələri Development rejimində, WATCH (hot-reload) ilə işə salır.
#   API + MVC (dotnet watch) + Tailwind (watch)
# İstifadə: ./dev.sh    (dayandırmaq üçün Ctrl+C)
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

export ASPNETCORE_ENVIRONMENT=Development
export DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1

API_URL="${API_URL:-http://localhost:5080}"
MVC_URL="${MVC_URL:-http://localhost:5081}"

pids=()
cleanup() {
  echo ""
  echo "==> Dayandırılır..."
  for pid in "${pids[@]:-}"; do
    kill "$pid" 2>/dev/null || true
  done
  wait 2>/dev/null || true
  echo "==> Hamısı dayandı."
}
trap cleanup EXIT INT TERM

echo "==> API (watch): $API_URL"
ASPNETCORE_URLS="$API_URL" dotnet watch --project LanguageLMS.Api run --no-launch-profile &
pids+=($!)

echo "==> MVC (watch): $MVC_URL"
ASPNETCORE_URLS="$MVC_URL" dotnet watch --project LanguageLMS.Mvc run --no-launch-profile &
pids+=($!)

if [ -d "LanguageLMS.Mvc/node_modules" ]; then
  echo "==> Tailwind ilkin build"
  ( cd LanguageLMS.Mvc && npm run css:build )
  echo "==> Tailwind (watch)"
  ( cd LanguageLMS.Mvc && npm run css:watch ) &
  pids+=($!)
else
  echo "==> node_modules yoxdur — Tailwind watch atlanır (əvvəlcə 'cd LanguageLMS.Mvc && npm install')"
fi

echo ""
echo "----------------------------------------------------------"
echo " API   : $API_URL   (openapi: $API_URL/openapi/v1.json)"
echo " MVC   : $MVC_URL"
echo " Watch : hər üç proses dəyişiklikləri avtomatik yeniləyir"
echo " Stop  : Ctrl+C"
echo "----------------------------------------------------------"
echo ""

wait
