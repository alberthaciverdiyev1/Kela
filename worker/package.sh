#!/usr/bin/env bash
#
# Edutor Nodes — platforma paketləri.
#
#   ./package.sh            hamısı (linux, mac, android)
#   ./package.sh linux      yalnız Linux (linux-x64)
#   ./package.sh mac        macOS (osx-arm64 + osx-x64, .app bundle)
#   ./package.sh android    Android (imzalı APK, arm64)
#
# Nəticələr `worker/dist/` qovluğuna yazılır. Hər paket öz içində .NET runtime
# daşıyır (self-contained) — istifadəçi heç nə quraşdırmır.
set -Eeuo pipefail

ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
PROJECT=$ROOT/Edutor.Nodes.App
DIST=$ROOT/dist
APP_NAME=Edutor.Nodes.App
VERSION=$(grep -oP '(?<=<Version>)[^<]+' "$PROJECT/Edutor.Nodes.App.csproj" 2>/dev/null || true)
VERSION=${VERSION:-1.0.0}
ICON=$PROJECT/Resources/drawable/icon.png

# Android quruluşu üçün JDK və SDK tələb olunur.
ANDROID_HOME=${ANDROID_HOME:-$HOME/Android/Sdk}
JAVA_SDK_DIRECTORY=${JavaSdkDirectory:-$HOME/Android/jdk}

log()  { printf '\033[1;34m==>\033[0m %s\n' "$*"; }
fail() { printf '\033[1;31mERROR:\033[0m %s\n' "$*" >&2; exit 1; }

publish() {
    local rid=$1 out=$2
    log "publish $rid"
    rm -rf "$out"
    dotnet publish "$PROJECT" -f net10.0 -c Release -r "$rid" --self-contained true \
        -p:DebugType=none -p:DebugSymbols=false -o "$out" >/dev/null
}

make_icon() {
    local size=$1 out=$2
    command -v magick >/dev/null || return 1
    magick "$ICON" -resize "${size}x${size}" "$out"
}

# ---------------------------------------------------------------- linux

build_linux() {
    local stage=$DIST/edutor-nodes-linux-x64
    local publish_dir=$ROOT/obj/publish/linux-x64

    publish linux-x64 "$publish_dir"
    rm -rf "$stage"
    mkdir -p "$stage"
    cp -a "$publish_dir"/. "$stage"/
    chmod +x "$stage/$APP_NAME"

    make_icon 256 "$stage/edutor-nodes.png" || true

    cat > "$stage/edutor-nodes.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=Edutor Nodes
Comment=Video konversiya worker-i
Exec=$APP_NAME
Icon=edutor-nodes
Terminal=false
Categories=Network;Utility;
EOF

    cat > "$stage/README.txt" <<'EOF'
Edutor Nodes (Linux)

1. Bu qovluğu istədiyiniz yerə çıxarın (məs. ~/EdutorNodes).
2. `./Edutor.Nodes.App` faylını işə salın.
3. Menyu qeydiyyatı üçün:
   cp edutor-nodes.desktop ~/.local/share/applications/
   cp edutor-nodes.png ~/.local/share/icons/hicolor/256x256/apps/
   (edutor-nodes.desktop faylındakı Exec sətrini tam yolla dəyişin.)

ffmpeg sistemdə olmalıdır:  sudo apt install ffmpeg   (və ya distro ekvivalenti)
ffmpeg başqa yerdədirsə, Tənzimləmələr bölməsində yolunu göstərin.
EOF

    tar -czf "$DIST/edutor-nodes-linux-x64-$VERSION.tar.gz" -C "$DIST" edutor-nodes-linux-x64
    rm -rf "$stage"
    log "linux → $DIST/edutor-nodes-linux-x64-$VERSION.tar.gz"
}

# ---------------------------------------------------------------- macOS

