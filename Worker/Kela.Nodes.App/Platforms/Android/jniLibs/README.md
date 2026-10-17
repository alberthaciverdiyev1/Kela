# Android üçün ffmpeg

`arm64-v8a/libffmpeg.so` və `libffprobe.so` — ffmpeg 7.1.1-in Android arm64 üçün
statik quruluşu (NDK r29, API 26).

**Niyə `lib*.so` adı ilə?** Android 10+ tətbiqin öz yazıla bilən qovluğundan ikili
fayl icra etməyi bloklayır. Yalnız APK-nın native kitabxana qovluğundan icra
mümkündür — oraya isə ancaq `lib*.so` adlı fayllar düşür. Ona görə `ffmpeg`
ikilisi `libffmpeg.so` kimi paketlənir.

## Yenidən qurmaq

```bash
# NDK (bir dəfə)
sdkmanager --install "ndk;29.0.14206865"

# ffmpeg + x264
/tmp/build-ffmpeg-android.sh        # skript repoda deyil, aşağıdaki konfiqurasiya ilə
```

Konfiqurasiya (`--disable-avdevice` **işlədilməz** — `lavfi` benchmark üçün lazımdır):

```
--target-os=android --arch=aarch64 --enable-cross-compile
--enable-static --disable-shared --enable-gpl --enable-libx264
--enable-jni --enable-mediacodec --enable-avdevice --enable-indev=lavfi
--sysroot=<ndk>/toolchains/llvm/prebuilt/linux-x86_64/sysroot
--cc=<ndk>/.../bin/aarch64-linux-android26-clang
```

Yoxlama (cihazda):

```bash
LIB=$(adb shell pm path com.kela.nodes | sed 's/package://;s|/base.apk|/lib/arm64|')
adb shell "$LIB/libffmpeg.so -encoders | grep -E 'h264_mediacodec|libx264'"
```

Hər ikisi görünməlidir: `h264_mediacodec` telefonun apparat encoder-ı, `libx264` isə
CPU ehtiyatıdır.
