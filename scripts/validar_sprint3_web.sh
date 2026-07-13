#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WEB_DIR="$ROOT_DIR/INDOTEL.Web"
TEST_DIR="$ROOT_DIR/INDOTEL.Web.Tests"
WEB_PROJECT="$WEB_DIR/INDOTEL.WEB.csproj"
TEST_PROJECT="$TEST_DIR/INDOTEL.Web.Tests.csproj"
PUBLISH_DIR="/tmp/indotel-web-sprint3"

printf '\n==> SDK de .NET\n'
dotnet --info

printf '\n==> Limpieza fisica de artefactos\n'
rm -rf \
  "$ROOT_DIR/.vs" \
  "$WEB_DIR/bin" \
  "$WEB_DIR/obj" \
  "$TEST_DIR/bin" \
  "$TEST_DIR/obj" \
  "$TEST_DIR/TestResults" \
  "$PUBLISH_DIR"

printf '\n==> Restauracion\n'
dotnet restore "$WEB_PROJECT" --force
dotnet restore "$TEST_PROJECT" --force

printf '\n==> Compilacion Release\n'
dotnet build "$WEB_PROJECT" --configuration Release --no-restore
dotnet build "$TEST_PROJECT" --configuration Release --no-restore

printf '\n==> Pruebas automaticas\n'
dotnet test "$TEST_PROJECT" --configuration Release --no-build --collect:"XPlat Code Coverage"

printf '\n==> Publicacion de comprobacion\n'
dotnet publish "$WEB_PROJECT" --configuration Release --no-restore --output "$PUBLISH_DIR"

printf '\nSprint 3 validado: restauracion, compilacion, pruebas y publicacion completadas.\n'
printf 'Salida publicada temporalmente en: %s\n' "$PUBLISH_DIR"