build_mac() {
    local rid=$1 arch=$2
    local publish_dir=$ROOT/obj/publish/$rid
    local app="$DIST/Edutor Nodes ($arch)"

    publish "$rid" "$publish_dir"

    rm -rf "$app"
    mkdir -p "$app/Edutor Nodes.app/Contents/MacOS" "$app/Edutor Nodes.app/Contents/Resources"
    cp -a "$publish_dir"/. "$app/Edutor Nodes.app/Contents/MacOS"/
    chmod +x "$app/Edutor Nodes.app/Contents/MacOS/$APP_NAME"

    if command -v magick >/dev/null; then
        local iconset=$ROOT/obj/icon.iconset iconset_dir=$ROOT/obj/AppIcon.iconset
        rm -rf "$iconset_dir"; mkdir -p "$iconset_dir"
        for size in 16 32 128 256 512; do
            magick "$ICON" -resize ${size}x${size} "$iconset_dir/icon_${size}x${size}.png"
            magick "$ICON" -resize $((size * 2))x$((size * 2)) "$iconset_dir/icon_${size}x${size}@2x.png"
        done
        # icns yalnız macOS-da (iconutil) hazırlanır; başqa OS-də PNG saxlanılır.
        if command -v iconutil >/dev/null; then
            iconutil -c icns "$iconset_dir" -o "$app/Edutor Nodes.app/Contents/Resources/AppIcon.icns"
        fi
    fi

    cat > "$app/Edutor Nodes.app/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>            <string>Edutor Nodes</string>
    <key>CFBundleDisplayName</key>     <string>Edutor Nodes</string>
    <key>CFBundleIdentifier</key>      <string>com.edutor.nodes</string>
    <key>CFBundleVersion</key>         <string>$VERSION</string>
    <key>CFBundleShortVersionString</key><string>$VERSION</string>
    <key>CFBundlePackageType</key>     <string>APPL</string>
    <key>CFBundleExecutable</key>      <string>$APP_NAME</string>
    <key>CFBundleIconFile</key>        <string>AppIcon</string>
    <key>LSMinimumSystemVersion</key>  <string>11.0</string>
    <key>NSHighResolutionCapable</key> <true/>
    <key>NSHumanReadableCopyright</key><string>Edutor</string>
</dict>
</plist>
EOF

    cat > "$app/README.txt" <<'EOF'
Edutor Nodes (macOS)

1. "Edutor Nodes.app" faylını Applications qovluğuna köçürün.
2. İlk açılışda Gatekeeper xəbərdarlıq verə bilər (paket imzalanmayıb):
   sağ klik → Open, və ya:
   xattr -dr com.apple.quarantine "Edutor Nodes.app"
3. ffmpeg lazımdır:  brew install ffmpeg

Qeyd: Apple silicon üçün ayrıca build var (Edutor Nodes (arm64).app).
Intel Mac-lər üçün "Edutor Nodes (x64).app" istifadə edin.
EOF

    tar -czf "$DIST/edutor-nodes-macos-$arch-$VERSION.tar.gz" -C "$DIST" "Edutor Nodes ($arch)"
    rm -rf "$app"
    log "macos $arch → $DIST/edutor-nodes-macos-$arch-$VERSION.tar.gz"
}

# ---------------------------------------------------------------- android

build_android() {
    [[ -d $ANDROID_HOME ]] || fail "Android SDK tapılmadı: $ANDROID_HOME"
    [[ -d $JAVA_SDK_DIRECTORY ]] || fail "JDK tapılmadı: $JAVA_SDK_DIRECTORY"

    # Sabit imza olmadan hər maşında fərqli debug açarı çıxır və tətbiq telefonda
    # yenilənə bilmir. Açarlar varsa onlar istifadə olunur (repo-ya düşmür).
    local signing=()
    if [[ -n ${EDUTOR_KEYSTORE:-} && -f ${EDUTOR_KEYSTORE} ]]; then
        signing=(
            -p:AndroidKeyStore=true
            -p:AndroidSigningKeyStore="$EDUTOR_KEYSTORE"
            -p:AndroidSigningKeyAlias="${EDUTOR_KEY_ALIAS:-edutor}"
            -p:AndroidSigningStorePass="$EDUTOR_KEYSTORE_PASSWORD"
            -p:AndroidSigningKeyPass="${EDUTOR_KEY_PASSWORD:-$EDUTOR_KEYSTORE_PASSWORD}"
        )
    fi

    log "publish android (arm64)"
    ANDROID_HOME=$ANDROID_HOME JavaSdkDirectory=$JAVA_SDK_DIRECTORY \
        dotnet build "$PROJECT" -p:BuildAndroid=true -f net10.0-android -c Release \
        -p:AndroidPackageFormat=apk "${signing[@]}" >/dev/null

    local apk
    apk=$(find "$PROJECT/bin/Release/net10.0-android" -name '*-Signed.apk' | head -1)
    apk=${apk:-$(find "$PROJECT/bin/Release/net10.0-android" -name '*.apk' | head -1)}
    [[ -n $apk ]] || fail "APK tapılmadı"

    cp "$apk" "$DIST/edutor-nodes-$VERSION.apk"
    log "android → $DIST/edutor-nodes-$VERSION.apk"
}

# ---------------------------------------------------------------- main

main() {
    local target=${1:-all}
    mkdir -p "$DIST"

    case $target in
        linux)              build_linux ;;
        mac)                build_mac osx-arm64 arm64; build_mac osx-x64 x64 ;;
        android)            build_android ;;
        all)                build_linux; build_mac osx-arm64 arm64; build_mac osx-x64 x64; build_android ;;
        *)                  fail "naməlum hədəf: $target (linux|mac|android|all)" ;;
    esac
}

main "$@"
