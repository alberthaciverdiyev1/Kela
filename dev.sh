#!/usr/bin/env bash

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

export ASPNETCORE_ENVIRONMENT=Development
export DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1

API_URL="${API_URL:-http://localhost:5080}"
MVC_URL="${MVC_URL:-http://localhost:5081}"
APP_URL="${APP_URL:-http://localhost:5238}"
ENABLE_WEB="${ENABLE_WEB:-1}"
ENABLE_APP="${ENABLE_APP:-1}"
HEALTH_URL="${HEALTH_URL:-$API_URL/openapi/v1.json}"

ARTIFACTS="$ROOT/.dev-artifacts"

TAILWIND="$ROOT/Kela.Mvc/node_modules/.bin/tailwindcss"
MVC_CSS="Kela.Mvc/wwwroot/css/app.css"
APP_CSS="Kela.App.Shared/wwwroot/css/app.css"

pids=()

start() {
  local dir=""
  if [ "${1:-}" = "--in" ]; then dir="$2"; shift 2; fi
  if [ -n "$dir" ]; then
    ( cd "$dir" && exec setsid "$@" ) &
  else
    ( exec setsid "$@" ) &
  fi
  pids+=($!)
}

cleanup() {
  [ "${_cleaned:-0}" = "1" ] && return
  _cleaned=1
  echo ""
  echo "==> Dayandırılır..."
  for pid in "${pids[@]:-}"; do
    kill -- -"$pid" 2>/dev/null || kill "$pid" 2>/dev/null || true
  done
  wait 2>/dev/null || true
  echo "==> Hamısı dayandı."
}
trap cleanup EXIT INT TERM

wait_for() {
  local url="$1" name="$2"
  for _ in $(seq 1 90); do
    if curl -sf -o /dev/null "$url" 2>/dev/null; then
      echo "==> $name hazırdır: $url"
      return 0
    fi
    sleep 1
  done
  echo "==> XƏBƏRDARLIQ: $name vaxtında hazır olmadı ($url)"
  return 1
}

_own_pids() {
  local pat
  for pat in "Kela.Api" "Kela.Mvc" "Kela.App.Web" "Kela.App.Shared" "tailwindcss"; do
    pgrep -f -- "$pat" 2>/dev/null || true
  done | sort -u | grep -vx -e "$$" -e "$PPID" || true
}

kill_existing() {
  local pids alive
  pids="$(_own_pids)"
  if [ -z "$pids" ]; then
    echo "==> Əvvəlcədən açıq proses yoxdur."
    return 0
  fi
  echo "==> Əvvəlki proseslər dayandırılır (API / MVC / App / Tailwind): $(echo $pids | tr '\n' ' ')"
  # shellcheck disable=SC2086
  kill -TERM $pids 2>/dev/null || true
  for _ in $(seq 1 20); do
    alive="$(_own_pids)"
    [ -z "$alive" ] && break
    sleep 0.5
  done
  if [ -n "${alive:-}" ]; then
    # shellcheck disable=SC2086
    kill -KILL $alive 2>/dev/null || true
    sleep 0.5
  fi
  echo "==> Təmizləndi."
}

kill_existing

# ------------------------------------------------------------------
# 0) Tailwind CLI hazırlığı (hər iki tərəf MVC-dəki CLI-dən istifadə edir)
# ------------------------------------------------------------------
if [ ! -x "$TAILWIND" ] && { [ "$ENABLE_WEB" = "1" ] || [ "$ENABLE_APP" = "1" ]; }; then
  echo "==> node_modules / Tailwind CLI yoxdur — 'npm install' çalışdırılır..."
  ( cd Kela.Mvc && npm install )
fi

build_css() { # $1 = input, $2 = output, $3 = ad
  "$TAILWIND" -i "$1" -o "$2" --minify && \
    echo "   $3: $(wc -c < "$2") bayt"
}

# ------------------------------------------------------------------
# 1) WEB tərəfi
# ------------------------------------------------------------------
if [ "$ENABLE_WEB" = "1" ]; then
  echo "==> [WEB] Tailwind CSS (MVC)"
  build_css "Kela.Mvc/wwwroot/css/input.css" "$MVC_CSS" "app.css"

  echo "==> [WEB] Tailwind (watch, nəzarətçi)"
  start bash -c "cd '$ROOT/Kela.Mvc' || exit 1
while true; do
  '$TAILWIND' -i ./wwwroot/css/input.css -o ./wwwroot/css/app.css --watch
  [ -s wwwroot/css/app.css ] || '$TAILWIND' -i ./wwwroot/css/input.css -o ./wwwroot/css/app.css --minify
  sleep 1
done"

  echo "==> [WEB] API (watch): $API_URL"
  ASPNETCORE_URLS="$API_URL" start dotnet watch --project Kela.Api --artifacts-path "$ARTIFACTS/api" run --no-launch-profile

  echo "==> [WEB] MVC (watch): $MVC_URL"
  ASPNETCORE_URLS="$MVC_URL" start dotnet watch --project Kela.Mvc --artifacts-path "$ARTIFACTS/mvc" run --no-launch-profile
else
  echo "==> [WEB] atlanır (ENABLE_WEB=0)"
fi

# ------------------------------------------------------------------
# 2) APP tərəfi (Blazor WASM)
# ------------------------------------------------------------------
if [ "$ENABLE_APP" = "1" ]; then
  echo "==> [APP] Tailwind CSS (App.Shared)"
  build_css "Kela.App.Shared/wwwroot/css/input.css" "$APP_CSS" "app.css"

  echo "==> [APP] Tailwind (watch, nəzarətçi)"
  start bash -c "cd '$ROOT/Kela.App.Shared' || exit 1
while true; do
  '$TAILWIND' -i ./wwwroot/css/input.css -o ./wwwroot/css/app.css --watch
  [ -s wwwroot/css/app.css ] || '$TAILWIND' -i ./wwwroot/css/input.css -o ./wwwroot/css/app.css --minify
  sleep 1
done"

  echo "==> [APP] Blazor WASM (Kela.App.Web, watch): $APP_URL"
  ASPNETCORE_URLS="$APP_URL" start dotnet watch --project Kela.App.Web --artifacts-path "$ARTIFACTS/app" run --no-launch-profile
else
  echo "==> [APP] atlanır (ENABLE_APP=0)"
fi

echo ""
echo "----------------------------------------------------------"
[ "$ENABLE_WEB" = "1" ] && echo " API   : $API_URL   (openapi: $API_URL/openapi/v1.json)"
[ "$ENABLE_WEB" = "1" ] && echo " MVC   : $MVC_URL"
[ "$ENABLE_APP" = "1" ] && echo " App   : $APP_URL   (Blazor WASM -> API: $API_URL)"
echo " Watch : API/MVC/App dəyişiklikləri avtomatik yeniləyir"
echo " Stop  : Ctrl+C"
echo "----------------------------------------------------------"
echo ""

wait
