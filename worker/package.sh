#!/usr/bin/env bash
#
# Edutor Nodes — platforma paketləri.
#
#   ./package.sh            hamısı (linux, mac, android)
#   ./package.sh linux      Linux: .deb + quraşdırma skriptli tar.gz
#   ./package.sh mac        macOS (osx-arm64 + osx-x64, .app + Install.command)
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

desktop_entry() {
    local exec_path=$1
    cat <<EOF
[Desktop Entry]
Type=Application
Name=Edutor Nodes
Comment=Video konversiya worker-i
Exec="$exec_path"
Icon=edutor-nodes
Terminal=false
Categories=Network;Utility;
EOF
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

    # .deb içində /usr/share altında istifadə olunur; tar.gz-də istifadəçi
    # tərəfindən ~/.local altına quraşdırılır.
    cat > "$stage/install.sh" <<'EOF'
#!/usr/bin/env bash
# Edutor Nodes — cari istifadəçi üçün quraşdırma (~/.local). Sudo lazım deyil.
set -eu

HERE=$(cd "$(dirname "$0")" && pwd)
PREFIX=${PREFIX:-$HOME/.local}
APP=$PREFIX/opt/edutor-nodes

mkdir -p "$APP" "$PREFIX/bin" "$PREFIX/share/applications" \
         "$PREFIX/share/icons/hicolor/256x256/apps"

cp -a "$HERE"/. "$APP"/
ln -sf "$APP/Edutor.Nodes.App" "$PREFIX/bin/edutor-nodes"
cp "$APP/edutor-nodes.png" "$PREFIX/share/icons/hicolor/256x256/apps/" 2>/dev/null || true

cat > "$PREFIX/share/applications/edutor-nodes.desktop" <<DESKTOP
[Desktop Entry]
Type=Application
Name=Edutor Nodes
Comment=Video konversiya worker-i
Exec="$APP/Edutor.Nodes.App"
Icon=edutor-nodes
Terminal=false
Categories=Network;Utility;
DESKTOP

echo "Quraşdırıldı: $APP"
echo "Menyudan və ya 'edutor-nodes' əmri ilə açılır."
echo "İlk açılışdan sonra tətbiq sistem açılışında avtomatik başlayır."

# Komanda PATH-də deyilsə istifadəçiyə xəbər verilir.
case ":$PATH:" in
    *":$PREFIX/bin:"*) ;;
    *) echo "Qeyd: $PREFIX/bin PATH-də deyil — ~/.profile faylına əlavə edin." ;;
esac
EOF

    cat > "$stage/uninstall.sh" <<'EOF'
#!/usr/bin/env bash
# Quraşdırılmış Edutor Nodes-u silir (tənzimləmələr və keş qalır).
set -eu

PREFIX=${PREFIX:-$HOME/.local}

rm -rf "$PREFIX/opt/edutor-nodes"
rm -f "$PREFIX/bin/edutor-nodes" \
      "$PREFIX/share/applications/edutor-nodes.desktop" \
      "$PREFIX/share/icons/hicolor/256x256/apps/edutor-nodes.png" \
      "$HOME/.config/autostart/edutor-nodes.desktop"

echo "Silindi. Tənzimləmələr: ~/.config/EdutorNodes"
EOF

    chmod +x "$stage/install.sh" "$stage/uninstall.sh"

    cat > "$stage/README.txt" <<'EOF'
Edutor Nodes (Linux)

QURAŞDIRMA (tövsiyə olunur):
  ./install.sh
  -> ~/.local/opt/edutor-nodes altına yazır, menyu qeydi yaradır.

və ya yalnız bu qovluqdan işlətmək üçün:
  ./Edutor.Nodes.App

SİLİNMƏ:
  ./uninstall.sh

ARXA PLAN:
Pəncərəni bağlamaq tətbiqi dayandırmır — treyə keçir və konversiya davam edir.
Tam çıxış üçün trey menyusunda "Çıxış" seçin. Tətbiq ilk açılışdan sonra
sistem açılışında avtomatik (gizli) başlayır; bunu Tənzimləmələr → Arxa plan
bölməsindən söndürmək olar.

ffmpeg sistemdə olmalıdır:  sudo apt install ffmpeg   (və ya distro ekvivalenti)
ffmpeg başqa yerdədirsə, Tənzimləmələr bölməsində yolunu göstərin.
EOF

    tar -czf "$DIST/edutor-nodes-linux-x64-$VERSION.tar.gz" -C "$DIST" edutor-nodes-linux-x64
    log "linux → $DIST/edutor-nodes-linux-x64-$VERSION.tar.gz"

    build_deb "$stage"
    rm -rf "$stage"
}

