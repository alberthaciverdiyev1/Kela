#!/usr/bin/env bash
# Kela mobil app (MAUI Blazor Hybrid) — Android cihaza qurub işə salır.
# İstifadə:  ./android.sh            (Debug, telefona deploy + run)
#            ./android.sh -c Release
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# --- Alətlər (lazım gələrsə dəyişdir) ---
export JAVA_HOME="${JAVA_HOME:-$HOME/android-tools/jdk}"
export ANDROID_HOME="${ANDROID_HOME:-$HOME/Android/Sdk}"
ADB="${ADB:-/opt/android-sdk/platform-tools/adb}"

CONFIG="Debug"
[ "${1:-}" = "-c" ] && CONFIG="${2:-Release}"

API_URL="${API_URL:-http://localhost:5080}"
APP_ID="${APP_ID:-com.kela.app}"

echo "==> JDK : $JAVA_HOME"
echo "==> SDK : $ANDROID_HOME"

# Cihaz yoxlaması
if ! "$ADB" get-state >/dev/null 2>&1; then
  echo "!!! Android cihaz tapılmadı. USB ilə qoş və 'adb devices' ilə yoxla." >&2
  exit 1
fi
"$ADB" devices

# Telefonun localhost:<port>-unu dev maşına yönləndir (app http://localhost:5080 istifadə edir)
PORT="${API_URL##*:}"
echo "==> adb reverse tcp:$PORT tcp:$PORT"
"$ADB" reverse "tcp:$PORT" "tcp:$PORT"

echo "==> Build & deploy ($CONFIG) ..."
dotnet build "$ROOT/Kela.App/Kela.App.csproj" \
  -f net10.0-android -c "$CONFIG" -t:Run \
  -p:AndroidSdkDirectory="$ANDROID_HOME" \
  -p:JavaSdkDirectory="$JAVA_HOME"

echo "==> Hazırdır. App: $APP_ID"
echo "    (Telefonu çıxarıb taxsan, əvvəlcə: $ADB reverse tcp:$PORT tcp:$PORT)"
