#!/usr/bin/env bash
# Run API, Web and Admin locally against the databases in scripts/local.env.
#
#   ./scripts/run-local.sh start [api|web|admin ...]   # default: all three, in the background
#   ./scripts/run-local.sh stop  [api|web|admin ...]
#   ./scripts/run-local.sh status
#   ./scripts/run-local.sh logs  <api|web|admin>        # tail -f
set -euo pipefail

repo="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
envfile="$repo/scripts/local.env"
rundir="$repo/logs/local"
mkdir -p "$rundir"

[ -f "$envfile" ] || { echo "Missing $envfile — copy scripts/local.env.example to scripts/local.env and fill it in." >&2; exit 1; }
set -a; # export everything sourced
# shellcheck disable=SC1090
source "$envfile"
set +a

declare -A proj=([api]=IAMS.Api [web]=IAMS.Web [admin]=IAMS.Admin)
declare -A url=([api]="${API_URL:-http://localhost:5175}" [web]="${WEB_URL:-http://localhost:5273}" [admin]="${ADMIN_URL:-http://localhost:2554}")

cmd="${1:-start}"; shift || true
apps=("$@"); [ ${#apps[@]} -eq 0 ] && apps=(api web admin)

pidfile() { echo "$rundir/$1.pid"; }
running() { local f; f="$(pidfile "$1")"; [ -f "$f" ] && kill -0 "$(cat "$f")" 2>/dev/null; }

start_one() {
  local app="$1" name="${proj[$1]}"
  if running "$app"; then echo "$app already running (pid $(cat "$(pidfile "$app")"))"; return; fi
  echo "==> starting $name on ${url[$app]}  (log: logs/local/$app.log)"
  (
    export ASPNETCORE_ENVIRONMENT=Development
    export ASPNETCORE_URLS="${url[$app]}"
    # Web and Admin talk to the local API over HTTP
    [ "$app" != api ] && export ApiSettings__BaseUrl="${API_URL:-http://localhost:5175}"
    # Per-app override, e.g. ADMIN_DefaultConnection (Admin's identity store is a different DB)
    local override_var="${app^^}_DefaultConnection"
    [ -n "${!override_var:-}" ] && export ConnectionStrings__DefaultConnection="${!override_var}"
    cd "$repo"
    nohup dotnet run --project "src/$name/$name.csproj" --no-launch-profile >"$rundir/$app.log" 2>&1 &
    echo $! >"$(pidfile "$app")"
  )
}

stop_one() {
  local app="$1" f; f="$(pidfile "$app")"
  if running "$app"; then
    echo "==> stopping $app (pid $(cat "$f"))"
    pkill -TERM -P "$(cat "$f")" 2>/dev/null || true   # dotnet run spawns the app as a child
    kill -TERM "$(cat "$f")" 2>/dev/null || true
  fi
  rm -f "$f"
}

case "$cmd" in
  start)
    # API first: it owns master DB migrations; Web/Admin need it up to log in.
    for app in api web admin; do for want in "${apps[@]}"; do [ "$app" = "$want" ] && start_one "$app"; done; done
    echo
    echo "Open:  Web   http://${TENANT:-test}.localhost:${WEB_URL##*:}   (tenant = subdomain)"
    echo "       Admin ${url[admin]}"
    echo "       API   ${url[api]}/swagger"
    echo "Follow a log: ./scripts/run-local.sh logs api" ;;
  stop)   for app in "${apps[@]}"; do stop_one "$app"; done ;;
  status) for app in api web admin; do if running "$app"; then echo "$app: running (pid $(cat "$(pidfile "$app")")) ${url[$app]}"; else echo "$app: stopped"; fi; done ;;
  logs)   exec tail -n 50 -f "$rundir/${apps[0]}.log" ;;
  *) echo "usage: $0 {start|stop|status|logs} [api|web|admin ...]" >&2; exit 1 ;;
esac
