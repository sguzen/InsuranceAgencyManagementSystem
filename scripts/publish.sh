#!/usr/bin/env bash
# Publish IAMS apps to ./publish/<app> (framework-dependent, Release) and zip them
# for copying to the IIS server. Replaces Visual Studio "Publish to folder".
#
#   ./scripts/publish.sh              # api web admin
#   ./scripts/publish.sh api admin    # subset
#
# Server-side config is NOT shipped: appsettings*.json are moved to
# publish/<app>/_config-reference/ so copying the app folder over the existing
# deployment never overwrites the server's connection strings, ApiSettings or
# Hosting:PathBase. Diff them against the server copies if new keys were added.
set -euo pipefail

repo="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
out="$repo/publish"
apps=("$@"); [ ${#apps[@]} -eq 0 ] && apps=(api web admin)
sha="$(git -C "$repo" rev-parse --short HEAD)"
stamp="$(date +%Y%m%d-%H%M)"

declare -A proj=([api]=IAMS.Api [web]=IAMS.Web [admin]=IAMS.Admin)

for app in "${apps[@]}"; do
  name="${proj[$app]:-}"
  [ -n "$name" ] || { echo "unknown app '$app' (use: api web admin)" >&2; exit 1; }
  dest="$out/$app"
  echo "==> Publishing $name -> $dest"
  rm -rf "$dest"
  dotnet publish "$repo/src/$name/$name.csproj" -c Release -o "$dest" --nologo -v q

  mkdir -p "$dest/_config-reference"
  shopt -s nullglob
  for f in "$dest"/appsettings*.json; do mv "$f" "$dest/_config-reference/"; done
  rm -f "$dest"/_config-reference/appsettings.Development*.json "$dest"/_config-reference/*.example.json
  shopt -u nullglob

  printf 'app=%s\ncommit=%s\nbranch=%s\npublished=%s\n' \
    "$name" "$sha" "$(git -C "$repo" rev-parse --abbrev-ref HEAD)" "$(date -Is)" > "$dest/VERSION.txt"

  zip="$out/iams-$app-$sha-$stamp.zip"
  rm -f "$zip"
  (cd "$dest" && zip -qr "$zip" .)
  echo "    $(du -sh "$dest" | cut -f1) folder, zip: $(basename "$zip")"
done

echo
echo "Done. Copy publish/<app>/ (or the zip) over the server folder; leave the server's appsettings*.json in place."
echo "If the server's web.config was customised (e.g. ASPNETCORE_ENVIRONMENT), keep the server copy too."
echo "Deploy order: api first (runs master DB migrations on start), then admin, then web."
