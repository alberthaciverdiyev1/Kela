#!/usr/bin/env bash
#
# Edutor deployment script (run on the server as root).
#
#   ./deploy.sh                 pull main, build api/admin/web, publish a new release, switch, restart
#   ./deploy.sh --skip-pull     deploy the current checkout in $SRC_DIR
#   ./deploy.sh --rollback      switch back to the previous release
#   ./deploy.sh --rollback <r>  switch back to a specific release directory name
#   ./deploy.sh --list          list available releases
#
# Every deploy creates a database backup first and a new versioned directory;
# `current` is a symlink that is switched atomically. Anything failing after the
# switch reverts to the previous release.
set -Eeuo pipefail

APP_ROOT=${APP_ROOT:-/var/www/edutor}
SRC_DIR=$APP_ROOT/src
RELEASES=$APP_ROOT/releases
CURRENT=$APP_ROOT/current
SHARED=$APP_ROOT/shared
BACKUPS=$SHARED/backups
ENV_DIR=${ENV_DIR:-/etc/edutor}
API_ENV=$ENV_DIR/api.env
ADMIN_ENV=$ENV_DIR/admin.env
REPO_URL=${REPO_URL:-https://github.com/alberthaciverdiyev1/Kela.git}
BRANCH=${BRANCH:-main}
KEEP_RELEASES=${KEEP_RELEASES:-3}
KEEP_BACKUPS=${KEEP_BACKUPS:-10}
API_PORT=${API_PORT:-5080}
ADMIN_PORT=${ADMIN_PORT:-5081}
WEB_API_BASE_URL=${WEB_API_BASE_URL:-https://edutor-api.foxsoft.online}

export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

log()  { printf '\033[1;34m==>\033[0m %s\n' "$*"; }
warn() { printf '\033[1;33mWARN:\033[0m %s\n' "$*"; }
fail() { printf '\033[1;31mERROR:\033[0m %s\n' "$*" >&2; exit 1; }

[[ $EUID -eq 0 ]] || fail "run as root"

# One deploy at a time.
exec 9>/run/edutor-deploy.lock
flock -n 9 || fail "another deploy is already running"

list_releases() {
    ls -1dt "$RELEASES"/*/ 2>/dev/null | sed 's:/$::' | xargs -r -n1 basename
}

previous_release() {
    local cur
    cur=$(basename "$(readlink -f "$CURRENT" 2>/dev/null || true)")
    list_releases | grep -vx "$cur" | head -1
}

wait_http() {
    local url=$1 timeout=${2:-90} i=0
    until curl -sf -m 5 -o /dev/null "$url"; do
        i=$((i + 2))
        (( i < timeout )) || return 1
        sleep 2
    done
}

restart_services() {
    systemctl restart edutor-api 9>&- || return 1
    wait_http "http://127.0.0.1:$API_PORT/health" 120 || return 1
    systemctl restart edutor-admin 9>&- || return 1
    wait_http "http://127.0.0.1:$ADMIN_PORT/health" 120 || return 1
}

switch_to() {
    local release=$1
    [[ -d $release ]] || fail "release not found: $release"
    chmod -R a+rX "$release"
    ln -sfn "$release" "$CURRENT.new"
    mv -Tf "$CURRENT.new" "$CURRENT"
}

load_db_env() {
    local cs part key value
    cs=$(grep -E '^ConnectionStrings__Postgres=' "$API_ENV" | tail -1 | cut -d= -f2-)
    [[ -n $cs ]] || fail "ConnectionStrings__Postgres not found in $API_ENV"

    DB_HOST=127.0.0.1 DB_PORT=5432 DB_USER= DB_PASS= DB_NAME=
    while IFS= read -r part; do
        key=$(echo "${part%%=*}" | xargs)
        value=${part#*=}
        case ${key,,} in
            host) DB_HOST=$value ;;
            port) DB_PORT=$value ;;
            database) DB_NAME=$value ;;
            username) DB_USER=$value ;;
            password) DB_PASS=$value ;;
        esac
    done < <(echo "$cs" | tr ';' '\n')
    [[ -n $DB_NAME && -n $DB_USER ]] || fail "cannot parse the Postgres connection string"
}

backup_database() {
    local sha=$1 file
    load_db_env
    mkdir -p "$BACKUPS"
    file=$BACKUPS/edutor-$(date -u +%Y%m%d-%H%M%S)-$sha.dump
    log "database backup -> $file"
    if ! PGPASSWORD=$DB_PASS pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -Fc -f "$file" "$DB_NAME"; then
        rm -f "$file"
        fail "database backup failed — deploy aborted"
    fi
    [[ -s $file ]] || fail "database backup is empty — deploy aborted"
    ls -1t "$BACKUPS"/edutor-*.dump | tail -n +$((KEEP_BACKUPS + 1)) | xargs -r rm -f
    echo "$file"
}

prune_releases() {
    local cur
    cur=$(readlink -f "$CURRENT" 2>/dev/null || true)
    list_releases | tail -n +$((KEEP_RELEASES + 1)) | while read -r name; do
        [[ $RELEASES/$name == "$cur" ]] && continue
        log "pruning old release $name"
        rm -rf "$RELEASES/$name"
    done
}

do_rollback() {
    local target=${1:-}
    if [[ -z $target ]]; then
        target=$(previous_release)
        [[ -n $target ]] || fail "no previous release to roll back to"
        target=$RELEASES/$target
    elif [[ $target != /* ]]; then
        target=$RELEASES/$target
    fi
    log "rolling back to $(basename "$target")"
    switch_to "$target"
    restart_services || fail "rollback started the previous release but its health check failed"
    log "rollback complete"
}

case ${1:-} in
    --list)
        list_releases
        exit 0
        ;;
    --rollback)
        do_rollback "${2:-}"
        exit 0
        ;;
esac

SKIP_PULL=0
for arg in "$@"; do
    case $arg in
        --skip-pull) SKIP_PULL=1 ;;
        *) fail "unknown argument: $arg" ;;
    esac
done

[[ -d $SRC_DIR/.git ]] || fail "$SRC_DIR is not a git checkout"
[[ -f $API_ENV && -f $ADMIN_ENV ]] || fail "environment files missing in $ENV_DIR"

if (( SKIP_PULL == 0 )); then
    log "pulling $BRANCH from GitHub"
    git -C "$SRC_DIR" fetch --prune origin "$BRANCH"
    git -C "$SRC_DIR" checkout -q "$BRANCH"
    git -C "$SRC_DIR" reset -q --hard "origin/$BRANCH"
fi

SHA=$(git -C "$SRC_DIR" rev-parse --short HEAD)
RELEASE=$RELEASES/$(date -u +%Y%m%d-%H%M%S)-$SHA
[[ -e $RELEASE ]] && fail "release directory already exists: $RELEASE"

mkdir -p "$RELEASES"
free_kb=$(df -Pk "$APP_ROOT" | awk 'NR==2 {print $4}')
(( free_kb > 1024 * 1024 )) || warn "less than 1G free on $APP_ROOT"

backup_database "$SHA"
PREVIOUS=$(readlink -f "$CURRENT" || true)
[[ -n $PREVIOUS && -d $PREVIOUS ]] || PREVIOUS=

build_release() {
    log "admin CSS"
    (cd "$SRC_DIR/admin/Edutor.Admin" && npm ci --no-audit --no-fund --silent && npm run --silent css:build)

    # The MAUI/WASM shared project has no package.json; it reuses the admin Tailwind CLI
    # and needs a node_modules next to the project so `@import "tailwindcss"` resolves.
    log "web CSS"
    [ -e "$SRC_DIR/web/Edutor.App.Shared/node_modules/tailwindcss" ] || \
        ln -sfn "$SRC_DIR/admin/Edutor.Admin/node_modules" "$SRC_DIR/web/Edutor.App.Shared/node_modules"
    (cd "$SRC_DIR/web/Edutor.App.Shared" && \
        "$SRC_DIR/admin/Edutor.Admin/node_modules/.bin/tailwindcss" \
        -i ./wwwroot/css/input.css -o ./wwwroot/css/app.css --minify)

    log "publish api"
    dotnet publish "$SRC_DIR/api/Edutor.Api" -c Release -o "$RELEASE/api" --nologo -v q

    log "publish admin"
    dotnet publish "$SRC_DIR/admin/Edutor.Admin" -c Release -o "$RELEASE/admin" --nologo -v q

    log "publish web (WASM)"
    dotnet publish "$SRC_DIR/web/Edutor.App.Web" -c Release -o "$RELEASE/web" --nologo -v q

    local css=$RELEASE/web/wwwroot/_content/Edutor.App.Shared/css/app.css
    [[ -s $css ]] || fail "web CSS missing from the publish output"

    # Cloudflare and browsers cache static files by URL; without a content hash a
    # stale or previously-missing app.css keeps being served after a deploy.
    local css_hash
    css_hash=$(sha256sum "$css" | cut -c1-10)
    sed -i "s|_content/Edutor.App.Shared/css/app.css\"|_content/Edutor.App.Shared/css/app.css?v=$css_hash\"|" \
        "$RELEASE/web/wwwroot/index.html"
    rm -f "$RELEASE/web/wwwroot/index.html.br" "$RELEASE/web/wwwroot/index.html.gz"

    cat > "$RELEASE/web/wwwroot/appsettings.json" <<EOF
{
  "Api": {
    "BaseUrl": "$WEB_API_BASE_URL"
  }
}
EOF

    printf '%s\n' "$SHA $(date -u +%FT%TZ)" > "$RELEASE/RELEASE_INFO"

    # The build leaves bin/obj behind; the release holds everything the hosts need.
    find "$SRC_DIR" -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + 2>/dev/null || true
}

log "building release $(basename "$RELEASE") from $SHA"
mkdir -p "$RELEASE"
# fd 9 carries the deploy lock; drop it in the subshell so long-lived build
# children (VBCSCompiler) do not inherit it and keep the lock forever.
( set -Eeuo pipefail; exec 9>&-; build_release ) || {
    rm -rf "$RELEASE"
    fail "build failed; nothing was switched"
}

log "switching current -> $(basename "$RELEASE")"
switch_to "$RELEASE"

if ! restart_services; then
    warn "health check failed after switching to $(basename "$RELEASE")"
    if [[ -n $PREVIOUS ]]; then
        log "reverting to $(basename "$PREVIOUS")"
        switch_to "$PREVIOUS"
        restart_services || fail "previous release did not come back healthy — manual intervention required"
        journalctl -u edutor-api -u edutor-admin -n 60 --no-pager || true
    fi
    fail "deploy failed; previous release restored"
fi

prune_releases
log "deployed $(basename "$RELEASE")"
curl -s "http://127.0.0.1:$API_PORT/health"; echo
ls -1t "$BACKUPS"/edutor-*.dump 2>/dev/null | head -1
