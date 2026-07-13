#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CAJA_PROJECT="$ROOT_DIR/INDOTEL_CAJA(REAL)/INDOTEL_CAJA(REAL).csproj"
TEST_PROJECT="$ROOT_DIR/INDOTEL_CAJA.Tests/INDOTEL_CAJA.Tests.csproj"
PUBLISH_DIR="/tmp/indotel-caja-sprint4-win-x64"

printf '\n==> SDK de .NET\n'
dotnet --info

printf '\n==> Limpieza fisica de artefactos\n'
find "$ROOT_DIR/INDOTEL_CAJA(REAL)" "$ROOT_DIR/INDOTEL_CAJA.Tests" \
  -type d \( -name bin -o -name obj -o -name TestResults \) \
  -prune -exec rm -rf {} + 2>/dev/null || true

printf '\n==> Restauracion\n'
dotnet restore "$CAJA_PROJECT"
dotnet restore "$TEST_PROJECT"

printf '\n==> Compilacion Release de Caja\n'
dotnet build "$CAJA_PROJECT" --configuration Release --no-restore

printf '\n==> Pruebas automaticas de infraestructura\n'
dotnet test "$TEST_PROJECT" \
  --configuration Release \
  --no-restore \
  --collect:"XPlat Code Coverage"

printf '\n==> Publicacion win-x64 de comprobacion\n'
rm -rf "$PUBLISH_DIR"
dotnet publish "$CAJA_PROJECT" \
  --configuration Release \
  --runtime win-x64 \
  --self-contained false \
  --no-restore \
  --output "$PUBLISH_DIR"

printf '\nSprint 4 validado: restauracion, compilacion, pruebas y publicacion completadas.\n'
printf 'Caja requiere Windows para su ejecucion. Artefacto temporal: %s\n' "$PUBLISH_DIR"