# dpkg-deb olmayan sistemlərdə (məs. Arch) .deb ar + tar ilə əl ilə yığılır.
build_deb() {
    local src=$1
    local arch=amd64
    local deb=$DIST/edutor-nodes_${VERSION}_${arch}.deb
    local work=$ROOT/obj/deb

    rm -rf "$work"
    mkdir -p "$work/data/opt/edutor-nodes" \
             "$work/data/usr/bin" \
             "$work/data/usr/share/applications" \
             "$work/data/usr/share/icons/hicolor/256x256/apps" \
             "$work/control"

    cp -a "$src"/. "$work/data/opt/edutor-nodes"/
    rm -f "$work/data/opt/edutor-nodes/install.sh" \
          "$work/data/opt/edutor-nodes/uninstall.sh" \
          "$work/data/opt/edutor-nodes/README.txt"

    ln -s /opt/edutor-nodes/Edutor.Nodes.App "$work/data/usr/bin/edutor-nodes"
    desktop_entry /usr/bin/edutor-nodes > "$work/data/usr/share/applications/edutor-nodes.desktop"
    cp "$src/edutor-nodes.png" "$work/data/usr/share/icons/hicolor/256x256/apps/" 2>/dev/null || true

    local size
    size=$(du -sk --exclude=DEBIAN "$work/data" | cut -f1)

    cat > "$work/control/control" <<EOF
Package: edutor-nodes
Version: $VERSION
Section: net
Priority: optional
Architecture: $arch
Installed-Size: $size
Maintainer: Edutor <support@bubora.online>
Depends: libx11-6, libice6, libsm6, libfontconfig1, libxrandr2, libxi6, libgl1
Recommends: ffmpeg
Description: Edutor Nodes - video conversion worker
 Distributes HLS video encoding jobs from the Edutor API to this device.
 Runs in the system tray and keeps encoding in the background.
EOF

    ( cd "$work/control" && tar --owner=0 --group=0 -czf "$work/control.tar.gz" . )
    ( cd "$work/data" && tar --owner=0 --group=0 -czf "$work/data.tar.gz" . )
    echo "2.0" > "$work/debian-binary"

    rm -f "$deb"
    ( cd "$work" && ar r "$deb" debian-binary control.tar.gz data.tar.gz 2>/dev/null )

    log "linux → $deb"
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
        local iconset_dir=$ROOT/obj/AppIcon.iconset
        rm -rf "$iconset_dir"; mkdir -p "$iconset_dir"
        for size in 16 32 128 256 512; do
            magick "$ICON" -resize ${size}x${size} "$iconset_dir/icon_${size}x${size}.png"
            magick "$ICON" -resize $((size * 2))x$((size * 2)) "$iconset_dir/icon_${size}x${size}@2x.png"
        done
        if command -v iconutil >/dev/null; then
            iconutil -c icns "$iconset_dir" -o "$app/Edutor Nodes.app/Contents/Resources/AppIcon.icns"
        else
            cp "$iconset_dir/icon_256x256.png" "$app/Edutor Nodes.app/Contents/Resources/AppIcon.png"
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
    <key>LSBackgroundOnly</key>        <false/>
    <key>NSHighResolutionCapable</key> <true/>
</dict>
</plist>
EOF

    cat > "$app/Install.command" <<'EOF'
#!/bin/bash
# İki kliklə quraşdırma: tətbiqi /Applications qovluğuna köçürür və açır.
set -e

HERE="$(cd "$(dirname "$0")" && pwd)"
DEST="/Applications/Edutor Nodes.app"

echo "Edutor Nodes quraşdırılır…"

rm -rf "$DEST"
cp -R "$HERE/Edutor Nodes.app" "$DEST"

# Paket imzalanmadığı üçün Gatekeeper karantin işarəsi silinir.
xattr -dr com.apple.quarantine "$DEST" 2>/dev/null || true

echo "Quraşdırıldı: $DEST"
echo "Tətbiq ilk açılışdan sonra sistem açılışında arxa planda başlayır."
echo

open "$DEST"
EOF

    chmod +x "$app/Install.command"

    cat > "$app/README.txt" <<'EOF'
Edutor Nodes (macOS)

QURAŞDIRMA:
  "Install.command" faylına iki klik edin.
  -> Tətbiq /Applications qovluğuna köçürülür və açılır.

  Terminaldan:  ./Install.command

Əl ilə quraşdırma:
  "Edutor Nodes.app" faylını /Applications qovluğuna sürüşdürün.
  İlk açılışda Gatekeeper xəbərdarlıq verə bilər (paket imzalanmayıb):
  sağ klik → Open, və ya:
  xattr -dr com.apple.quarantine "/Applications/Edutor Nodes.app"

ARXA PLAN:
Pəncərəni bağlamaq tətbiqi dayandırmır — treyə keçir və konversiya davam edir.
Tam çıxış üçün trey menyusunda "Çıxış" seçin. Tətbiq ilk açılışdan sonra
sistem açılışında avtomatik (gizli) başlayır; Tənzimləmələr → Arxa plan.

ffmpeg lazımdır:  brew install ffmpeg
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
