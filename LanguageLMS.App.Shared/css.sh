  #!/usr/bin/env bash
# Tailwind build (MVC-dəki mövcud CLI-dən istifadə edir — əlavə npm install lazım deyil)
set -e
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TAILWIND="$DIR/../LanguageLMS.Mvc/node_modules/.bin/tailwindcss"
if [ ! -x "$TAILWIND" ]; then
  echo "tailwind CLI tapılmadı: $TAILWIND (əvvəlcə LanguageLMS.Mvc-də 'npm install')" >&2
  exit 1
fi
"$TAILWIND" -i "$DIR/Styles/app.css" -o "$DIR/wwwroot/css/app.css" --minify
echo "app.css hazır: $(wc -c < "$DIR/wwwroot/css/app.css") bayt"
