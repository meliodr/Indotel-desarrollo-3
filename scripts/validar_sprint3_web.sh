#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WEB_PROJECT="$ROOT_DIR/INDOTEL.Web/INDOTEL.WEB.csproj"
TEST_PROJECT="$ROOT_DIR/INDOTEL.Web.Tests/INDOTEL.Web.Tests.csproj"
PUBLISH_DIR="/tmp/indotel-web-sprint3"

printf '\n==> SDK de .NET\n'
dotnet --info

printf '\n==> Limpieza\n'
dotnet clean "$WEB_PROJECT" --configuration Release

printf '\n==> Restauracion\n'
dotnet restore "$WEB_PROJECT"
dotnet restore "$TEST_PROJECT"

printf '\n==> Compilacion Release\n'
dotnet build "$WEB_PROJECT" --configuration Release --no-restore
dotnet build "$TEST_PROJECT" --configuration Release --no-restore

printf '\n==> Pruebas automaticas\n'
dotnet test "$TEST_PROJECT" --configuration Release --no-build --collect:"XPlat Code Coverage"

printf '\n==> Publicacion de comprobacion\n'
rm -rf "$PUBLISH_DIR"
dotnet publish "$WEB_PROJECT" --configuration Release --no-restore --output "$PUBLISH_DIR"

printf '\nSprint 3 validado: restauracion, compilacion, pruebas y publicacion completadas.\n'
printf 'Salida publicada temporalmente en: %s\n' "$PUBLISH_DIR"
